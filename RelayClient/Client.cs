using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Core;
using Core.Network;
using Core.Plugins;

namespace RelayClient
{
    public class Client : Module
    {
        private int port;
        private IPAddress serverAddress;

        public Client() : base("Relay-Client","Client side of server setup",5) {}

        public override void Init()
        {
            base.Init();
            var succ = Anima.Instance.KnowledgePool.TryInsertValue("Server-Port", 0);
            var succ2 = Anima.Instance.KnowledgePool.TryInsertValue("Server-IP", "");
            var succ3 = Anima.Instance.KnowledgePool.TryInsertValue("Server-Ping-Rate", TimeSpan.FromSeconds(5));
            if (!succ)
            {
                Anima.Instance.WriteLine("Added concept of Server-Port to pool, likely needs to be set");
            }

            if (!succ2)
            {
                Anima.Instance.WriteLine("Added concept of Server-IP to pool, likely needs to be set");
            }

            if (!succ3)
            {
                Anima.Instance.WriteLine("Added concept of Server-Ping-Rate to pool, likely needs to be set");
            }

            if (!Helper.IPv6Support)
            {
                Anima.Instance.ErrorStream.WriteLine("IPv6 is not supported");
            }

            Anima.Instance.KnowledgePool.TryGetValue("Server-Ping-Rate", out TimeSpan rate);
            this.TickDelay = rate;

            Anima.Instance.KnowledgePool.TryGetValue("Server-IP", out string IP);
            serverAddress =  IPAddress.Parse(IP);

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
                    serverPayload.Add(nm);
                }
                catch (Exception e)
                {
                    Anima.Instance.ErrorStream.WriteLine($"Could not deserialize a message: {m} {Anima.NewLineChar}because: {e.Message}");
                }
            }

            var GetRequest = new NetMessage();
            serverPayload.Add(GetRequest);

            var payload = serverPayload.ToArray();
            var serializedPayload = Anima.Serialize(payload);
            var tcpClient = Helper.TryConnectClient(serverAddress, port);
            var t = Helper.TrySendMessage(tcpClient, serializedPayload);
            t.Wait();

            if (!t.Result.Item1)
            {
                //If we couldn't send them, we need to put them back for later
                foreach (var msg in messageReference)
                {
                    Anima.Instance.MailBoxes.PostMessage(msg);
                }
                return;
            }

            var replyReader = new StreamReader(tcpClient.GetStream());
            var reply = Helper.ReadFromStreamUntilEnd(replyReader);
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
