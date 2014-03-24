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

namespace LifeCycle
{
	/// <summary>
	/// Interaction logic for ClinicianWindow.xaml
	/// </summary>
	public partial class ClinicianWindow : Window
	{

        //for sockets
        Socket socketClient;

		public ClinicianWindow()
		{
			this.InitializeComponent();
			
			// Insert code required on object creation below this point.

            CreateSocketConnection();

            
            
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