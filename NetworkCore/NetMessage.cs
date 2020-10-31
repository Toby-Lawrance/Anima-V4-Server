using System;
using System.Text.Json.Serialization;

namespace Core.Network
{
    public class NetMessage
    {
        public readonly string SendHost;
        [JsonInclude]
        public readonly string ReceiveHost;

        [JsonInclude]
        public readonly string SendPlugin;
        [JsonInclude]
        public readonly string ReceivePlugin;

        [JsonInclude]
        public bool GetRequest = false;

        [JsonInclude]
        public readonly string Value;

        public NetMessage()
        {
            SendHost = Environment.MachineName;
        }

        public NetMessage(string receiveHost, string sendPlugin, string receivePlugin, string value)
        {
            SendHost = Environment.MachineName;
            ReceiveHost = receiveHost;
            SendPlugin = sendPlugin;
            ReceivePlugin = receivePlugin;
            Value = value;
        }
    }
}