using ArCana.Blockchain.Util;
using ArCana.Cryptography;
using MessagePack;
using System;
using System.Collections.Generic;
using static Utf8Json.JsonSerializer;

namespace ArCana.Blockchain
{
    [MessagePackObject]
    public class BlockHeader
    {
        [Key(0)]
        public HexString Id { get; set; }
        [Key(1)]
        public uint Bits { get; set; }
        [Key(2)]
        public HexString PreviousBlockHash { get; set; }
        [Key(3)]
        public byte[] MerkleRootHash { get; set; }
        [Key(4)]
        public DateTime Timestamp { get; set; }
        [Key(5)]
        public ulong Nonce { get; set; }

        public BlockHeader Clone() => CloneUtil.Clone(this);

        public byte[] ComputeId()
        {
            var block = this.Clone();
            block.Id = null;
            var data = Serialize(block);
            return HashUtil.DoubleSHA256Hash(data);
        }
    }
    [MessagePackObject]
    public class Block : BlockHeader
    {
        [Key(6)]
        public List<Transaction> Transactions { get; set; }

        public new Block Clone() => CloneUtil.Clone(this);

        public BlockHeader GetBlockHeader()
        {
            var header = new BlockHeader()
            {
                Id = Id,
                Bits = Bits,
                PreviousBlockHash = PreviousBlockHash,
                MerkleRootHash = MerkleRootHash,
                Timestamp = Timestamp,
                Nonce = Nonce
            };
            return header;
        }
    }
}
