using System;
using Core.Plugins;

namespace RelayClient
{
    public class Client : Module
    {
        public Client() : base("Relay-Client","Client side of server setup",1) {}

        public override void Tick()
        {
            throw new NotImplementedException();
        }
    }
}
