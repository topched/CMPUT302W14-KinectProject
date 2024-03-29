// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;

namespace Coding4Fun.Kinect.KinectService.Common
{
	public class AudioFrameReadyEventArgs : EventArgs
	{
		public AudioFrameData AudioFrame { get; set; }
	}
}