// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System.Windows.Media.Imaging;

namespace Coding4Fun.Kinect.KinectService.Common
{
	public class ColorFrameData
	{
		public ColorImageFrame ImageFrame { get; set; }
		public ImageFormat Format { get; set; }
		public BitmapImage BitmapImage { get; set; }
		public byte[] RawImage { get; set; }
	}
}