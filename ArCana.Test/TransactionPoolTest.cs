using ArCana.Blockchain;
using System;
using System.Linq;
using Xunit;

namespace ArCana.Test
{
    public class TransactionPoolTest
    {
        [Fact]
        public void AddTxsTest()
        {
            var tp = new TransactionPool();
            tp.MemPool.Clear();
            var tb = new TransactionBuilder();
            var firstTx = tb.ToTransaction();
            var txs = Enumerable.Range(1, 10).Select(_ => tb.ToTransaction()).ToArray();
            tp.AddTxs(firstTx);
            tp.MemPool.Count.Is(1);

            tp.AddTxs(txs);
            tp.MemPool.All(x => txs.Concat(new[] {firstTx}).Select(tx => tx.Id).Contains(x.Id)).Is(true);
            tp.MemPool.Count.Is(txs.Length + 1);
        }

        [Fact]
        public void RemoveTxsTest()
        {
            var tp = new TransactionPool();
            tp.MemPool.Clear();
            var tb = new TransactionBuilder();
            var firstTx = tb.ToTransaction();

            var txs = Enumerable.Range(1, 10).Select(_ => tb.ToTransaction()).ToArray();
            tp.AddTxs(firstTx);
            tp.MemPool.Count.Is(1);
            tp.RemoveTxs(firstTx.Id);
            tp.MemPool.Count.Is(0);
            tp.AddTxs(txs);

            tp.MemPool.All(x => txs.Select(tx => tx.Id).Contains(x.Id)).Is(true);
            tp.MemPool.Count.Is(txs.Length);

            tp.RemoveTxs(txs.First().Id);
            tp.MemPool.Select(x => x.Id).Contains(txs.First().Id).Is(false);
            tp.MemPool.Count.Is(txs.Length-1);
        }
    }
}
