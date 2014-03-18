using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Windows.Forms;
using Microsoft.Kinect;
using System.Drawing;

namespace WindowsFormsApplication1
{
    public partial class Exercise : Form
    {

        // This SoundPlayer plays a sound when the user finishes the exercise.
        System.Media.SoundPlayer finishSoundPlayer = new System.Media.SoundPlayer(@"C:\Windows\Media\tada.wav");

        bool isRunning = false;

        // Keep track of the elapsed time.
        int timeLeft = 1800;
        int minutesLeft;
        int secondsLeft;

        // Keep track of biometric data
        int heartRate = 0;
        int oX = 0;

        public Exercise()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (timeLeft > 0)
            {
                // Display the new time left 
                // by updating the Time Left label.
                timeLeft = timeLeft - 1;
                UpdateDisplays();
            }
            else
            {
                // If the user ran out of time stop the timer, update buttons,
                // etc. and show a MessageBox.
                StopTimer();
                timeLabel.Text = "Time's up!";
                startButton.Enabled = false;
                finishSoundPlayer.Play();
                MessageBox.Show("You've finished your workout! \n Great job!");
            }
        }
        /// <summary>
        /// Begin the timer and update buttons, etc.
        /// </summary>
        public void StartExercise()
        {
            UpdateDisplays();
            timer1.Start();
            startButton.Text = "Pause";
            isRunning = true;
        }
        /// <summary>
        /// If the timer is not started, start it.  If the timer has started, stop it.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            if (isRunning)
            {
                StopTimer();
                startButton.Text = "Resume";
            }
            else
                StartExercise();
        }
        /// <summary>
        /// Display the time left in the proper format, eg.: 29m 59s.
        /// </summary>
        public void UpdateDisplays()
        {
            minutesLeft = timeLeft / 60;
            secondsLeft = timeLeft - minutesLeft * 60;
            timeLabel.Text = minutesLeft + "m " + secondsLeft + "s";
            readBiometrics();
            heartRateLabel.Text = heartRate + " bpm";
            // If the user's heartRate is inside target range.
            if ((heartRate < 160) && (heartRate > 100))
            {
                encourageLabel.Text = "Good!";
                encourageLabel.ForeColor = Color.Green;
            }
            // If the user's heartRate is a little low.
            else if ((heartRate < 100) && (heartRate > 50))
            {
                encourageLabel.Text = "Speed up!";
                encourageLabel.ForeColor = Color.Yellow;
            }
            // If the user's heartRate is a little high.
            else if ((heartRate > 160) && (heartRate < 200))
            {
                encourageLabel.Text = "Slow down!";
                encourageLabel.ForeColor = Color.Yellow;
            }
        }

        /// <summary>
        /// Stop the timer, either by pausing or running out of time.
        /// </summary>
        public void StopTimer()
        {
            timer1.Stop();
            isRunning = false;
        }

        /// <summary>
        /// If the user presses the quit button, ask them to confirm
        /// before exiting the app.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void quitButton_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to quit?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                // user clicked yes - exit the app
                this.Close();
            }
            else
            {
                // user clicked no - do nothing
            }
        }

        public void readBiometrics()
        {
            heartRate = 171;
            oX = 4;
        }
    }
}
