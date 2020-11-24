using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using ArCana.Blockchain.Util;
using ArCana.Cryptography;
using ArCana.Extensions;
using static Utf8Json.JsonSerializer;

namespace ArCana.Blockchain
{
    public class Miner
    {
        public byte[] MinerPublicKeyHash { get; set; }
        public bool IsMining { get; set; } = false;
        public TransactionPool TransactionPool { get; set; }
        public Blockchain Blockchain { get; set; }

        public Miner(TransactionPool tp, Blockchain blockchain, byte[] minerKeyHash)
        {
            TransactionPool = tp;
            Blockchain = blockchain;
            MinerPublicKeyHash = minerKeyHash;
        }

        public static bool Mine(Block block, CancellationToken token)
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

        public bool Execute(CancellationToken token, out Block block)
        {
            block = null;
            var txs = TransactionPool.GetPool();
            var time = DateTime.UtcNow;
            var subsidy = BlockchainUtil.GetSubsidy(Blockchain.Chain.Count);

            var txList = txs.Where(tx =>
            {
                if (token.IsCancellationRequested || !Blockchain.VerifyTransaction(tx, time, false, out var txFee)) return false;
                subsidy += txFee;
                return true;
            }).ToList();

            var coinbaseOut = new Output()
            {
                Amount = subsidy,
                PublicKeyHash = MinerPublicKeyHash
            };
            var tb = new TransactionBuilder(new List<Output>(){coinbaseOut}, new List<Input>());
            var coinbaseTx = tb.ToTransaction(time);

            if (!Blockchain.VerifyTransaction(coinbaseTx, time, true, out var txFee, subsidy)) return false;
            txList.Insert(0, coinbaseTx);

            var txIds = txList.Select(x => x.Id.Bytes).ToList();
            var mineBlock = new Block()
            {
                Id = null,
                PreviousBlockHash = Blockchain.Chain.Last().Id,
                Transactions = null,
                MerkleRootHash = HashUtil.ComputeMerkleRootHash(txIds),
                Bits = Blockchain.GetDifficulty()
            };

            if (!Mine(mineBlock, token)) return false;

            mineBlock.Transactions = txList;
            block = mineBlock;
            return true;
        }
    }
}
