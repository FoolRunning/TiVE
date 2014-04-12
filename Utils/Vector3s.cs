using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProdigalSoftware.Utils
{
    public struct Vector3s
    {
        public readonly short X;
        public readonly short Y;
        public readonly short Z;

        public Vector3s(short x, short y, short z)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }
}
