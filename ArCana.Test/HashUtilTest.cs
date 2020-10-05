using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using ArCana.Cryptography;

namespace ArCana.Test
{
    public class HashUtilTest
    {
        [Fact]
        public void RipeMD160Test()
        {
            var ans = "9c1185a5c5e9fc54612808977ee8f548b2258d31";
            var asciiByte = Encoding.ASCII.GetBytes("");
            var hash = HashUtil.RIPEMD160(asciiByte);
            var actual = string.Join("", hash.Select(x => $"{x:x2}"));
            actual.Is(ans);
        }
    }
}
