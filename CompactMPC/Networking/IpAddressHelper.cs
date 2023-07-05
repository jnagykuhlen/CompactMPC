using System.Net;

namespace CompactMPC.Networking
{
    public static class IpAddressHelper
    {
        public static IPEndPoint BoundToPort(this IPAddress ipAddress, int port)
        {
            return new IPEndPoint(ipAddress, port);
        }
    }
}