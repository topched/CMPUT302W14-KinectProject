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
        private byte[] pixels;

        //for sockets
        Socket socketClient;

        public MainWindow()
        {
            this.InitializeComponent();

            //Initialize the senser chooser and UI
            this.sensorChooser = new KinectSensorChooser();
            this.sensorChooser.KinectChanged += sensorChooser_KinectChanged;
            this.sensorChooserUi.KinectSensorChooser = this.sensorChooser;
            this.sensorChooser.Start();


            // Insert code required on object creation below this point.

            CreateSocketConnection();


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
                frame.CopyPixelDataTo(pixels);

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

            };


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

                //test sending a string
                String tmp = "I am connected";
                byte[] data = System.Text.Encoding.ASCII.GetBytes(tmp);
                socketClient.Send(data);

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