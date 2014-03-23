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
        private byte[] pixels;

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

        public MainWindow()
        {
            InitializeComponent();

            // Set up Streams from which to read heartrate and saturation data.
            if (File.Exists(filePathHR))
                heartRateFile = new StreamReader(filePathHR);
            if (File.Exists(filePathOX))
                oxygenSatFile = new StreamReader(filePathOX);

            updateDisplays();

            //Initialize the senser chooser and UI
            this.sensorChooser = new KinectSensorChooser();
            this.sensorChooser.KinectChanged += sensorChooser_KinectChanged;
            this.sensorChooserUi.KinectSensorChooser = this.sensorChooser;
            this.sensorChooser.Start();

            // Set up timer ticks.
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 1); // 1 second

            // Bind the sensor chooser's current sensor to the KinectRegion
            var regionSensorBinding = new Binding("Kinect") { Source = this.sensorChooser };
            BindingOperations.SetBinding(this.kinectRegion, KinectRegion.KinectSensorProperty, regionSensorBinding);

            //fill the time buttons for the workout time selection
            for (int i = 1; i < 12; i++)
            {
                var timeSelectionButton = new KinectCircleButton{
                    Content = i * 5,
                    Height = 180
                };

                timeSelectionButton.Click += timeSelectionButton_Click;
                workoutTimeScrollContent.Children.Add(timeSelectionButton);
            }


        }

        void timeSelectionButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(sender.ToString());
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
            ColorImageFrame frame = e.OpenColorImageFrame();

            if (frame == null)
            {
                return;
            }

            this.pixels = new byte[frame.PixelDataLength];
            frame.CopyPixelDataTo(pixels);

            //get the bitmap of the color frame
            this.outputImage = new WriteableBitmap(
                frame.Width,frame.Height,96,96,PixelFormats.Bgr32,null);

            this.outputImage.WritePixels(
                new Int32Rect(0, 0, frame.Width, frame.Height), this.pixels, frame.Width * 4, 0);

            //show the patient feed video
            this.kinectPatientFeed.Source = this.outputImage;
            
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
        /// <summary>
        /// Updates the heartRate, oxygenSat, and timer displays once a second.
        /// </summary>
        public void updateDisplays()
        {
            // Display the heartrate.
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
                heartRateLabel.Content = "-- %";

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
            }
            //startWorkoutCountdownLabel.Content = "Ready";
            else
            {
                dispatcherTimer.Stop();
                beginWorkoutButton.Content = "Begin Workout";
                workoutInProgress = false;
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

            MessageBoxResult result = MessageBox.Show("Are you sure you want to return to the login screen?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                // Close the streamReaders.
                heartRateFile.Close();
                oxygenSatFile.Close();

                // Close this window and open the loginWindow.
                var newWindow = new LoginWindow();
                newWindow.Show();
                this.Close();
            }
        }

        

    }
}
