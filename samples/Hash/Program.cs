using System;
using System.Linq;
using System.Text;
using ArCana.Cryptography;
using static System.Console;

namespace Hash
{
    class Program
    {
        static void Main(string[] args)
        {
            var data = Encoding.UTF8.GetBytes("ArCana - A Simple Blockchain Library");
            WriteByte(HashUtil.SHA256(data));
            WriteByte(HashUtil.RIPEMD160(data));
            WriteByte(HashUtil.DoubleSHA256Hash(data));
            WriteByte(HashUtil.Hash160(data));
        }

        static void WriteByte(byte[] data)
            => WriteLine(string.Join("", data.Select(x => $"{x:x2}")));
    }
}
