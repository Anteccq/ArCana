using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ArCana.Cryptography;
using Utf8Json;
using static Utf8Json.JsonSerializer;

namespace ArCana.Blockchain.Util
{
    public static class BlockchainUtil
    {
        private static readonly DateTime GenesisTime = new DateTime(2020, 10, 7, 20, 01, 52, DateTimeKind.Utc);
        private const int CoinBaseInterval = 10000;

        public static Block CreateGenesisBlock()
        {
            var time = new DateTime(2020, 10, 31, 1, 50, 0, DateTimeKind.Utc);
            var tx = CreateCoinBaseTransaction(0, null, time, "Haloween - ArCana");
            tx.TimeStamp = time;
            var txs = new List<Transaction>() { tx };
            var rootHash = HashUtil.ComputeMerkleRootHash(txs.Select(x => x.Id.Bytes).ToList());

            return new Block()
            {
                Id = new HexString("00006D930177AA974CFBA170357D202AEB22BDA7CEA348F93DF9AD9326776843"),
                PreviousBlockHash = new HexString(""),
                Nonce = 8528990852057875687,
                Transactions = txs,
                MerkleRootHash = rootHash,
                Timestamp = time,
                Bits = 17,
            };
        }

        public static Transaction CreateCoinBaseTransaction(int height, byte[] publicKeyHash, string engrave = "")
            => CreateCoinBaseTransaction(height, publicKeyHash, DateTime.UtcNow, engrave);

        public static Transaction CreateCoinBaseTransaction(int height, byte[] publicKeyHash, DateTime time, string engrave = "")
        {
            var cbOut = new Output()
            {
                Amount = (ulong)GetSubsidy(height),
                PublicKeyHash = publicKeyHash
            };
            var tb = new TransactionBuilder(new List<Output>() { cbOut }, new List<Input>(), engrave);
            return tb.ToTransaction(time);
        }

        public static ulong GetSubsidy(int height) =>
            (ulong)50 >> height / CoinBaseInterval;

        public static bool VerifyBlockchain(IList<Block> chain)
        {
            var isRight = chain.Take(chain.Count - 1).SkipWhile((block, i) =>
            {
                var hash = block.ComputeId();
                var prevHash = chain[i + 1].PreviousBlockHash.Bytes;
                return prevHash.SequenceEqual(hash);
            }).Any();

            return !isRight;
        }
    }
}
