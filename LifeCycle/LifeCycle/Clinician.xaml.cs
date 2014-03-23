using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
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
	public partial class Clinician


	{
		public Clinician()
		{
			this.InitializeComponent();

			// Insert code required on object creation below this point.
		}

        public void patient1Thumb_Click(object sender, RoutedEventArgs e)
        {
            patientIDBlock.Text = "clicked.";
        }
	}
}