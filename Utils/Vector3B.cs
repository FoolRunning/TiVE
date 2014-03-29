using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProdigalSoftware.Utils
{
    public struct Vector3b
    {
        public readonly byte X;
        public readonly byte Y;
        public readonly byte Z;

        public Vector3b(byte x, byte y, byte z)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }
}
