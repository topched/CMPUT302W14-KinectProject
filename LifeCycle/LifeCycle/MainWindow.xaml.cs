using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
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

        private KinectSensorChooser sensorChooser;
        ColorImageFormat imageFormat = ColorImageFormat.RgbResolution640x480Fps30;

        private WriteableBitmap outputImage;
        private WriteableBitmap inputImage;
        private byte[] pixels;
        //private int pixelcnt;
        
        public string filePathHR = @"..\..\HR.txt";
        public string filePathOX = @"..\..\OX.txt";
        public string heartLine;
        public string oxygenLine;
        public StreamReader heartRateFile = null;
        public StreamReader oxygenSatFile = null;
        
        public int secondsLeft = 1800;
        public int minutesLeft = 30;
        public int heartRate = 135;
        public int OX = 1;

        public System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
        public bool workoutInProgress = false;
        public Process process = new Process();

        //for the websockets
        private List<IWebSocketConnection> sockets;

        //for regular sockets
        private AsyncCallback socketWorkerCallback;
        public Socket socketListener;
        public Socket socketWorker;

        public MainWindow()
        {
            InitializeComponent();

            //Used to start a websocket server
            //InitializeWebSockets();

            //used to start socket server
            InitializeSockets();
         
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.FileName = "java";

            //Cant hardcode a file path like this 
            //process.StartInfo.Arguments = @"-cp C:\Users\Valerie\Documents\GitHub\CMPUT302W14-KinectProject\EchoClient\src\ ChatClient Brian 142.244.208.133";
            //process.Start(); 
            //process.BeginOutputReadLine();

            // Set up Streams from which to read heartrate and saturation data.
            if (File.Exists(filePathHR))
                heartRateFile = new StreamReader(filePathHR);
            if (File.Exists(filePathOX))
                oxygenSatFile = new StreamReader(filePathOX);
            
            //updateDisplays();
            minutesLeft = (secondsLeft) / 60;
            timerLabel.Content = minutesLeft + "m " + (secondsLeft - (minutesLeft * 60)) + "s";
            
            // Set up timer ticks.
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 1); // 1 second
            
            //Initialize the senser chooser and UI
            this.sensorChooser = new KinectSensorChooser();
            this.sensorChooser.KinectChanged += sensorChooser_KinectChanged;
            this.sensorChooserUi.KinectSensorChooser = this.sensorChooser;
            //this.sensorChooser.Start();

            // Bind the sensor chooser's current sensor to the KinectRegion
            var regionSensorBinding = new Binding("Kinect") { Source = this.sensorChooser };
            BindingOperations.SetBinding(this.kinectRegion, KinectRegion.KinectSensorProperty, regionSensorBinding);

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

        private void InitializeSockets()
        {
            try
            {
                //create listening socket
                socketListener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPAddress addy = System.Net.IPAddress.Parse("127.0.0.1");
                IPEndPoint iplocal = new IPEndPoint(addy, 8444);
                //bind to local IP Address
                socketListener.Bind(iplocal);
                //start listening -- 4 is max connections queue, can be changed
                socketListener.Listen(4);
                //create call back for client connections -- aka maybe recieve video here????
                socketListener.BeginAccept(new AsyncCallback(OnSocketConnection), null);
            }
            catch(SocketException e)
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
            catch(SocketException e)
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

                //just getting simple text right now -- needs to be changed
                //char[] chars = new char[end + 1];
                //System.Text.Decoder d = System.Text.Encoding.UTF8.GetDecoder();
                //int len = d.GetChars(socketID.dataBuffer, 0, end, chars, 0);
                //System.String tmp = new System.String(chars);
                //MessageBox.Show("Got stuff " + tmp);

                byte[] tmp = new byte[end + 1];
                tmp = socketID.dataBuffer;

                this.inputImage = new WriteableBitmap(
                    640, 480, 96, 96, PixelFormats.Bgr32, null);

                this.inputImage.WritePixels(
                    new Int32Rect(0, 0, 640, 480), tmp, 640 * 4, 0);

                //this.kinectClinitianFeed.Source = this.inputImage;

                //MessageBox.Show("Hit");

                //we are in another thread need -- takes to main UI
                int cnt = 0;
                this.Dispatcher.Invoke((Action)(() =>
                    {
                        kinectClinitianFeed.Source = this.inputImage;
                        MessageBox.Show("hit here");

                        if(cnt%2 == 0)
                        {
                            showOptionsButton.Content = "HELLLLLO";

                        }
                        else
                        {
                            showOptionsButton.Content = "BYYYYYYYYE";
                        }

                        

                    }));

                cnt++;

                WaitForData(socketWorker);
            }
            catch(ObjectDisposedException)
            {
                System.Diagnostics.Debugger.Log(0, "1", "\nOnDataReceived: Socket has been closed\n");
            }
            catch(SocketException e)
            {
                MessageBox.Show(e.Message);
            }

        }

        //Computers running on the same network should be able to get at this. -- not used for anything
        private void InitializeWebSockets()
        {
            sockets = new List<IWebSocketConnection>();

            var server = new WebSocketServer("ws://localhost:8181");

            server.Start(socket =>
            {

                socket.OnOpen = () =>
                {
                    sockets.Add(socket);
                };
                socket.OnClose = () =>
                {
                    sockets.Remove(socket);
                };
                socket.OnMessage = message =>
                {
                    MessageBox.Show(message);
                };
            });
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
                beginWorkoutButton.IsEnabled = false;
                MessageBoxResult result = MessageBox.Show("Workout completed - Great job!", "Success!", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        
        public void updateDisplays()
        {
            /*/ Display the heartrate.
            heartRateFile = process.StandardOutput;
            if ((heartLine = heartRateFile.ReadLine()) != null)
            {
                string[] hRWords = heartLine.Split(' ');
                if (hRWords[0] == "HR")
                    heartRateLabel.Content = hRWords[1] + " BPM";
            }
            else
                heartRateLabel.Content = "-- BPM";

           //  Display the oxygen sat.
            if ((oxygenLine = oxygenSatFile.ReadLine()) != null)
            {
                string[] o2Words = oxygenLine.Split(' ');
                if (o2Words[0] == "OX")
                    oxygenSatLabel.Content = o2Words[1] + " %";
            } 
            else
                heartRateLabel.Content = "-- %"; */

            // Display the remaining time.
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

        }

        /// <summary>
        /// User presses the exit button to quit the workout - close streams and this window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void exitProgramButton_Click(object sender, RoutedEventArgs e)
        {
            // Close the streamReaders.
            heartRateFile.Close();
            oxygenSatFile.Close();

            //Close the kinect properly
            this.sensorChooser.Stop();
            this.Close();

            socketListener.Close();
            socketWorker.Close();

           /* MessageBoxResult result = MessageBox.Show("Are you sure you want to return to the login screen?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                

                // Close this window and open the loginWindow.
                var newWindow = new LoginWindow();
                newWindow.Show();
                this.Close();
            } */
        }

        void timeSelectionButton_Click(object sender, RoutedEventArgs e)
        {

            string[] stuff = sender.ToString().Split(' ');
            minutesLeft = Int32.Parse(stuff[1]);
            secondsLeft = (minutesLeft) * 60;
            timerLabel.Content = minutesLeft + "m " + 0 + "s";
            
        }

        

    }
}
