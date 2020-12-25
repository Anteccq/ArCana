using ArCana.Cryptography;
using System.Text;
using Xunit;

namespace ArCana.Test
{
    public class SignatureTest
    {
        [Fact]
        public void SignVerifyTest()
        {
            var key = new Key();

            for (var i = 0; i < 10; i++)
            {
                var data = Encoding.UTF8.GetBytes($"Test String{i}");
                var signature = Signature.Sign(data, key.PrivateKey, key.PublicKey);
                Signature.Verify(data, signature, key.PublicKey, key.PublicKeyHash).Is(true);
                var key2 = new Key();
                Signature.Verify(data, signature, key2.PublicKey, key2.PublicKeyHash).Is(false);
                Signature.Verify(data, signature, key.PublicKey, key2.PublicKeyHash).Is(false);
                Signature.Verify(data, signature, key2.PublicKey, key.PublicKeyHash).Is(false);
            }
        }
    }
}
