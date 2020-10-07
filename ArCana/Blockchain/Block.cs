using System;
using System.Collections.Generic;
using System.Text;
using MessagePack;

namespace ArCana.Blockchain
{
    [MessagePackObject]
    public class Block
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
        [Key(6)]
        public List<Transaction> Transactions { get; set; }
    }
}
