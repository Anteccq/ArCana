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

        public async Task SendAsync(IPEndPoint remotePoint) =>
            await SendAsync(remotePoint.Address, remotePoint.Port);

        public static Message Create(MessageType type, byte[] data)
        {
            return new Message()
            {
                Type = type,
                Payload = data
            };
        }
    }

    public class HandShake
    {
        public List<string> KnowIpEndPoints { get; set; }

        public Message ToMessage() => Message.Create(MessageType.HandShake, Serialize(this));
    }

    public class AddrPayload
    {
        public List<string> KnownIpEndPoints { get; set; }

        public Message ToMessage() => Message.Create(MessageType.Addr, Serialize(this));
    }

    public class Ping
    {
        public Message ToMessage() => Message.Create(MessageType.Ping, Serialize(this));
    }

    public class SurfaceHandShake
    {
        public Message ToMessage() => Message.Create(MessageType.SurfaceHandShake, Serialize(this));
    }

    public class NewTransaction
    {
        public Transaction Transaction { get; set; }

        public Message ToMessage() => Message.Create(MessageType.NewTransaction, Serialize(this));

    }

    public class NewBlock
    {
        public Block Block { get; set; }

        public Message ToMessage() => Message.Create(MessageType.NewBlock, Serialize(this));
    }
}
