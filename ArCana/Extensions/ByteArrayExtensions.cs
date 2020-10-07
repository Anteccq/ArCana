using System;
using System.Collections.Generic;
using System.Text;

namespace ArCana.Extensions
{
    public static class ByteArrayExtensions
    {
        public static HexString ToHexString(this byte[] data)
            => new HexString(data);
    }
}
