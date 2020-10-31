using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ArCana.Blockchain
{
    public class TransactionPool
    {
        public List<Transaction> MemPool { get; } = new List<Transaction>();

        public Transaction[] GetPool() =>
            MemPool.ToArray();

        public void RemoveTxs(params HexString[] ids)
        {
            lock (MemPool)
            {
                var target = MemPool.Select(x => x.Id).Intersect(ids);
                MemPool.RemoveAll(x => target.Contains(x.Id));
            }
        }

        public void AddTxs(params Transaction[] txs)
        {
            lock (MemPool)
            {
                var exceptList = txs.Where(tx => !MemPool.Select(x => x.Id).Contains(tx.Id));
                MemPool.AddRange(exceptList);
            }
        }
    }
}
