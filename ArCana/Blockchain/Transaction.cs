using System;
using System.Collections.Generic;
using System.Text;
using ArCana.Blockchain.Util;
using ArCana.Cryptography;
using Utf8Json;
using static Utf8Json.JsonSerializer;

namespace ArCana.Blockchain
{
    public class Transaction
    {
        public HexString Id { get; set; }
        public DateTime TimeStamp { get; set; }
        public string Engraving { get; set; }
        public List<Output> Outputs { get; set; }
        public List<Input> Inputs { get; set; }
        public ulong TransactionFee { get; set; }

        public Transaction Clone()
            => CloneUtil.Clone(this);

        public byte[] GetSignHash()
        {
            var tx = this.Clone();
            tx.Id = null;
            foreach (var input in tx.Inputs)
            {
                input.PublicKey = null;
                input.Signature = null;
            }

            var b = Serialize(tx);
            return HashUtil.DoubleSHA256Hash(b);
        }
    }

    public class Output
    {
        public ulong Amount;
        public byte[] PublicKeyHash { get; set; }
    }

    public class Input
    {
        public HexString TransactionId { get; set; }
        public int OutputIndex { get; set; }
        public byte[] Signature { get; set; }
        public byte[] PublicKey { get; set; }
    }
}
