using System;
using System.Collections.Generic;
using System.Text;
using ArCana.Blockchain.Util;
using ArCana.Cryptography;
using Org.BouncyCastle.Utilities.Encoders;
using Utf8Json;
using static Utf8Json.JsonSerializer;

namespace ArCana.Blockchain
{
    [MessagePack.MessagePackObject]
    public class Transaction
    {
        [MessagePack.Key(0)]
        public HexString Id { get; set; }
        [MessagePack.Key(1)]
        public DateTime TimeStamp { get; set; }
        [MessagePack.Key(2)]
        public string Engraving { get; set; }
        [MessagePack.Key(3)]
        public List<Output> Outputs { get; set; }
        [MessagePack.Key(4)]
        public List<Input> Inputs { get; set; }
        [MessagePack.Key(5)]
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

    [MessagePack.MessagePackObject]
    public class Output
    {
        [MessagePack.Key(0)]
        public ulong Amount;

        [MessagePack.Key(1)]
        public byte[] PublicKeyHash { get; set; }
    }

    [MessagePack.MessagePackObject]
    public class Input
    {
        [MessagePack.Key(0)]
        public HexString TransactionId { get; set; }
        [MessagePack.Key(1)]
        public int OutputIndex { get; set; }
        [MessagePack.Key(2)]
        public byte[] Signature { get; set; }
        [MessagePack.Key(3)]
        public byte[] PublicKey { get; set; }
    }

    public class TransactionOutput
    {
        public HexString TransactionId { get; set; }
        public int OutIndex { get; set; }
        public Output Output { get; set; }
    }
}
