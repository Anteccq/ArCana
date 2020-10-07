using System;
using System.Collections.Generic;
using System.Text;
using static MessagePack.MessagePackSerializer;

namespace ArCana.Blockchain.Util
{
    public static class CloneUtil
    {
        public static T Clone<T>(T obj)
            => Deserialize<T>(Serialize(obj));
    }
}
