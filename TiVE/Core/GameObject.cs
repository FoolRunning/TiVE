using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProdigalSoftware.TiVE.Core
{
    internal sealed class GameObject
    {
        public readonly Handle Handle;

        public GameObject(Handle handle)
        {
            Handle = handle;
        }
    }
}
