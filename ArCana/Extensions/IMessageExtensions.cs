using System;
using System.Collections.Generic;
using System.Text;
using ArCana.Network.Messages;
using static Utf8Json.JsonSerializer;

namespace ArCana.Extensions
{
    public static class IMessageExtensions
    {
        public static Message ToMessage(this IMessage data, MessageType type)
        {
            return new Message()
            {
                Type = type,
                Payload = Serialize(data)
            };
        }
    }
}
