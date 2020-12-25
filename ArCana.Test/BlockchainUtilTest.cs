using ArCana.Blockchain;
using ArCana.Blockchain.Util;
using ArCana.Extensions;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace ArCana.Test
{
    public class BlockchainUtilTest
    {
        [Fact]
        public void VerifyBlockchainTest()
        {
            var list = new List<Block>();
            for(var i = 0; i<10;i++){
                Block block;
                if (i == 0)
                {
                    block = new Block()
                    {
                        PreviousBlockHash = new byte[]{0x00}.ToHexString(),
                        Nonce = (ulong)i
                    };
                    block.Id = block.ComputeId().ToHexString();
                }
                else
                {
                    block = new Block()
                    {
                        PreviousBlockHash = list.Last().Id,
                        Nonce = (ulong)i
                    };
                    block.Id = block.ComputeId().ToHexString();
                }
                list.Add(block);
            }

            BlockchainUtil.VerifyBlockchain(list).Is(true);

            var incorrectBlock = new Block()
            {
                PreviousBlockHash = new byte[]{0x00}.ToHexString(),
                Nonce = (ulong)1,
            };
            incorrectBlock.Id = incorrectBlock.ComputeId().ToHexString();
            list.Add(incorrectBlock);

            BlockchainUtil.VerifyBlockchain(list).Is(false);
        }
    }
}
