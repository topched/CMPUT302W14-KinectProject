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

namespace CliniCycle
{
    /// <summary>
    /// Interaction logic for ClinicianWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private KinectSensorChooser sensorChooser;
        ColorImageFormat imageFormat = ColorImageFormat.RgbResolution640x480Fps30;

        private WriteableBitmap outputImage;
        private WriteableBitmap bigOutputImage;
        private byte[] pixels;

        //for sockets
        Socket socketClient;

        private AsyncCallback socketBioWorkerCallback;
        public Socket socketBioListener;
        public Socket bioSocketWorker;

        // Patient's names for now
        String p1, p2, p3, p4, p5, p6;

        public MainWindow()
        {
            this.InitializeComponent();
            Loaded += OnLoaded;

            InitializeBioSockets();

            // Insert code required on object creation below this point.

            CreateSocketConnection();


        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            this.sensorChooser = new KinectSensorChooser();
            this.sensorChooser.KinectChanged += sensorChooser_KinectChanged;
            this.sensorChooserUi.KinectSensorChooser = this.sensorChooser;
            this.sensorChooser.Start();

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

                this.pixels = new byte[frame.PixelDataLength];
                //MessageBox.Show(frame.PixelDataLength.ToString());
                frame.CopyPixelDataTo(pixels);
                //MessageBox.Show(pixels.Length.ToString());

                //string tmp = pixels.Length.ToString();
                //MessageBox.Show(tmp);

                //pixels appears to be 1228800 bytes long

                //get the bitmap of the color frame
                this.outputImage = new WriteableBitmap(
                    frame.Width, frame.Height, 96, 96, PixelFormats.Bgr32, null);

                this.outputImage.WritePixels(
                    new Int32Rect(0, 0, frame.Width, frame.Height), this.pixels, frame.Width * 4, 0);

                //show the patient feed video
                this.kinectPatientFeed.Source = this.outputImage;

                //send the image to the patient
                //socketClient.Send(this.pixels);
                SocketAsyncEventArgs arg = new SocketAsyncEventArgs();
                arg.SetBuffer(this.pixels, 0, this.pixels.Length);
                arg.Completed += arg_Completed;

                try
                {
                    socketClient.SendAsync(arg);
                }
                catch (SocketException se)
                {
                    MessageBox.Show("error: " + se.ToString());
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
                IPEndPoint iplocal = new IPEndPoint(addy, 4444);
                //bind to local IP Address
                socketBioListener.Bind(iplocal);
                //start listening -- 4 is max connections queue, can be changed
                socketBioListener.Listen(4);
                //create call back for client connections -- aka maybe recieve video here????
                socketBioListener.BeginAccept(new AsyncCallback(OnBioSocketConnection), null);
            }
            catch (SocketException e)
            {
                //something went wrong
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
                System.String[] name = tmp.Split('|');
                
                System.String[] data = name[1].Split(' ');
                p1 = "Atlantic";
                p2 = "Shield";
                p3 = "Arctic";
                p4 = "Plains";
                p5 = "Cordillera";
                p6 = "Great Lakes";
                // Set the UI in the main thread.
                this.Dispatcher.Invoke((Action)(() =>
                {
                    if (data[0] == "HR") {
                        data[1] = data[1].Remove(2);
                        if (name[0] == p1)
                            heartRate1.Content = "Heart Rate: " + data[1] + " bpm";
                        if (name[0] == p2)
                            heartRate2.Content = "Heart Rate: " + data[1] + " bpm";
                        if (name[0] == p3)
                            heartRate3.Content = "Heart Rate: " + data[1] + " bpm";
                        if (name[0] == p4)
                            heartRate4.Content = "Heart Rate: " + data[1] + " bpm";
                        if (name[0] == p5)
                            heartRate5.Content = "Heart Rate: " + data[1] + " bpm";
                        if (name[0] == p6)
                            heartRate6.Content = "Heart Rate: " + data[1] + " bpm";
                    }
                    else if (data[0] == "OX")
                    {
                        if (name[0] == p1)
                            sat1.Content = "Oxygen Sat: " + data[1] + "%";
                        if (name[0] == p2)
                            sat2.Content = "Oxygen Sat: " + data[1] + "%";
                        if (name[0] == p3)
                            sat3.Content = "Oxygen Sat: " + data[1] + "%";
                        if (name[0] == p4)
                            sat4.Content = "Oxygen Sat: " + data[1] + "%";
                        if (name[0] == p5)
                            sat5.Content = "Oxygen Sat: " + data[1] + "%";
                        if (name[0] == p6)
                            sat6.Content = "Oxygen Sat: " + data[1] + "%";
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
            patientIDBlock.Text = "Satan";
            patientHeartrateBlock.Text = "666";
            patientOxygenSatBlock.Text = "99.99";
            //This will need to be changed to switch the video feed
            this.kinectPatientFeedLarge.Source = outputImage;

        }

        private void patient2_Click(object sender, RoutedEventArgs e)
        {
            patientIDBlock.Text = "als;jdf";
            patientHeartrateBlock.Text = "3656";
            patientOxygenSatBlock.Text = "80";
        }

        private void patient3_Click(object sender, RoutedEventArgs e)
        {
            patientIDBlock.Text = "khk";
            patientHeartrateBlock.Text = "444";
            patientOxygenSatBlock.Text = "6546";
        }

        private void patient4_Click(object sender, RoutedEventArgs e)
        {
            patientIDBlock.Text = "Kyle";
            patientHeartrateBlock.Text = "57";
            patientOxygenSatBlock.Text = "98";
        }





    }
}