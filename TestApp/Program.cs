using System;
using System.Collections.Generic;
using System.Threading;
using ArCana.Blockchain;
using ArCana.Blockchain.Util;
using ArCana.Cryptography;
using ArCana.Extensions;

namespace TestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var aliceKey = new Key();
            var bobKey = new Key();

            //Aliceが50コイン所持しているというトランザクションが必要
            var coinbaseTx = BlockchainUtil.CreateCoinBaseTransaction(0, aliceKey.PublicKeyHash, engrave:"Alice Coin");
            var txBuilder = new TransactionBuilder();
            txBuilder.Inputs.Add(new Input()
            {
                OutputIndex = 0,
                TransactionId = coinbaseTx.Id,
            });
            txBuilder.Outputs.Add(new Output()
            {
                PublicKeyHash = bobKey.PublicKeyHash,
                Amount = 10
            });
            txBuilder.Outputs.Add(new Output()
            {
                PublicKeyHash = aliceKey.PublicKeyHash,
                Amount = 40
            });

            var tx = txBuilder.ToSignedTransaction(aliceKey.PrivateKey, aliceKey.PublicKey);
            var hash = tx.GetSignHash();
            var input = tx.Inputs[0];
            var isAliceTx = Signature.Verify(hash, input.Signature, input.PublicKey, coinbaseTx.Outputs[0].PublicKeyHash);
            Console.WriteLine($"Is Alice Tx ? : { isAliceTx}");

            var preBlock = new Block()
            {
                Timestamp = DateTime.UtcNow,
                Transactions = new List<Transaction>() { coinbaseTx },
                Bits = Difficulty.MinDifficultBits
            };

            var block = new Block()
            {
                Timestamp = DateTime.UtcNow,
                Transactions = new List<Transaction>() { tx },
                Bits = Difficulty.MinDifficultBits
            };
            preBlock.MerkleRootHash = HashUtil.ComputeMerkleRootHash(preBlock.Transactions);
            block.MerkleRootHash = HashUtil.ComputeMerkleRootHash(block.Transactions);

            if (!Miner.Mine(preBlock, CancellationToken.None)) return;
            var blockchain = new Blockchain();
            blockchain.BlockVerify(preBlock);

            var noahKey = new Key();

            var noahCoinbase = BlockchainUtil.CreateCoinBaseTransaction(blockchain.Chain.Count, noahKey.PublicKeyHash);
            block.Transactions.Insert(0, noahCoinbase);
            foreach (var transaction in block.Transactions)
            {
                var isVerified = blockchain.VerifyTransaction(transaction, block.Timestamp, false);
                if (!isVerified) return;
                transaction.TransactionFee = blockchain.CalculateFee(transaction);
            }

            if (!Miner.Mine(block, CancellationToken.None)) return;

            Console.WriteLine("ブロック追加前----------------");
            blockchain.Utxos.ForEach(x => Console.WriteLine($"{x.Output.PublicKeyHash.ToHex()} : {x.Output.Amount}"));

            //ブロック追加
            blockchain.BlockVerify(block);
            
            Console.WriteLine("ブロック追加後----------------");
            blockchain.Utxos.ForEach(x => Console.WriteLine($"{x.Output.PublicKeyHash.ToHex()} : {x.Output.Amount}"));

            var target = Difficulty.ToTargetBytes(1);
            Console.WriteLine(target.ToHex());
            byte[] data;
            ulong nonce = 0;
            do
            {
                data = HashUtil.DoubleSHA256Hash(BitConverter.GetBytes(nonce++));
                Console.WriteLine(data.ToHex());
            } while (!Miner.HashCheck(data, target));
        }
    }
}
