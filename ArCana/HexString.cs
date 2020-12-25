using ArCana.Extensions;
using MessagePack;
using System;
using System.Linq;

namespace ArCana
{
    [MessagePackObject]
    public class HexString : IEquatable<HexString>
    {
        private string _string;
        private byte[] _bytes;
        [IgnoreMember]
        public string String
        {
            get => _string;
            set
            {
                _string = value;
                _bytes = ToBytes(value);
            }
        }
        [Key(0)]
        public byte[] Bytes
        {
            get => _bytes;
            set
            {
                _bytes = value;
                _string = value.ToHex();
            }
        }

        public HexString(string str)
        {
            String = str;
        }

        public HexString(byte[] bytes)
        {
            Bytes = bytes;
        }

        public static byte[] ToBytes(string s)
        {
            var str = s;
            var array = new byte[str.Length / 2];
            for (var i = 0; i < str.Length; i += 2)
            {
                array[i / 2] = Convert.ToByte(str.Substring(i, 2), 16);
            }
            return array;
        }

        public bool Equals(HexString other)
            => !(other is null) && this.Bytes.SequenceEqual(other.Bytes);

        public override string ToString() => String;
    }
}
