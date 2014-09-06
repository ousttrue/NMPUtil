using System;
using System.Net.Sockets;

namespace NMPUtil.Tcp
{
    public class TcpSocketEventArgs: EventArgs
    {
        public Socket Socket;
    }
}
