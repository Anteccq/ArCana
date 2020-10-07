using System;
using System.Collections.Generic;
using System.Text;
using ArCana.Cryptography;
using ArCana.Extensions;
using static Utf8Json.JsonSerializer;

namespace ArCana.Blockchain
{
    public class TransactionBuilder
    {
        private readonly Transaction _transaction;

        public IList<Output> Outputs => _transaction.Outputs;

        public IList<Input> Inputs => _transaction.Inputs;

        public TransactionBuilder() : this(new Transaction())
        {

        }

        public TransactionBuilder(List<Output> outputs, List<Input> inputs, string engrave = "")
        {
            _transaction = new Transaction()
            {
                Engraving = engrave,
                Outputs = outputs,
                Inputs = inputs
            };
        }

        public TransactionBuilder(Transaction tx)
        {
            tx.Inputs ??= new List<Input>();
            tx.Outputs ??= new List<Output>();
            tx.Engraving ??= "";
            tx.TransactionFee = 0;
            _transaction = tx;
        }

        public Transaction ToSignedTransaction(byte[] privateKey, byte[] publicKey)
        {
            _transaction.TimeStamp = DateTime.UtcNow;
            _transaction.Id = null;
            var hash = _transaction.GetSignHash();
            var signature = Signature.Sign(hash, privateKey, publicKey);
            foreach (var inEntry in Inputs)
            {
                inEntry.PublicKey = publicKey;
                inEntry.Signature = signature;
            }
            var txData = Serialize(_transaction);
            var txHash = HashUtil.DoubleSHA256Hash(txData);
            _transaction.Id = txHash.ToHexString();
            return _transaction.Clone();
        }

        public Transaction ToTransaction() => ToTransaction(DateTime.UtcNow);

        public Transaction ToTransaction(DateTime timestamp)
        {
            _transaction.TimeStamp = timestamp;
            var txData = Serialize(_transaction);
            var txHash = HashUtil.DoubleSHA256Hash(txData);
            _transaction.Id = txHash.ToHexString();
            return _transaction.Clone();
        }
    }
}
