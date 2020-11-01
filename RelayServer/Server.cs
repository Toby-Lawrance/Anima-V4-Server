using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Core;
using Core.Network;
using Core.Plugins;

namespace RelayServer
{
    public class Server : Module
    {
        private TcpListener listener;

        private Dictionary<string, Queue<NetMessage>> relayBuffer;

        public Server() : base("Relay-Server","Server plugin for handling other remote computers", TimeSpan.FromDays(1)) {}

        public override void Init()
        {
            base.Init();
            var succ = Anima.Instance.KnowledgePool.TryInsertValue("Server-Port", 0);

            if (succ)
            {
                Anima.Instance.WriteLine("Added concept of Server-Port to pool, likely needs to be set");
            }

            relayBuffer = new Dictionary<string, Queue<NetMessage>>();

            if (listener is not null) return;
            Anima.Instance.KnowledgePool.TryGetValue("Server-Port", out int Port);

            listener = new TcpListener(IPAddress.IPv6Any, Port);
            listener.Start();
            this.StartTask(HandleRequests,TaskCreationOptions.LongRunning);
        }

        private void HandleRequests()
        {
            while (true)
            {
                try
                {
                    var client = listener.AcceptTcpClient();

                    var strem = new StreamReader(client.GetStream());

                    string ReadContents = Helper.ReadFromStreamUntilEnd(strem);

                    Anima.Instance.WriteLine($"Received this: {ReadContents}");

                    var netMessages = Anima.Deserialize<NetMessage[]>(ReadContents);

                    bool AnyGetRequests = netMessages.Any(nm => nm.GetRequest);

                    Anima.Instance.WriteLine($"There is a Get request: {AnyGetRequests}");

                    if (AnyGetRequests)
                    {
                        Anima.Instance.WriteLine("Sending reply with info");
                        var host = netMessages.First()?.SendHost;
                        if (host is null)
                        {
                            Anima.Instance.WriteLine($"Unable to get send host name: {netMessages.Length}");
                        }

                        if (relayBuffer.ContainsKey(netMessages.First().SendHost))
                        {
                            string Messages = Anima.Serialize(relayBuffer[netMessages.First().SendHost]);
                            
                            var replyStrem = new StreamWriter(client.GetStream());
                            replyStrem.WriteLine(Messages);
                        }
                        else
                        {
                            Anima.Instance.WriteLine($"No messages for: {netMessages.First().SendHost}");
                        }
                    }

                    foreach (var netMessage in netMessages)
                    {
                        if (string.IsNullOrWhiteSpace(netMessage.ReceiveHost)) continue;

                        netMessage.GetRequest = false;
                        if (relayBuffer.ContainsKey(netMessage.ReceiveHost) &&
                            (relayBuffer[netMessage.ReceiveHost] is not null))
                        {
                            relayBuffer[netMessage.ReceiveHost].Enqueue(netMessage);
                        }
                        else
                        {
                            relayBuffer.Add(netMessage.ReceiveHost,new Queue<NetMessage>());
                            relayBuffer[netMessage.ReceiveHost].Enqueue(netMessage);
                        }
                    }
                    
                    client.Close();
                }
                catch (SocketException se)
                {
                    if (se.SocketErrorCode == SocketError.AddressFamilyNotSupported)
                    {
                        Anima.Instance.ErrorStream.WriteLine(se.Message);
                    }
                    else
                    {
                        Anima.Instance.ErrorStream.WriteLine(se.Message);
                    }
                }
                catch (Exception e)
                {
                    Anima.Instance.ErrorStream.WriteLine(e);
                }
            }
        }

        public override void Tick()
        {
            return;
        }

    }
}
