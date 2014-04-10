// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Net.Sockets;

namespace Coding4Fun.Kinect.KinectService.Listeners
{
	class ConnectionEventArgs : EventArgs
	{
		public TcpClient TcpClient { get; set; }
		public SocketClient SocketClient { get; set; }
	}
}