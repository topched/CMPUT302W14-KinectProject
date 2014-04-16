#region Using declarations
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit;
using Microsoft.Kinect.Toolkit.Controls;

using Coding4Fun.Kinect.KinectService.Common;
using Coding4Fun.Kinect.KinectService.Listeners;
using Coding4Fun.Kinect.KinectService.WpfClient;
using ColorImageFormat = Microsoft.Kinect.ColorImageFormat;
using DepthImageFormat = Microsoft.Kinect.DepthImageFormat;
using ColorImageFrame = Microsoft.Kinect.ColorImageFrame;

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
using NAudio.Wave;
using NAudio;

using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using System.Threading;
using DynamicDataDisplaySample.ECGViewModel;
using System.ComponentModel;
using System.Windows.Threading;

#endregion
namespace CliniCycle
{
    /// <summary>
    /// Interaction logic for ClinicianWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        #region Variable declarations
        int patientNum = 0;

        // Audio 
        /*WaveOut wo = new WaveOut();
        WaveFormat wf = new WaveFormat(16000, 1);
        BufferedWaveProvider mybufferwp = null;*/
       
        //ColorImageFormat imageFormat = ColorImageFormat.RgbResolution640x480Fps30;

        // The input and output images to be displayed.
        private WriteableBitmap outputImage;
        private WriteableBitmap inputImage1;
        private WriteableBitmap inputImage2;
        private WriteableBitmap inputImage3;
        private WriteableBitmap inputImage4;
        private WriteableBitmap inputImage5;
        private WriteableBitmap inputImage6;
        private WriteableBitmap bigInputImage;

        //for sockets
        private byte[] pixels = new byte[0];

        private AsyncCallback socketBioWorkerCallback;
        public Socket socketBioListener;
        public Socket bioSocketWorker;

        // Patient's names for now
        String p1, p2, p3, p4, p5, p6;

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

        private Random _Random;
        private int _maxECG;
        public int MaxECG
        {
            get { return _maxECG; }
            set { _maxECG = value; this.OnPropertyChanged("MaxECG"); }
        }

        private int _minECG;
        public int MinECG
        {
            get { return _minECG; }
            set { _minECG = value; this.OnPropertyChanged("MinECG"); }
        }

        public ECGPointCollection ecgPointCollection;
        DispatcherTimer updateCollectionTimer;
        private int i = 0;

        #endregion

        public MainWindow()
        {
            this.InitializeComponent();
            //Loaded += OnLoaded;

           // InitializeBioSockets();

            //setup the kinect server
            InitializeKinect();
            //InitializeAudio();

            InitializeECG();
            
            this.DataContext = this;                
        }

        #region Kinect
        private void InitializeKinect()
        {
            this.sensorChooser = new KinectSensorChooser();
            this.sensorChooser.KinectChanged += sensorChooser_KinectChanged;
            this.sensorChooserUi.KinectSensorChooser = this.sensorChooser;
            this.sensorChooser.Start();

            // Don't try this unless there is a kinect.
            if (sensorChooser.Kinect != null)
            {
                // bind to the UI for control
                // Bind the sensor chooser's current sensor to the KinectRegion
                var regionSensorBinding = new Binding("Kinect") { Source = this.sensorChooser };
                BindingOperations.SetBinding(this.kinectRegion, KinectRegion.KinectSensorProperty, regionSensorBinding);

                //// Receiving video from patient.
                _videoClient = new ColorClient();
                _videoClient.ColorFrameReady += _videoClient_ColorFrameReady;
                _videoClient.Connect("192.168.184.19", 4555);

                // kinect sending video out on port 4531
                _videoListener = new ColorListener(this.sensorChooser.Kinect, 4531, ImageFormat.Jpeg);
                _videoListener.Start();

                /*/ Recieving audio from patient.
                _audioClient = new AudioClient();
                _audioClient.AudioFrameReady += _audioClient_AudioFrameReady;
                _audioClient.Connect("192.168.184.19", 4533); */
            }
        }

       /* private void InitializeAudio()
        {
            mybufferwp = new BufferedWaveProvider(wf);
            mybufferwp.BufferDuration = TimeSpan.FromMinutes(5);
            wo.Init(mybufferwp);
            wo.Play();
        }*/

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

                //pixels appears to be 1228800 bytes long
                outputImage.WritePixels(
                    new Int32Rect(0, 0, frame.Width, frame.Height), this.pixels, frame.Width * 4, 0);

                this.clinicianFeed.Source = outputImage;
                this.kinectPatientFeedLarge.Source = inputImage1;

            };

        }

        void _videoClient_ColorFrameReady(object sender, ColorFrameReadyEventArgs e)
        {
            this.kinectPatientFeed.Source = e.ColorFrame.BitmapImage;
            buttonPatient1.Visibility = Visibility.Hidden;
        }

       /*void _audioClient_AudioFrameReady(object sender, AudioFrameReadyEventArgs e)
        {
            if (mybufferwp != null)
            {
                mybufferwp.AddSamples(e.AudioFrame.AudioData, 0, e.AudioFrame.AudioData.Length);
            }
        }*/
        #endregion

        #region Biodata
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
                IPAddress addy = System.Net.IPAddress.Parse("192.168.184.9");
                IPEndPoint iplocal = new IPEndPoint(addy, 4449);
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
                MessageBox.Show(tmp);
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
                    else if (data[0] == "BP")
                    {
                        if (patientNum == 1) patientBloodPressureBlock.Text = data[1];
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
        #endregion

        #region Buttons
        private void patient1_Click(object sender, RoutedEventArgs e)
        {
            patientIDBlock.Text = p1;
            patientNum = 1;
            patientHeartrateBlock.Text = heartRate1.Content.ToString();
            patientOxygenSatBlock.Text = sat1.Content.ToString();

            // If not connected to client, connect now.
            if (sensorChooser.Kinect != null)
            {
                if (!_videoClient.IsConnected)
                {
                    _videoClient.Connect("192.168.184.19", 4555);
                }

                // Audio.
                /*if (!_audioClient.IsConnected)
                {
                    _audioClient.Connect("192.168.184.19", 4533);
                }*/
            }
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

        /// <summary>
        /// User presses the exit button to quit the workout - ask user to confirm. If they do, close streams and this window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void exitProgramButton_Click(object sender, RoutedEventArgs e)
        {
            // Configure the message box to be displayed 
            string messageBoxText = "Are you sure you want to quit?";
            string caption = "LifeCycle";
            MessageBoxButton button = MessageBoxButton.YesNo;
            MessageBoxImage icon = MessageBoxImage.Warning;
            MessageBoxResult result = MessageBox.Show(messageBoxText, caption, button, icon);

            // Process message box results 
            switch (result)
            {
                case MessageBoxResult.Yes:
                    // User pressed Yes button:
                    // Stop listening for video.
                    if (_videoListener != null)
                        _videoListener.Stop();

                    // Stop listening for bioData.
                    if (socketBioListener != null)
                        socketBioListener.Close();

                    if (bioSocketWorker != null)
                        bioSocketWorker.Close();

                    /*/ Stop playing audio and dispose of the player.
                    if (wo.PlaybackState == PlaybackState.Playing)
                    {
                        wo.Stop();
                        wo.Dispose();
                        wo = null;
                    } */

                    // Close the program.
                    this.Close();
                    break;
                case MessageBoxResult.No:
                    // User pressed No button:
                    // Do nothing.
                    break;
            }
        }
        #endregion



        public void InitializeECG()
        {
            ecgPointCollection = new ECGPointCollection();

            updateCollectionTimer = new DispatcherTimer();
            updateCollectionTimer.Interval = TimeSpan.FromMilliseconds(100);
            updateCollectionTimer.Tick += new EventHandler(updateCollectionTimer_Tick);
            updateCollectionTimer.Start();

            var ds = new EnumerableDataSource<ECGPoint>(ecgPointCollection);
            ds.SetXMapping(x => dateAxis.ConvertToDouble(x.Date));
            ds.SetYMapping(y => y.ECG);
            plotter.AddLineGraph(ds, Colors.SlateGray, 2, "ECG"); 
            plotter.VerticalAxis.Remove();
            MaxECG = 1;
            MinECG = -1;
        }

        void updateCollectionTimer_Tick(object sender, EventArgs e)
        {
            i++;
            _Random = new Random();
            ecgPointCollection.Add(new ECGPoint(_Random.NextDouble(), DateTime.Now));
        }

        #region INotifyPropertyChanged members

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                this.PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }

        #endregion



    }
}