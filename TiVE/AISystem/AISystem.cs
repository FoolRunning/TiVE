using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProdigalSoftware.TiVE.Core;

namespace ProdigalSoftware.TiVE.AISystem
{
    internal sealed class AISystem : EngineSystem
    {
        public AISystem() : base("AI")
        {
            // Path-finding code taken from: http://www.codeproject.com/Articles/632424/EpPathFinding-cs-A-Fast-Path-Finding-Algorithm-Jum
        }

        #region Implementation of EngineSystem
        public override void Dispose()
        {
            
        }

        public override bool Initialize()
        {
            return true;
        }

        public override void ChangeScene(Scene oldScene, Scene newScene)
        {
            
        }

        protected override bool UpdateInternal(int ticksSinceLastUpdate, float timeBlendFactor, Scene currentScene)
        {
            return true;
        }
        #endregion
    }
}
