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
            var coinbaseTx = BlockchainUtil.CreateCoinBaseTransaction(0, aliceKey.PublicKeyHash, "Alice Coin");
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

            var block = new Block()
            {
                Timestamp = DateTime.UtcNow,
                Transactions = new List<Transaction>() { coinbaseTx },
                Bits = Difficulty.MinDifficultBits
            };

            var block2 = new Block()
            {
                Timestamp = DateTime.UtcNow,
                Transactions = new List<Transaction>() { tx },
                Bits = Difficulty.MinDifficultBits
            };
            block.MerkleRootHash = HashUtil.ComputeMerkleRootHash(block.Transactions);
            block2.MerkleRootHash = HashUtil.ComputeMerkleRootHash(block2.Transactions);

            Miner.Mine(block, CancellationToken.None);
            var blockchain = new Blockchain();
            blockchain.BlockVerify(block);

            var ok = blockchain.VerifyTransaction(tx, block2.Timestamp, false);

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
