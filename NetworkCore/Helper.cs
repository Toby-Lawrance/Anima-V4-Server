using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Core;

namespace Core.Network
{
    public static class Helper
    {
        public static bool IPv6Support = Socket.OSSupportsIPv6;

        private static async Task<(bool, IPAddress)> TryConnect(IPAddress a, int p)
        {
            try
            {
                var tcp = new TcpClient(AddressFamily.InterNetworkV6);
                await tcp.ConnectAsync(a, p);
                return (true, a);
            }
            catch (Exception e)
            {
                Anima.Instance.ErrorStream.WriteLine($"Unable to connect to:{a} because {e.Message}");
                return (false, a);
            }
        }

        private static TcpClient TryConnectClient(IPAddress a, int p)
        {
            try
            {
                var tcp = new TcpClient(AddressFamily.InterNetworkV6);
                tcp.Connect(a, p);
                return tcp;
            }
            catch (Exception e)
            {
                Anima.Instance.ErrorStream.WriteLine($"Unable to connect to:{a} because {e.Message}");
                return null;
            }
        }
    }
}
