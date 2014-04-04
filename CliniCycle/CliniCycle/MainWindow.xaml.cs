using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit;
using Microsoft.Kinect.Toolkit.Controls;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Net.Sockets;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;

namespace CliniCycle
{
    /// <summary>
    /// Interaction logic for ClinicianWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        int patientNum = 0;

        private KinectSensorChooser sensorChooser;
        ColorImageFormat imageFormat = ColorImageFormat.RgbResolution640x480Fps30;

        private WriteableBitmap outputImage;
        private WriteableBitmap inputImage;
        private WriteableBitmap bigOutputImage;
        private byte[] pixels = new byte[0];

        //for sockets
        public Socket socketClient;

        private AsyncCallback socketBioWorkerCallback;
        public Socket socketBioListener;
        public Socket bioSocketWorker;

        private AsyncCallback socketWorkerCallback;
        public Socket socketListener;
        public Socket socketWorker;

        // Patient's names for now
        String p1, p2, p3, p4, p5, p6;

        public MainWindow()
        {
            this.InitializeComponent();
            Loaded += OnLoaded;

            InitializeBioSockets();

            // Insert code required on object creation below this point.

            CreateSocketConnection();

            InitializeSockets();
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            this.sensorChooser = new KinectSensorChooser();
            this.sensorChooser.KinectChanged += sensorChooser_KinectChanged;
            this.sensorChooserUi.KinectSensorChooser = this.sensorChooser;
            //this.sensorChooser.Start();

            // Bind the sensor chooser's current sensor to the KinectRegion
            var regionSensorBinding = new Binding("Kinect") { Source = this.sensorChooser };
            BindingOperations.SetBinding(this.kinectRegion, KinectRegion.KinectSensorProperty, regionSensorBinding);
        }

        /// <summary>
        /// Called when the KinectSensorChooser gets a new sensor
        /// </summary>
        /// <param name="sender">sender of the event</param>
        /// <param name="e">event arguments</param>
        void sensorChooser_KinectChanged(object sender, KinectChangedEventArgs e)
        {

            //MessageBox.Show(e.NewSensor == null ? "No Kinect" : e.NewSensor.Status.ToString());

            if (e.OldSensor != null)
            {
                try
                {
                    e.OldSensor.DepthStream.Range = DepthRange.Default;
                    e.OldSensor.SkeletonStream.EnableTrackingInNearRange = false;
                    e.OldSensor.DepthStream.Disable();
                    e.OldSensor.SkeletonStream.Disable();
                    e.OldSensor.ColorStream.Disable();
                }
                catch (InvalidOperationException)
                {
                    // KinectSensor might enter an invalid state while enabling/disabling streams or stream features.
                    // E.g.: sensor might be abruptly unplugged.

                }
            }

            if (e.NewSensor != null)
            {
                try
                {
                    e.NewSensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
                    e.NewSensor.SkeletonStream.Enable();
                    e.NewSensor.ColorStream.Enable(imageFormat);
                    e.NewSensor.ColorFrameReady += NewSensor_ColorFrameReady;

                    //add the color frames ready handler here

                    try
                    {
                        e.NewSensor.DepthStream.Range = DepthRange.Near;
                        e.NewSensor.SkeletonStream.EnableTrackingInNearRange = true;

                        //seated mode could come in handy on the bike -- uncomment below
                        //e.NewSensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Seated;
                    }
                    catch (InvalidOperationException)
                    {
                        // Non Kinect for Windows devices do not support Near mode, so reset back to default mode.
                        e.NewSensor.DepthStream.Range = DepthRange.Default;
                        e.NewSensor.SkeletonStream.EnableTrackingInNearRange = false;
                    }
                }
                catch (InvalidOperationException)
                {
                    // KinectSensor might enter an invalid state while enabling/disabling streams or stream features.
                    // E.g.: sensor might be abruptly unplugged.
                }
            }
        }

        void NewSensor_ColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            using (ColorImageFrame frame = e.OpenColorImageFrame())
            {

                if (frame == null)
                {
                    return;
                }

                if (pixels.Length == 0)
                {
                    pixels = new byte[frame.PixelDataLength];

                    //get the bitmap of the color frame
                    outputImage = new WriteableBitmap(
                        frame.Width, frame.Height, 96, 96, PixelFormats.Bgr32, null);

                    //show the patient feed video
                    kinectPatientFeed.Source = outputImage;
                }
                //pixels appears to be 1228800 bytes long

                //send the image to the patient
                SocketAsyncEventArgs arg = new SocketAsyncEventArgs();
                arg.SetBuffer(pixels, 0, pixels.Length);
                arg.Completed += arg_Completed;

                try
                {
                    socketClient.SendAsync(arg);
                }
                catch (SocketException se)
                {
                    MessageBox.Show("error: " + se.ToString());
                }

                frame.CopyPixelDataTo(pixels);
                //pixels appears to be 1228800 bytes long
                outputImage.WritePixels(
                    new Int32Rect(0, 0, frame.Width, frame.Height), pixels, frame.Width * 4, 0);

                // Displays the top left video in the large screen when the top left video is clicked, not tested yet since no Kinect!
                //Doesn't actually switch feeds
                if (patientNum == 1)
                {
                    kinectPatientFeedLarge.Source = outputImage;
                }

                 
            };


        }

        void arg_Completed(object sender, SocketAsyncEventArgs e)
        {
            //MessageBox.Show("message sent");
        }

        private void CreateSocketConnection()
        {
            try
            {
                //create a new client socket
                socketClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                //local host for now w/ port 8444
                System.Net.IPAddress remoteIPAddy = System.Net.IPAddress.Parse("127.0.0.1");
                System.Net.IPEndPoint remoteEndPoint = new System.Net.IPEndPoint(remoteIPAddy, 8444);
                socketClient.Connect(remoteEndPoint);
                
            }
            catch (SocketException e)
            {
                MessageBox.Show(e.Message);
            }
        }

        /// <summary>
        /// Sets the connection for biometrics.
        /// </summary>
        /// 
        public class BioSocketPacket
        {
            public System.Net.Sockets.Socket packetSocket;
            public byte[] dataBuffer = new byte[666];
        }

        private void InitializeBioSockets()
        {
            try
            {
                //create listening socket
                socketBioListener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPAddress addy = System.Net.IPAddress.Parse("127.0.0.1");
                IPEndPoint iplocal = new IPEndPoint(addy, 4443);
                //bind to local IP Address
                socketBioListener.Bind(iplocal);
                //start listening -- 4 is max connections queue, can be changed
                socketBioListener.Listen(4);
                //create call back for client connections -- aka maybe recieve video here????
                socketBioListener.BeginAccept(new AsyncCallback(OnBioSocketConnection), null);
            }
            catch (SocketException e)
            {
                //something went wrongpatient
                MessageBox.Show(e.Message);
            }

        }
        private void OnBioSocketConnection(IAsyncResult asyn)
        {
            try
            {
                bioSocketWorker = socketBioListener.EndAccept(asyn);

                WaitForBioData(bioSocketWorker);
            }
            catch (ObjectDisposedException)
            {
                System.Diagnostics.Debugger.Log(0, "1", "\n OnSocketConnection: Socket has been closed\n");
            }
            catch (SocketException e)
            {
                MessageBox.Show(e.Message);
            }

        }
        private void WaitForBioData(System.Net.Sockets.Socket soc)
        {
            try
            {
                if (socketBioWorkerCallback == null)
                {
                    socketBioWorkerCallback = new AsyncCallback(OnBioDataReceived);
                }

                BioSocketPacket sockpkt = new BioSocketPacket();
                sockpkt.packetSocket = soc;
                //start listening for data
                soc.BeginReceive(sockpkt.dataBuffer, 0, sockpkt.dataBuffer.Length, SocketFlags.None, socketBioWorkerCallback, sockpkt);
            }
            catch (SocketException e)
            {
                MessageBox.Show(e.Message);
            }
        }

        private void OnBioDataReceived(IAsyncResult asyn)
        {
            try
            {
                BioSocketPacket socketID = (BioSocketPacket)asyn.AsyncState;
                //end receive
                int end = 0;
                end = socketID.packetSocket.EndReceive(asyn);

                //just getting simple text right now -- needs to be changed
                char[] chars = new char[end + 1];
                System.Text.Decoder d = System.Text.Encoding.UTF8.GetDecoder();
                int len = d.GetChars(socketID.dataBuffer, 0, end, chars, 0);
                System.String tmp = new System.String(chars);
                tmp = Regex.Replace(tmp, @"\t|\n|\r", " ");
                // MessageBox.Show(tmp);
                System.String[] name = tmp.Split('|');
                System.String[] data = name[1].Split(' ');
                p1 = "patient1";
                p2 = "patient2";
                p3 = "patient3";
                p4 = "patient4";
                p5 = "patient5";
                p6 = "patient6";
                // Set the UI in the main thread.
                this.Dispatcher.Invoke((Action)(() =>
                {   
                    if (data[0] == "HR") {
  

                        if (name[0] == p1)
                        {
                            heartRate1.Content = "Heart Rate: " + data[1] + " bpm";
                            if (patientNum == 1)
                            {
                                patientHeartrateBlock.Text = "Heart Rate: " + data[1] + " bpm";
                                if (Int32.Parse(data[1]) > 165) patientHeartrateBlock.Foreground = new SolidColorBrush(Colors.Red);
                                else patientHeartrateBlock.Foreground = new SolidColorBrush(Colors.White);
                            }
                            if (Int32.Parse(data[1]) > 165) heartRate1.Foreground = new SolidColorBrush(Colors.Red);
                            else heartRate1.Foreground = new SolidColorBrush(Colors.Black);
                        }
                        if (name[0] == p2)
                        {
                            heartRate2.Content = "Heart Rate: " + data[1] + " bpm";
                            if (patientNum == 2)
                            {
                                patientHeartrateBlock.Text = "Heart Rate: " + data[1] + " bpm";
                                if (Int32.Parse(data[1]) > 165) patientHeartrateBlock.Foreground = new SolidColorBrush(Colors.Red);
                                else patientHeartrateBlock.Foreground = new SolidColorBrush(Colors.White);
                            }
                            if (Int32.Parse(data[1]) > 165) heartRate2.Foreground = new SolidColorBrush(Colors.Red);
                            else heartRate2.Foreground = new SolidColorBrush(Colors.Black);
                        }
                        if (name[0] == p3)
                        {
                            heartRate3.Content = "Heart Rate: " + data[1] + " bpm";
                            if (patientNum == 3)
                            {
                                patientHeartrateBlock.Text = "Heart Rate: " + data[1] + " bpm";
                                if (Int32.Parse(data[1]) > 165) patientHeartrateBlock.Foreground = new SolidColorBrush(Colors.Red);
                                else patientHeartrateBlock.Foreground = new SolidColorBrush(Colors.White);
                            }
                            if (Int32.Parse(data[1]) > 165) heartRate3.Foreground = new SolidColorBrush(Colors.Red);
                            else heartRate3.Foreground = new SolidColorBrush(Colors.Black);
                            
   
                        }
                        if (name[0] == p4)
                        {
                            heartRate4.Content = "Heart Rate: " + data[1] + " bpm";
                            if (patientNum == 4)
                            {
                                patientHeartrateBlock.Text = "Heart Rate: " + data[1] + " bpm";
                                if (Int32.Parse(data[1]) > 165) patientHeartrateBlock.Foreground = new SolidColorBrush(Colors.Red);
                                else patientHeartrateBlock.Foreground = new SolidColorBrush(Colors.White);
                            }
                            if (Int32.Parse(data[1]) > 165) heartRate4.Foreground = new SolidColorBrush(Colors.Red);
                            else heartRate4.Foreground = new SolidColorBrush(Colors.Black);
                        }
                        if (name[0] == p5)
                        {
                            heartRate5.Content = "Heart Rate: " + data[1] + " bpm";
                            if (patientNum == 5)
                            {
                                patientHeartrateBlock.Text = "Heart Rate: " + data[1] + " bpm";
                                if (Int32.Parse(data[1]) > 165)  patientHeartrateBlock.Foreground = new SolidColorBrush(Colors.Red);
                                else patientHeartrateBlock.Foreground = new SolidColorBrush(Colors.White);
                            }
                            if (Int32.Parse(data[1]) > 165) heartRate5.Foreground = new SolidColorBrush(Colors.Red);
                            else heartRate5.Foreground = new SolidColorBrush(Colors.Black);
                        }
                        if (name[0] == p6)
                        {
                            heartRate6.Content = "Heart Rate: " + data[1] + " bpm";
                            if (patientNum == 6)
                            {
                                patientHeartrateBlock.Text = "Heart Rate: " + data[1] + " bpm";
                                if (Int32.Parse(data[1]) > 165) patientHeartrateBlock.Foreground = new SolidColorBrush(Colors.Red);
                                else patientHeartrateBlock.Foreground = new SolidColorBrush(Colors.White);
                            }
                            if (Int32.Parse(data[1]) > 165) heartRate6.Foreground = new SolidColorBrush(Colors.Red);
                            else
                                heartRate6.Foreground = new SolidColorBrush(Colors.Black);
                        }

                    }

                    else if (data[0] == "OX")
                    {
                        if (name[0] == p1)
                        {
                            sat1.Content = "Oxygen Sat: " + data[1] + "%";
                            if (patientNum == 1) patientOxygenSatBlock.Text = data[1] + "%";
                        }
                        if (name[0] == p2)
                        {
                            sat2.Content = "Oxygen Sat: " + data[1] + "%";
                            if (patientNum == 2) patientOxygenSatBlock.Text = data[1] + "%";
                        }
                        if (name[0] == p3)
                        {
                            sat3.Content = "Oxygen Sat: " + data[1] + "%";
                            if (patientNum == 3) patientOxygenSatBlock.Text = data[1] + "%";
                        }
                        if (name[0] == p4) { 
                            sat4.Content = "Oxygen Sat: " + data[1] + "%";
                            if (patientNum == 4) patientOxygenSatBlock.Text = data[1] + "%";
                        }
                        if (name[0] == p5){
                            sat5.Content = "Oxygen Sat: " + data[1] + "%";
                            if (patientNum == 5) patientOxygenSatBlock.Text = data[1] + "%";
                        }
                        if (name[0] == p6){
                            sat6.Content = "Oxygen Sat: " + data[1] + "%";
                            if (patientNum == 6) patientOxygenSatBlock.Text = data[1] + "%";
                        }
                    }
                }));

                WaitForBioData(bioSocketWorker);
            }
            catch (ObjectDisposedException)
            {
                System.Diagnostics.Debugger.Log(0, "1", "\nOnDataReceived: Socket has been closed\n");
            }
            catch (SocketException e)
            {
                MessageBox.Show(e.Message);
            }

        }

        private void patient1_Click(object sender, RoutedEventArgs e)
        {
            patientIDBlock.Text = p1;
            patientNum = 1;
            patientHeartrateBlock.Text = heartRate1.Content.ToString();
            patientOxygenSatBlock.Text = sat1.Content.ToString();
            //This will need to be changed to switch the video feed
            kinectPatientFeedLarge.Source = outputImage;

        }

        private void patient2_Click(object sender, RoutedEventArgs e)
        {
            patientNum = 2;
            patientIDBlock.Text = p2;
            patientHeartrateBlock.Text = heartRate2.Content.ToString();
            patientOxygenSatBlock.Text = sat2.Content.ToString();
        }

        private void patient3_Click(object sender, RoutedEventArgs e)
        {
            patientNum = 3;
            patientIDBlock.Text = p3;
            patientHeartrateBlock.Text = heartRate3.Content.ToString(); 
            patientOxygenSatBlock.Text = sat3.Content.ToString();
        }

        private void patient4_Click(object sender, RoutedEventArgs e)
        {
            patientNum = 4;
            patientIDBlock.Text = p4;
            patientHeartrateBlock.Text = heartRate4.Content.ToString();
            patientOxygenSatBlock.Text = sat4.Content.ToString();

        }

        private void patient5_Click(object sender, RoutedEventArgs e)
        {
            patientNum = 5;
            patientIDBlock.Text = p5;
            patientHeartrateBlock.Text = heartRate5.Content.ToString();
            patientOxygenSatBlock.Text = sat5.Content.ToString();
        }
        private void patient6_Click(object sender, RoutedEventArgs e)
        {
            patientNum = 6;
            patientIDBlock.Text = p6;
            patientHeartrateBlock.Text = heartRate6.Content.ToString();
            patientOxygenSatBlock.Text = sat6.Content.ToString();
        }

        private void exitProgramButton_Click(object sender, RoutedEventArgs e)
        {
            //Close the kinect properly
            this.sensorChooser.Stop();
            this.Close();

            if (socketBioListener != null)
                socketBioListener.Close();
            if (bioSocketWorker != null)
                bioSocketWorker.Close();
        }

        private void InitializeSockets()
        {
            try
            {
                //create listening socket
                socketListener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPAddress addy = System.Net.IPAddress.Parse("127.0.0.1");
                IPEndPoint iplocal = new IPEndPoint(addy, 8445);
                //bind to local IP Address
                socketListener.Bind(iplocal);
                //start listening -- 4 is max connections queue, can be changed
                socketListener.Listen(4);
                //create call back for client connections -- aka maybe recieve video here????
                socketListener.BeginAccept(new AsyncCallback(OnSocketConnection), null);
            }
            catch (SocketException e)
            {
                //something went wrong
                MessageBox.Show(e.Message);
            }

        }

        private void OnSocketConnection(IAsyncResult asyn)
        {
            try
            {
                socketWorker = socketListener.EndAccept(asyn);

                WaitForData(socketWorker);
            }
            catch (ObjectDisposedException)
            {
                System.Diagnostics.Debugger.Log(0, "1", "\n OnSocketConnection: Socket has been closed\n");
            }
            catch (SocketException e)
            {
                MessageBox.Show(e.Message);
            }

        }

        /// <summary>
        /// The methods below are used to create a socket to read video frames from  
        /// </summary>
        public class SocketPacket
        {
            public System.Net.Sockets.Socket packetSocket;
            public byte[] dataBuffer;

        }

        private void WaitForData(System.Net.Sockets.Socket soc)
        {
            try
            {
                if (socketWorkerCallback == null)
                {
                    socketWorkerCallback = new AsyncCallback(OnDataReceived);
                }

                SocketPacket sockpkt = new SocketPacket();
                sockpkt.packetSocket = soc;

                //need a buffer the size of 1 color frame
                sockpkt.dataBuffer = new byte[1228800];

                //start listening for data
                soc.BeginReceive(sockpkt.dataBuffer, 0, sockpkt.dataBuffer.Length, SocketFlags.None, socketWorkerCallback, sockpkt);
            }
            catch (SocketException e)
            {
                MessageBox.Show(e.Message);
            }
        }

        private void OnDataReceived(IAsyncResult asyn)
        {
            try
            {
                SocketPacket socketID = (SocketPacket)asyn.AsyncState;
                //end receive
                int end = 0;
                end = socketID.packetSocket.EndReceive(asyn);


                //byte[] tmp = new byte[end];
                //MessageBox.Show("A:" + tmp.Length.ToString() + "---" + end.ToString());
                //tmp = socketID.dataBuffer;


                inputImage = new WriteableBitmap(
                     640, 480, 96, 96, PixelFormats.Bgr32, null);

                inputImage.WritePixels(
                     new Int32Rect(0, 0, 640, 480), socketID.dataBuffer, 640 * 4, 0);

                inputImage.Freeze();

                if (inputImage != null)
                {
                    //we are in another thread need -- takes to main UI
                    Dispatcher.Invoke((Action)(() =>
                    {
                        kinectPatientFeed.Source = inputImage;

                    }));
                }

                WaitForData(socketWorker);
            }
            catch (ObjectDisposedException)
            {
                System.Diagnostics.Debugger.Log(0, "1", "\nOnDataReceived: Socket has been closed\n");
            }
            catch (SocketException e)
            {
                MessageBox.Show(e.Message);
            }

        }






    }
}