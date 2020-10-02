using System;
using System.Collections.Generic;
using System.Text;
using Org.BouncyCastle.Crypto.Digests;

namespace ArCana.Cryptography
{
    public static class HashUtil
    {
        public static byte[] RIPEMD160(byte[] data)
        {
            var digest = new RipeMD160Digest();
            var result = new byte[digest.GetDigestSize()];
            digest.BlockUpdate(data, 0, data.Length);
            digest.DoFinal(result, 0);
            return result;
        }

        public static byte[] SHA256(byte[] data)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            return sha256.ComputeHash(data);
        }

        public static byte[] DoubleSHA256Hash(byte[] data)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            return sha256.ComputeHash(sha256.ComputeHash(data));
        }

        public static byte[] Hash160(byte[] data) => RIPEMD160(SHA256(data));
    }
}
