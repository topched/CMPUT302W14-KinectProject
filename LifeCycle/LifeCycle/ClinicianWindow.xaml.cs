using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace LifeCycle
{
	/// <summary>
	/// Interaction logic for ClinicianWindow.xaml
	/// </summary>
	public partial class ClinicianWindow : Window
	{
		public ClinicianWindow()
		{
			this.InitializeComponent();
			
			// Insert code required on object creation below this point.
            
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