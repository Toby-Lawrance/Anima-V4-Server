using System;
using System.Net.Sockets;
using Core.Plugins;

namespace RelayServer
{
    public class Server : Module
    {
        private TcpListener listener;

        public Server() : base("Relay-Server","Server plugin for handling other remote computers",1) {}

        public override void Init()
        {
            base.Init();

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
