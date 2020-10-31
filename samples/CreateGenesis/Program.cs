using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ArCana;
using ArCana.Blockchain;
using ArCana.Blockchain.Util;
using ArCana.Cryptography;
using static Utf8Json.JsonSerializer;

namespace CreateGenesis
{
    class Program
    {
        static void Main(string[] args)
        {
            var time = new DateTime(2020, 10, 31, 1, 50, 0, DateTimeKind.Utc);
            var tx = BlockchainUtil.CreateCoinBaseTransaction(0, null, "Haloween - ArCana");
            tx.TimeStamp = time;
            var txs = new List<Transaction>(){tx};
            var rootHash = HashUtil.ComputeMerkleRootHash(txs.Select(x => x.Id.Bytes).ToList());

            var block = new Block()
            {
                PreviousBlockHash = new HexString(""),
                Nonce = 0,
                Transactions = null,
                MerkleRootHash = rootHash,
                Timestamp = time,
                Bits = 17
            };
            if (!Miner.Mine(block, new CancellationToken())) return;
            block.Transactions = txs;
            var json = Serialize(block);
            Console.WriteLine(PrettyPrint(json));

            Console.ReadKey();
        }
    }
}
