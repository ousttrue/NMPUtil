using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NMPUtil.Tcp
{
    public class TcpSocketEventArgs: EventArgs
    {
        public Socket Socket;
    }
}
