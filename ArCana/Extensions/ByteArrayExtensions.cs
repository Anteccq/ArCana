using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ArCana.Extensions
{
    public static class ByteArrayExtensions
    {
        public static HexString ToHexString(this byte[] data)
            => new HexString(data);

        public static string ToHex(this byte[] data)
            => string.Join("", data.Select(x => $"{x:X2}"));
    }
}
