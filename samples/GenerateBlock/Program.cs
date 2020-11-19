using System;
using System.Collections.Generic;
using System.Linq;
using ArCana.Blockchain;
using ArCana.Blockchain.Util;
using ArCana.Cryptography;
using ArCana.Extensions;
using Utf8Json;

namespace GenerateBlock
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
                Transactions = new List<Transaction>()
                {
                    coinbaseTx,
                    tx
                },
                Bits = Difficulty.MinDifficultBits
            };
            block.MerkleRootHash = HashUtil.ComputeMerkleRootHash(block.Transactions);
            Console.WriteLine(block.MerkleRootHash.ToHex());

            var genesis = BlockchainUtil.CreateGenesisBlock();
            var blockData = JsonSerializer.Serialize(genesis);
            var json = JsonSerializer.PrettyPrint(blockData);
            Console.WriteLine(json);
        }
    }
}