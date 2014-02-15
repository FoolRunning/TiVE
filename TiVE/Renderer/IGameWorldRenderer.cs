using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProdigalSoftware.TiVE.Renderer
{
    internal interface IGameWorldRenderer
    {
        void CleanUp();
        void Draw(Camera camera, out int drawCount, out int polygonCount);
    }
}
