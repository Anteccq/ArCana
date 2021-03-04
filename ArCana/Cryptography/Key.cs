namespace ArCana.Cryptography
{
    public class Key
    {
        public byte[] PublicKey { get; set; }
        public byte[] PrivateKey { get; set; }
        public byte[] PublicKeyHash { get; set; }

        public Key()
        {
            (PrivateKey, PublicKey) = Signature.GenerateKeys();
            PublicKeyHash = HashUtil.Hash160(PublicKey);
        }
    }
}
