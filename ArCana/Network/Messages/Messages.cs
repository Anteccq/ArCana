using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using ArCana.Blockchain;
using ArCana.Extensions;
using Utf8Json;
using static Utf8Json.JsonSerializer;

namespace ArCana.Network.Messages
{
    public class Message
    {
        public virtual MessageType Type { get; set; }

        public virtual byte[] Payload { get; set; }

        public async Task SendAsync(IPAddress remoteAddress, int port)
        {
            var timeOut = 3000;
            using var client = new TcpClient()
            {
                SendTimeout = timeOut,
                ReceiveTimeout = timeOut
            };
            await client.ConnectAsync(remoteAddress, port);
            await using var stream = client.GetStream();
            await SerializeAsync(stream, this);
        }

        public async Task SendMessageAsync(IPEndPoint remotePoint) =>
            await SendAsync(remotePoint.Address, remotePoint.Port);
    }

    public class HandShake : IMessage
    {
        public List<string> KnowIpEndPoints { get; set; }

        public Message ToMessage() => this.ToMessage(MessageType.HandShake);
    }

    public class AddrPayload : IMessage
    {
        public List<string> KnownIpEndPoints { get; set; }

        public Message ToMessage() => this.ToMessage(MessageType.Addr);
    }

    public class Ping : IMessage
    {
        public Message ToMessage() => this.ToMessage(MessageType.Ping);
    }

    public class SurfaceHandShake : IMessage
    {
        public Message ToMessage() => this.ToMessage(MessageType.SurfaceHandShake);
    }

    public class NewTransaction : IMessage
    {
        public Transaction Transaction { get; set; }

        public Message ToMessage() => this.ToMessage(MessageType.NewTransaction);

    }

    public class NewBlock : IMessage
    {
        public Block Block { get; set; }

        public Message ToMessage() => this.ToMessage(MessageType.NewBlock);
    }
}
