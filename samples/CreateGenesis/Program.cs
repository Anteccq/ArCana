using ArCana;
using ArCana.Blockchain;
using ArCana.Blockchain.Util;
using ArCana.Cryptography;
using ArCana.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using static Utf8Json.JsonSerializer;

namespace CreateGenesis
{
    class Program
    {
        static void Main(string[] args)
        {
            var time = new DateTime(2020, 10, 31, 1, 50, 0, DateTimeKind.Utc);
            var tx = BlockchainUtil.CreateCoinBaseTransaction(0, null, time, engrave:"Haloween - ArCana");
            tx.TimeStamp = time;
            var txs = new List<Transaction>(){tx};
            var rootHash = HashUtil.ComputeMerkleRootHash(txs.Select(x => x.Id.Bytes).ToList());

            var block = new Block()
            {
                Id = new HexString("00006D930177AA974CFBA170357D202AEB22BDA7CEA348F93DF9AD9326776843"),
                PreviousBlockHash = new HexString(""),
                Nonce = 8528990852057875687,
                Transactions = null,
                MerkleRootHash = rootHash,
                Timestamp = time,
                Bits = 17,
            };

            //Always True
            Console.WriteLine(Miner.HashCheck(block.ComputeId(), Difficulty.ToTargetBytes(block.Bits)));
            Console.WriteLine( block.Id.Equals(block.ComputeId().ToHexString()) );
        }

        public static bool MineGenesis(Block block, CancellationToken token)
        {
            block.Id = null;
            var rnd = new Random();
            var buf = new byte[sizeof(ulong)];
            rnd.NextBytes(buf);
            var target = Difficulty.ToTargetBytes(block.Bits);
            var nonce = BitConverter.ToUInt64(buf, 0);
            while (!token.IsCancellationRequested)
            {
                block.Nonce = nonce++;
                var data = Serialize(block.GetBlockHeader());
                var hash = HashUtil.DoubleSHA256Hash(data);
                if (!Miner.HashCheck(hash, target)) continue;
                block.Id = hash.ToHexString();
                return true;
            }
            return false;
        }
    }
}
