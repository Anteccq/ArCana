using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using ArCana;
using ArCana.Blockchain;
using ArCana.Blockchain.Util;
using ArCana.Cryptography;
using ArCana.Extensions;
using ArCana.Network;
using ArCana.Network.Messages;
using Utf8Json;

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

            var fee = 0ul;
            foreach (var transaction in block.Transactions)
            {
                var isVerified = blockchain.VerifyTransaction(transaction, block.Timestamp, false, out var txFee);
                if (!isVerified) return;
                fee += txFee;
            }

            var noahKey = new Key();

            var noahCoinbase = BlockchainUtil.CreateCoinBaseTransaction(blockchain.Chain.Count, noahKey.PublicKeyHash, fee);
            block.Transactions.Insert(0, noahCoinbase);

            if (!Miner.Mine(block, CancellationToken.None)) return;

            Console.WriteLine("ブロック追加前----------------");
            blockchain.Utxos.ForEach(x => Console.WriteLine($"{x.Output.PublicKeyHash.ToHex()} : {x.Output.Amount}"));

            //ブロック追加
            blockchain.BlockVerify(block);
            
            Console.WriteLine("ブロック追加後----------------");
            blockchain.Utxos.ForEach(x => Console.WriteLine($"{x.Output.PublicKeyHash.ToHex()} : {x.Output.Amount}"));

            var inputs =
                blockchain.Chain
                    .SelectMany(x => x.Transactions)
                    .SelectMany(x => x.Inputs);

            var outputs =
                blockchain.Chain
                    .SelectMany(x => x.Transactions)
                    .Select(x => (x.Outputs, x.Id))
                    .SelectMany(x => ToTxO(x.Outputs, x.Id));

            var utxo =
                outputs.Where(opt =>
                    !inputs.Any(ipt =>
                        ipt.OutputIndex == opt.OutIndex &&
                        ipt.TransactionId.Equals(opt.TransactionId)));

            var alicePkh = aliceKey.PublicKeyHash;

            var aliceUtxo =
                utxo.Select(x => x.Output)
                    .Where(x => x.PublicKeyHash?.SequenceEqual(alicePkh) ?? false)
                    .ToList();

            var coinSum =
                aliceUtxo
                    .Select(x => x.Amount)
                    .Aggregate((a,b) => a+b);

            Console.WriteLine($"Alice : {coinSum} coin");

            //念のためにUTXOを最新にする。
            blockchain.UpdateUtxos();

            var coinSum2 =
                blockchain.Utxos
                    .Where(x => x.Output.PublicKeyHash?.SequenceEqual(alicePkh) ?? false)
                    .Select(x => x.Output.Amount)
                    .Aggregate((a, b) => a + b);

            Console.WriteLine($"Alice : {coinSum2} coin");

            var noahCoin =
                blockchain.Utxos
                    .First(x => 
                        x.Output.PublicKeyHash?
                            .SequenceEqual(noahKey.PublicKeyHash) ?? false);

            var noahInput = new Input()
            {
                OutputIndex = noahCoin.OutIndex,
                TransactionId = noahCoin.TransactionId
            };

            var noahAmount = noahCoin.Output.Amount;

            var toAliceOutput = new Output()
            {
                PublicKeyHash = alicePkh,
                Amount = noahAmount / 2
            };

            var toNoahOutput = new Output()
            {
                PublicKeyHash = noahKey.PublicKeyHash,
                Amount = noahAmount - toAliceOutput.Amount
            };

            var tb = new TransactionBuilder();
            tb.Inputs.Add(noahInput);   
            tb.Outputs.Add(toAliceOutput);
            tb.Outputs.Add(toNoahOutput);
            var noahTx = tb.ToSignedTransaction(noahKey.PrivateKey, noahKey.PublicKey);

            var node1 = new Server(new CancellationTokenSource());
            var node2 = new Server(new CancellationTokenSource());
            node1.StartAsync(50001).GetAwaiter().GetResult();
            node2.StartAsync(50002).GetAwaiter().GetResult();
            node2.MessageReceived += async (msg, ip) =>
            {
                var newTx = JsonSerializer.Deserialize<NewTransaction>(msg.Payload).Transaction;
                Console.WriteLine($"New Transaction! : {newTx.Id.String}");
            };

            var localhost = IPAddress.Parse("127.0.0.1");

            var txMessage = new NewTransaction() {Transaction = noahTx};
            txMessage.ToMessage()
                .SendAsync(localhost, 50002, 50001).GetAwaiter().GetResult();

            Console.ReadKey();

            var target = Difficulty.ToTargetBytes(1);   
            Console.WriteLine(target.ToHex());
            byte[] data;
            ulong nonce = 0;
            do
            {
                data = HashUtil.DoubleSHA256Hash(BitConverter.GetBytes(nonce++));
                Console.WriteLine(data.ToHex());
            } while (!Miner.HashCheck(data, target));

            var aliceNm = new NetworkManager(CancellationToken.None);
            var bobNm = new NetworkManager(CancellationToken.None);
            aliceNm.StartServerAsync(50005).GetAwaiter().GetResult();
            bobNm.StartServerAsync(50006).GetAwaiter().GetResult();
        }

        static IEnumerable<TransactionOutput> ToTxO(List<Output> outputs, HexString id)
        {
            return outputs.Select((t, i) => new TransactionOutput()
            {
                TransactionId = id,
                OutIndex = i,
                Output = t,
            });
        }
    }
}
