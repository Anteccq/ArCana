namespace ArCana.Cryptography
{
    public class Coordinate
    {
        public byte[] X { get; set; }

        public byte[] Y { get; set; }

        public void Deconstruct(out byte[] x, out byte[] y) => (x, y) = (X, Y);
    }
}
