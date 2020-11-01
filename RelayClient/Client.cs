using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Core;
using Core.Network;
using Core.Plugins;
using Newtonsoft.Json;

namespace RelayClient
{
    public class Client : Module
    {
        private int port;
        private IPAddress serverAddress;

        public Client() : base("Relay-Client","Client side of server setup",30) {}

        public override void Init()
        {
            base.Init();
            var succ = Anima.Instance.KnowledgePool.TryInsertValue("Server-Port", 0);
            var succ2 = Anima.Instance.KnowledgePool.TryInsertValue("Server-IP", "");
            var succ3 = Anima.Instance.KnowledgePool.TryInsertValue("Server-Ping-Rate", 30);
            if (succ)
            {
                Anima.Instance.WriteLine("Added concept of Server-Port to pool, likely needs to be set");
            }

            if (succ2)
            {
                Anima.Instance.WriteLine("Added concept of Server-IP to pool, likely needs to be set");
            }

            if (succ3)
            {
                Anima.Instance.WriteLine("Added concept of Server-Ping-Rate to pool, likely needs to be set");
            }

            if (!Helper.IPv6Support)
            {
                Anima.Instance.ErrorStream.WriteLine("IPv6 is not supported");
            }

            Anima.Instance.KnowledgePool.TryGetValue("Server-Ping-Rate", out int rate);
            this.TickDelay = TimeSpan.FromSeconds(rate);

            Anima.Instance.KnowledgePool.TryGetValue("Server-IP", out string IP);
            if (!string.IsNullOrWhiteSpace(IP))
            {
                serverAddress = IPAddress.Parse(IP);
            }
            else
            {
                serverAddress = Dns.GetHostAddresses(Dns.GetHostName()).First();
                Anima.Instance.ErrorStream.WriteLine($"Using default host IP");
            }

            Anima.Instance.KnowledgePool.TryGetValue("Server-Port", out int Port);
            this.port = Port;
        }

        public override void Tick()
        {
            Message m;
            var messageReference = new List<Message>();
            var serverPayload = new List<NetMessage>();
            while ((m = Anima.Instance.MailBoxes.GetMessage(this)) is not null)
            {
                try
                {
                    messageReference.Add(m);
                    var nm = Anima.Deserialize<NetMessage>(m.Value);
                    Anima.Instance.WriteLine($"Sending from: {m.Sender}, {Anima.Serialize(nm)}");
                    serverPayload.Add(nm);
                }
                catch (Exception e)
                {
                    Anima.Instance.ErrorStream.WriteLine($"Could not deserialize a message: {m} {Anima.NewLineChar}because: {e.Message}");
                }
            }

            var GetRequest = new NetMessage(true);
            serverPayload.Add(GetRequest);

            var payload = serverPayload.ToArray();
            var serializedPayload = Anima.Serialize(payload);
            Anima.Instance.WriteLine($"About to try and send: {serializedPayload}");
            var tcpClient = Helper.TryConnectClient(serverAddress, port);
            Anima.Instance.WriteLine($"Tried to connect: {tcpClient}");
            var t = Helper.TrySendMessage(tcpClient, serializedPayload);
            Anima.Instance.WriteLine($"Sent off: {t.Status}");
            t.Wait();
            Anima.Instance.WriteLine($"It's done: {t.Status}, {t.Result}");
            if (!t.Result.Item1)
            {
                Anima.Instance.WriteLine($"Couldn't send messages, saving them for later: {t.Result}, {t.Status}");
                //If we couldn't send them, we need to put them back for later
                foreach (var msg in messageReference)
                {
                    Anima.Instance.MailBoxes.PostMessage(msg);
                }
                return;
            }

            var replyReader = new StreamReader(tcpClient.GetStream());
            Anima.Instance.WriteLine($"Waiting for reply: {replyReader}");
            var reply = Helper.ReadFromStreamUntilEnd(replyReader);
            Anima.Instance.WriteLine($"Got: {reply} in reply");
            var messageQueue = Anima.Deserialize<Queue<NetMessage>>(reply);

            foreach (var message in messageQueue)
            {
                var msg = new Message(message.SendPlugin, message.ReceivePlugin, "Remote", message.Value);
                Anima.Instance.MailBoxes.PostMessage(msg);
            }

            tcpClient.Close();
        }

    }
}
