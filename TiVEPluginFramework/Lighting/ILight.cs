﻿using ProdigalSoftware.Utils;

namespace ProdigalSoftware.TiVEPluginFramework.Lighting
{
    public interface ILight
    {
        Vector3b Location { get; }

        Color3f Color { get; }

        float LightBlockDist { get; }
    }
}
