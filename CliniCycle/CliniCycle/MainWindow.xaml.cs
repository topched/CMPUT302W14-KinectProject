using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit;
using Microsoft.Kinect.Toolkit.Controls;
using System.Windows;
////using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
//using System.Windows.Shapes;
using System.Drawing;
using System.Drawing.Imaging;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Runtime.InteropServices;

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
        private WriteableBitmap inputImage;
        private WriteableBitmap bigOutputImage;
        private byte[] pixels = new byte[0];

        //for sending video stream
        Socket socketClient;

        //for regular sockets
        private AsyncCallback socketWorkerCallback;
        public Socket socketListener;
        public Socket socketWorker;

        public MainWindow()
        {
            this.InitializeComponent();
            Loaded += OnLoaded;

            //used to open socket for sending
            CreateSocketConnection();

            //used to open socket for recieving
            //InitializeSockets();

            //this.sensorChooser = new KinectSensorChooser();
            //this.sensorChooser.KinectChanged += sensorChooser_KinectChanged;
            //this.sensorChooserUi.KinectSensorChooser = this.sensorChooser;
            //this.sensorChooser.Start();

            // Bind the sensor chooser's current sensor to the KinectRegion
            //var regionSensorBinding = new Binding("Kinect") { Source = this.sensorChooser };
            //BindingOperations.SetBinding(this.kinectRegion, KinectRegion.KinectSensorProperty, regionSensorBinding);


        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            this.sensorChooser = new KinectSensorChooser();
            this.sensorChooser.KinectChanged += sensorChooser_KinectChanged;
            this.sensorChooserUi.KinectSensorChooser = this.sensorChooser;
            this.sensorChooser.Start();

            //Bind the sensor chooser's current sensor to the KinectRegion
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

        /// <summary>
        /// Called when a new color frame is ready
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

                    outputImage = new WriteableBitmap(
                    frame.Width, frame.Height, 96, 96, PixelFormats.Bgr32, null);

                    //set the output location
                    kinectPatientFeedLarge.Source = outputImage;

                }
                //send the frame to the patient
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
                outputImage.WritePixels(
                    new Int32Rect(0, 0, frame.Width, frame.Height), pixels, frame.Width * 4, 0);


                //pixels appears to be 1228800 bytes long
           
            };


        }

        //called when the message was sent to the patient
        void arg_Completed(object sender, SocketAsyncEventArgs e)
        {
            //MessageBox.Show("message sent");
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

                //we are in another thread need -- takes to main UI
                Dispatcher.Invoke((Action)(() =>
                {
                    kinectPatientFeedLarge.Source = inputImage;

                }));


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

        /// <summary>
        /// This method is used to create a socket for sending video frames
        /// </summary>
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

        // ------- Button clicks below ---------- //
        private void patient1_Click(object sender, RoutedEventArgs e)
        {
            patientIDBlock.Text = "Satan";
            patientHeartrateBlock.Text = "666";
            patientOxygenSatBlock.Text = "99.99";
            //This will need to be changed to switch the video feed
            //this.kinectPatientFeedLarge.Source = outputImage;

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