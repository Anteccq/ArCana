using System;
using System.Linq;
using System.Runtime.InteropServices;
using ArCana.Blockchain;
using ArCana.Blockchain.Util;
using ArCana.Cryptography;
using static System.Console;

namespace KeyGenerate
{
    class Program
    {
        static void Main(string[] args)
        {
            var key = new Key();
            WriteByte(key.PrivateKey);
            WriteByte(key.PublicKey);
            WriteByte(key.PublicKeyHash);

            var key2 = new Key();
            WriteByte(key2.PrivateKey);
            WriteByte(key2.PublicKey);
            WriteByte(key2.PublicKeyHash);

            var coinbaseTx = BlockchainUtil.CreateCoinBaseTransaction(0, key.PublicKeyHash, "Test CoinbaseTx");
            var txbuilder = new TransactionBuilder();
            txbuilder.Inputs.Add(new Input()
            {
                OutputIndex = 0,
                TransactionId = coinbaseTx.Id,
            });
            txbuilder.Outputs.Add(new Output()
            {
                PublicKeyHash = key2.PublicKeyHash,
                Amount = 50
            });

            var signedTx = txbuilder.ToSignedTransaction(key.PrivateKey, key.PublicKey);

            var hash = signedTx.GetSignHash();
            foreach (var input in signedTx.Inputs)
            {
                var valid = Signature.Verify(hash, input.Signature, input.PublicKey,
                    coinbaseTx.Outputs.First().PublicKeyHash);
                WriteLine($"Validity check : {valid}");
            }
        }

        static void WriteByte(byte[] data)
            => WriteLine(string.Join("", data.Select(x => $"{x:x2}")));
    }
}
