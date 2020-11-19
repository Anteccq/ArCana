using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ArCana.Blockchain;
using Xunit;

namespace ArCana.Test
{
    public class DifficultyTest
    {
        [Fact]
        public void BitsPropertyTest()
        {
            uint bits = 20;
            var dif = new Difficulty(bits);
            dif.Bits.Is(bits);
            
            bits = Difficulty.MinDifficultBits-1;
            dif = new Difficulty(bits);
            dif.Bits.Is(Difficulty.MinDifficultBits);

            bits = Difficulty.MaxDifficultBits+1;
            dif = new Difficulty(bits);
            dif.Bits.Is(Difficulty.MaxDifficultBits);
        }

        [Fact]
        public void ToTargetBytesTest()
        {
            var data = ConcatBytes(new byte[] {0x00, 0x00, 0x0F});
            var dif = new Difficulty(20);
            dif.ToTargetBytes().SequenceEqual(data).Is(true);

            data = ConcatBytes(new byte[] { 0x00, 0x00, 0x00, 0x03 });
            dif = new Difficulty(30);
            dif.ToTargetBytes().SequenceEqual(data).Is(true);
        }

        byte[] ConcatBytes(byte[] bytes)
        {
            var byteLength = 32;
            var data = new byte[byteLength];
            for (var i = 1; i < byteLength; i++)
            {
                if (i < bytes.Length) data[i] = bytes[i];
                else data[i] = 0xFF;
            }

            return data;
        }
    }
}
