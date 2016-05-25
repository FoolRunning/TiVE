﻿using OpenTK.Input;
using ProdigalSoftware.TiVEPluginFramework;

namespace ProdigalSoftware.TiVE.Core.Backend.OpenTKImpl
{
    internal class MouseImpl : IMouse
    {
        #region Implementation of IMouse
        public Vector2i Location
        {
            get 
            {
                MouseState mouseState = Mouse.GetState(0);
                return new Vector2i(mouseState.X, mouseState.Y); 
            }
        }
        
        public int WheelLocation
        {
            get 
            {
                MouseState mouseState = Mouse.GetState(0);
                return mouseState.Wheel; 
            }
        }
        #endregion
    }
}
