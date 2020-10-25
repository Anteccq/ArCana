using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using ArCana.Cryptography;
using ArCana.Extensions;
using static Utf8Json.JsonSerializer;

namespace ArCana.Blockchain
{
    public class Miner
    {
        public static Miner Instance { get; } = new Miner();

        public byte[] MinerPublicKeyHash { get; set; }

        public static bool Mine(Block block, CancellationToken token)
        {
            var rnd = new Random();
            var buf = new byte[sizeof(ulong)];
            rnd.NextBytes(buf);
            var target = Difficulty.ToTargetBytes(block.Bits);
            var nonce = BitConverter.ToUInt64(buf, 0);
            while (!token.IsCancellationRequested)
            {
                block.Nonce = nonce++;
                block.Timestamp = DateTime.UtcNow;
                var data = Serialize(block);
                var hash = HashUtil.DoubleSHA256Hash(data);
                if (!HashCheck(hash, target)) continue;
                block.Id = hash.ToHexString();
                return true;
            }
            return false;
        }

        public static bool HashCheck(byte[] data1, byte[] target)
        {
            if (data1.Length != 32 || target.Length != 32) return false;
            for (var i = 0; i < data1.Length; i++)
            {
                if (data1[i] < target[i]) return true;
                if (data1[i] > target[i]) return false;
            }
            return true;
        }
    }
}
