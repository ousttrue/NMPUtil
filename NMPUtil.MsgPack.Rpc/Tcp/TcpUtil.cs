using System;
using System.Net;


namespace NMPUtil.Tcp
{
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
