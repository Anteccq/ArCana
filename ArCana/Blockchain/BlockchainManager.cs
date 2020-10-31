using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using ArCana.Network;
using ArCana.Network.Messages;
using static Utf8Json.JsonSerializer;

namespace ArCana.Blockchain
{
    public class BlockchainManager
    {
        private Blockchain _blockchain { get; set; }

        public BlockchainManager() : this(new Blockchain())
        {

        }

        public BlockchainManager(Blockchain blockchain)
        {
            _blockchain = blockchain;
        }

        public async Task NewBlockHandle(NewBlock msg, IPEndPoint endPoint, int localPort)
        {
            var block = msg.Block;
            if(!ValidCheck(block)) return;
            var lastBlock = _blockchain.Chain.Last();
            if (!lastBlock.Id.Equals(block.PreviousBlockHash))
            {
                var req = new Message()
                {
                    Type = MessageType.RequestFullChain,
                    Payload = new byte[] {0}
                }.SendAsync(endPoint, localPort);
            }
            _blockchain.BlockVerify(block);
        }

        async Task ReceiveFullChain(Message msg, IPEndPoint endPoint)
        {
            var chain = Deserialize<List<Block>>(msg.Payload);
            if (chain.Any(block => !ValidCheck(block)) || !Blockchain.VerifyBlockchain(chain)) return;
            var diff = (ulong)chain.Sum(x => x.Bits);
            var localDiff = (ulong)_blockchain.Chain.Sum(x => x.Bits);
            if (diff > localDiff)
            {
                _blockchain.ChainApply(chain);
            }
        }

        async Task SendFullChain(IPEndPoint endPoint, int localPort)
        {
            var blockMsg = new Message()
            {
                Type = MessageType.FullChain,
                Payload = Serialize(_blockchain.Chain)
            };
            await blockMsg.SendAsync(endPoint, localPort);
        }

        public static bool ValidCheck(Block block)
        {
            var id = block.ComputeId();
            var target = Difficulty.ToTargetBytes(block.Bits);
            var isMined = Miner.HashCheck(id, target);
            var twoHours = TimeSpan.FromHours(2);
            var validDate = block.Timestamp < (DateTime.UtcNow + twoHours);
            var isCoinBase = block.Transactions[0].Inputs.Count == 0;
            return block.Id.Bytes.SequenceEqual(id) && isMined && validDate && isCoinBase;
        }
    }
}
