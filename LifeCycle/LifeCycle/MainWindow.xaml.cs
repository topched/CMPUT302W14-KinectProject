using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
//using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
//using System.Windows.Controls;
using System.Windows.Data;
using System.Timers;
//using System.Windows.Documents;
//using System.Windows.Input;
//using System.Windows.Navigation;
//using System.Windows.Shapes;
using System.Runtime.InteropServices;
using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit;
using Microsoft.Kinect.Toolkit.Controls;
using Coding4Fun.Kinect.KinectService.Common;
using Coding4Fun.Kinect.KinectService.Listeners;
using Coding4Fun.Kinect.KinectService.WpfClient;
using ColorImageFormat = Microsoft.Kinect.ColorImageFormat;
using ColorImageFrame = Microsoft.Kinect.ColorImageFrame;
using DepthImageFormat = Microsoft.Kinect.DepthImageFormat;

using System.Diagnostics;
using System.Threading;
//for web sockets
using Fleck;
//for regular sockets
using System.Net;
using System.Net.Sockets;

namespace LifeCycle
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private WriteableBitmap outputImage;
        private byte[] pixels = new byte[0];

        public Socket socketToClinician;
        public Socket socketClient;

        /*public string filePathHR = @"..\..\HR.txt";
        public string filePathOX = @"..\..\OX.txt";
        public string heartLine;
        public string oxygenLine;
        public StreamReader heartRateFile = null;
        public StreamReader oxygenSatFile = null;*/
        
        public int secondsLeft = 2;
        public int minutesLeft = 30;
        public int heartRate = 135;
        public int OX = 1;

        public String bloodPressure;
        Random rnd = new Random();

        public System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
        public bool workoutInProgress = false;
        public Process serverProcess = new Process();

        //for the websockets
        private List<IWebSocketConnection> sockets;

        private AsyncCallback socketBioWorkerCallback;
        public Socket socketBioListener;
        public Socket bioSocketWorker;

        //kinect sensor 
        private KinectSensorChooser sensorChooser;

        //kinect listeners
        private static DepthListener _depthListener;
        private static ColorListener _videoListener;
        private static SkeletonListener _skeletonListener;
        private static AudioListener _audioListener;

        //kinect clients
        private ColorClient _videoClient;
        private AudioClient _audioClient;
        private SkeletonClient _skeletonClient;
        private DepthClient _depthClient;
        

        public MainWindow()
        {
            InitializeComponent();

            //used to start socket server for bioSockets
            InitializeBioSockets();

            //start the kinect
            InitializeKinect();
            
            //updateDisplays();
            minutesLeft = (secondsLeft) / 60;
            timerLabel.Content = minutesLeft + "m " + (secondsLeft - (minutesLeft * 60)) + "s";
            
            // Set up timer ticks.
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 1); // 1 second
            
            //fill the time buttons for the workout time selection
            SolidColorBrush brush = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FF01A2E8"));
            for (int i = 1; i < 7; i++)
            {
                var timeSelectionButton = new KinectCircleButton{
                    Content = i * 10,
                    Height = 90,
                    Foreground = brush
                    
                };

                timeSelectionButton.Click += timeSelectionButton_Click;
                workoutTimeScrollContent.Children.Add(timeSelectionButton);
            }


        }

        private void InitializeKinect()
        {
            this.sensorChooser = new KinectSensorChooser();
            this.sensorChooser.KinectChanged += sensorChooser_KinectChanged;
            this.sensorChooserUi.KinectSensorChooser = this.sensorChooser;
            this.sensorChooser.Start();

            //bind to the UI for control
            // Bind the sensor chooser's current sensor to the KinectRegion
            var regionSensorBinding = new Binding("Kinect") { Source = this.sensorChooser };
            BindingOperations.SetBinding(this.kinectRegion, KinectRegion.KinectSensorProperty, regionSensorBinding);

            //trying to get the video from the client -- this can fail
            _videoClient = new ColorClient();
            _videoClient.ColorFrameReady += _videoClient_ColorFrameReady;
            _videoClient.Connect("192.168.184.9", 4531);

            // Don't show the connect button if you're already connected.
            if (_videoClient.IsConnected)
                connectToButton.Visibility = Visibility.Hidden;

            // Sending video to Clinician
            _videoListener = new ColorListener(this.sensorChooser.Kinect, 4555, ImageFormat.Jpeg);
            _videoListener.Start();
       
            //trying to get the audio from the client -- this can fail
            _audioClient = new AudioClient();
            _audioClient.AudioFrameReady += _audioClient_AudioFrameReady;
            _audioClient.Connect("192.168.184.9", 4533);

            //for sending audio
            _audioListener = new AudioListener(this.sensorChooser.Kinect, 4533);
            _audioListener.Start();

            //for sending depth 
            //_depthListener = new DepthListener(_kinect, 4531);
            //_depthListener.Start();

            //for sending skeleton
            //_skeletonListener = new SkeletonListener(_kinect, 4532);
            //_skeletonListener.Start();
        
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
                    e.NewSensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
                    e.NewSensor.ColorFrameReady += NewSensor_ColorFrameReady;

                    
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
                    this.pixels = new byte[frame.PixelDataLength];
                }
                frame.CopyPixelDataTo(this.pixels);

                outputImage = new WriteableBitmap(frame.Width, frame.Height, 96, 96, PixelFormats.Bgr32, null);

                outputImage.WritePixels(
                    new Int32Rect(0, 0, frame.Width, frame.Height), this.pixels, frame.Width * 4, 0);

                this.kinectPatientFeed.Source = outputImage;

            };

        }

        //called when a video frame from the client is ready
        void _videoClient_ColorFrameReady(object sender, ColorFrameReadyEventArgs e)
        {

            this.kinectClinicianFeed.Source = e.ColorFrame.BitmapImage;
            connectToButton.Visibility = Visibility.Hidden;
                     
        }

        void _audioClient_AudioFrameReady(object sender, AudioFrameReadyEventArgs e)
        {
            //need to handle the audio here
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
                //MessageBox.Show(tmp);
                if (!tmp.Contains('|'))
                {
                    // MessageBox.Show(tmp);
                    tmp = string.Concat("Tiny Tim |", tmp);
                    //MessageBox.Show(tmp);
                }
                System.String[] name = tmp.Split('|');

                System.String[] fakeBP = new String[1] { "BP" };
                System.String[] fakeECG = new String[1] { "ECG" };


                if (name.Length == 2)
                {
                    System.String[] data = name[1].Split(' ');

                    byte[] dataToClinician = System.Text.Encoding.ASCII.GetBytes(tmp);

                    //socketToClinician.Send(dataToClinician);
                    // MessageBox.Show("Got stuff!");

                    // Decide on what encouragement text should be displayed based on heart rate.
                    if (data[0] == "HR")
                    {
                        // Below target zone.
                        this.Dispatcher.Invoke((Action)(() =>
                        {
                            heartRateLabel.Content = data[1];
                        }));
                        /* if (Int32.Parse(data[1]) < maxHR * 0.6)
                         {
                             encourageBrush = new SolidColorBrush(Colors.Orange);
                             encourageText = "Speed up!";
                         }

                         // Above target zone.
                         else if (Int32.Parse(data[1]) > maxHR * 0.8)
                         {
                             encourageBrush = new SolidColorBrush(Colors.Cyan);
                             encourageText = "Slow down!";
                         }

                         // Within target zone.
                         else
                         {
                             encourageBrush = new SolidColorBrush(Colors.MediumVioletRed);
                             encourageText = "Keep it up!";
                         }
                         
                        // Make the changes in the UI thread.
                        this.Dispatcher.Invoke((Action)(() =>
                        {
                            encouragementBox.Foreground = encourageBrush;
                            encouragementBox.Content = encourageText;

                        })); */
                    }

                    // Change the Sats display in the UI thread.
                    else if (data[0] == "OX")
                    {
                        this.Dispatcher.Invoke((Action)(() =>
                        {
                            oxygenSatLabel.Content = data[1] + " %";
                        }));
                    }

                    if (fakeBP[0] == "BP")
                    {
                        bloodPressure = rnd.Next(70, 190) + "/" + rnd.Next(40, 100);
                        this.Dispatcher.Invoke((Action)(() =>
                        {

                           // bloodPressureLabel.Content = bloodPressure;
                        }));
                    }

                    /*Trade trade = new Trade(DateTime.Now, _Random.Next(0, 100));
                    this.Dispatcher.Invoke((Action)(() =>
                        {
                    _source.AppendAsync(Dispatcher, trade);
                        }));*/



                }
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

        /// ------- END BIO SOCKETS ------- ///
      

      

        /// <summary>
        /// Updates displays every second and handles ending the workout if time is up.
        /// </summary>
        /// <param name="o"></param>
        /// <param name="sender"></param>
        public void dispatcherTimer_Tick(object o, EventArgs sender)
        {
            if (secondsLeft > 0)
            {
                --secondsLeft;
                updateDisplays();
            }
            else
            {
                dispatcherTimer.Stop();

                // disable the workout button
                beginWorkoutButton.IsEnabled = false;
                beginWorkoutButton.Content = "Begin Workout";

                // hide encouragement box
                encouragementBox.Visibility = Visibility.Hidden;

                //show optionsButton
                showOptionsButton.Visibility = Visibility.Visible;

                // Congratulate the user.
                //encouragementBox.Content = "Great Job!";
                //encouragementBox.Foreground = new SolidColorBrush(Colors.Gold);
                //MessageBoxResult result = MessageBox.Show("Workout completed - Great job!", "Success!", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                workoutInProgress = false;
            }
        }

        
        public void updateDisplays()
        {
            minutesLeft = (secondsLeft) / 60;
            timerLabel.Content = minutesLeft + "m " + (secondsLeft - (minutesLeft * 60)) + "s";
        }

        /*      All Button Clicks Below                */


        private void beginWorkoutButton_Click(object sender, RoutedEventArgs e)
        {
            if (workoutInProgress == false)
            {
                dispatcherTimer.Start();
                beginWorkoutButton.Content = "Stop Workout";
                workoutInProgress = true;

                //hide the options button during the workout
                showOptionsButton.Visibility = Visibility.Hidden;

                // show the encouragementBox.
                encouragementBox.Visibility = Visibility.Visible;

                //show the timer label
                //timerLabel.Visibility = Visibility.Visible;

            }
            //startWorkoutCountdownLabel.Content = "Ready";
            else
            {
                dispatcherTimer.Stop();
                beginWorkoutButton.Content = "Begin Workout";
                workoutInProgress = false;

                //re-show the options button
                showOptionsButton.Visibility = Visibility.Visible;

                // hide the encouragementBox.
                encouragementBox.Visibility = Visibility.Hidden;

                //hide the timer label
               // timerLabel.Visibility = Visibility.Hidden;
            }

        }

        private void showOptionsButton_Click(object sender, RoutedEventArgs e)
        {
            //hide the start & quit buttons. Show the done button
            beginWorkoutButton.Visibility = Visibility.Hidden;
            exitProgramButton.Visibility = Visibility.Hidden;
            closeOptionsButton.Visibility = Visibility.Visible;
            encouragementBox.Visibility = Visibility.Hidden;

            //show the time label and scroll viewer
            optionsTimeLabel.Visibility = Visibility.Visible;
            selectTimeScrollViewer.Visibility = Visibility.Visible;

        }

        private void closeOptionsButton_Click(object sender, RoutedEventArgs e)
        {

            //hide the done button. Show the start & quit buttons
            beginWorkoutButton.Visibility = Visibility.Visible;
            exitProgramButton.Visibility = Visibility.Visible;
            closeOptionsButton.Visibility = Visibility.Hidden;

            //hide the time label and scroll viewer
            optionsTimeLabel.Visibility = Visibility.Hidden;
            selectTimeScrollViewer.Visibility = Visibility.Hidden;

            // re-enable the workout button
            beginWorkoutButton.IsEnabled = true;

        }

        

        /// <summary>
        /// User presses the exit button to quit the workout - ask user to confirm. If they do, close streams and this window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void exitProgramButton_Click(object sender, RoutedEventArgs e)
        {

                    
            if (_videoListener != null)
            _videoListener.Stop();

            this.Close();

        }

        void timeSelectionButton_Click(object sender, RoutedEventArgs e)
        {

            string[] stuff = sender.ToString().Split(' ');
            minutesLeft = Int32.Parse(stuff[1]);
            secondsLeft = (minutesLeft) * 60;
            timerLabel.Content = minutesLeft + "m " + 0 + "s";
            
        }


        private void connectToButton_Click(object sender, RoutedEventArgs e)
        {

            /*/for sending the video -- future implementation
            _videoListener = new ColorListener(this.sensorChooser.Kinect, 4555, ImageFormat.Jpeg);
            _videoListener.Start(); */

            // Begin receiving video stream
            _videoClient.Connect("192.168.184.9", 4531);
        }

       
    }
}
