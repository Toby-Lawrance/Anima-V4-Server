using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;
using Core;

namespace Core.Network
{
    public static class Helper
    {
        public static bool IPv6Support = Socket.OSSupportsIPv6;

        public static NetMessage ReplyMessage(NetMessage nm, string value)
        {
            return new NetMessage(nm.SendHost,nm.ReceivePlugin,nm.SendPlugin,value);
        }

        public static string ReadFromStreamUntilEnd(StreamReader sr)
        {
            try
            {
                string ReadContents = "";
                string line;
                while ((line = sr.ReadLine()) != "<EOF>")
                {
                    ReadContents += line + Anima.NewLineChar;
                }

                return ReadContents;
            }
            catch (Exception e)
            {
                Anima.Instance.ErrorStream.WriteLine($"Unable to read from stream properly due to: {e.Message}");
                return "";
            }
        }

        public static async Task<(bool, IPAddress)> TryConnect(IPAddress a, int p)
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

        public static TcpClient TryConnectClient(IPAddress a, int p)
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

        public static async Task<(bool, TcpClient)> TrySendMessage(TcpClient tcp, string message)
        {
            if (tcp is null) return (false, null);

            var strem = new StreamWriter(tcp.GetStream());
            await strem.WriteLineAsync(message);
            await strem.FlushAsync();
            return (true, tcp);

        }

        public static async Task<(bool, IPAddress)> TrySendMessage(IPAddress a, int port, string message)
        {
            var tcp = TryConnectClient(a, port);
            if (tcp is null) return (false, a);

            var strem = new StreamWriter(tcp.GetStream());
            await strem.WriteLineAsync(message);
            await strem.FlushAsync();
            return (true, a);

        }
    }
}
