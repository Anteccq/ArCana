using System;
using System.Collections.Generic;
using System.Text;

namespace ArCana.Cryptography
{
    internal class Coordinate
    {
        public byte[] X { get; set; }

        public byte[] Y { get; set; }

        public void Deconstruct(out byte[] x, out byte[] y) => (x, y) = (X, Y);
    }
}
