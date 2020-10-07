using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ArCana.Cryptography;

namespace ArCana.Blockchain.Util
{
    public static class BlockchainUtil
    {
        private static readonly DateTime GenesisTime = new DateTime(2020, 10, 7, 20, 01, 52, DateTimeKind.Utc);
        private const int CoinBaseInterval = 10000;

        public static Block CreateGenesisBlock()
        {
            var tx = CreateCoinBaseTransaction(0, null, "ArC - A Little BlockChain by C#");
            tx.TimeStamp = GenesisTime;
            var txs = new List<Transaction> { tx };
            var rootHash = HashUtil.ComputeMerkleRootHash(txs.Select(x => x.Id.Bytes).ToList());

            return new Block()
            {
                PreviousBlockHash = new HexString(""),
                Nonce = 2083236893,
                Transactions = txs,
                MerkleRootHash = rootHash,
                Timestamp = GenesisTime,
                Bits = 17
            };
        }

        public static Transaction CreateCoinBaseTransaction(int height, byte[] publicKeyHash, string engrave = "")
        {
            var cbOut = new Output()
            {
                Amount = (ulong)GetSubsidy(height),
                PublicKeyHash = publicKeyHash
            };
            var tb = new TransactionBuilder(new List<Output>() { cbOut }, new List<Input>(), engrave);
            return tb.ToTransaction();
        }

        public static ulong GetSubsidy(int height) =>
            (ulong)50 >> height / CoinBaseInterval;
    }
}
