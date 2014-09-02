using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;


namespace NMPUtil.Streams
{
    public class SocketEventArgs : EventArgs
    {
        public Socket Socket;
    }


    public static class TcpUtil
    {
        static public IPEndPoint EndPoint(String host, Int32 port)
        {
            if (String.IsNullOrEmpty(host))
            {
                return EndPoint(new IPAddress(0), port);
            }
            else
            {
                return EndPoint(IPAddress.Parse(host), port);
            }
        }

        static public IPEndPoint EndPoint(IPAddress address, Int32 port)
        {
            return new IPEndPoint(address, port);
        }
    }
}
