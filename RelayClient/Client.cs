using System;
using System.Collections.Generic;
using System.Net;
using Core;
using Core.Network;
using Core.Plugins;

namespace RelayClient
{
    public class Client : Module
    {
        private int port;

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
        }

        public override void Tick()
        {
            throw new NotImplementedException();
        }

        public override void Close()
        {
            base.Close();
        }
    }
}
