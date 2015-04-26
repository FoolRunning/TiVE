using System.Collections.Generic;
using ProdigalSoftware.TiVE.Core.Backend;
using Squid;

namespace ProdigalSoftware.TiVE.GUISystem
{
    internal class SquidRenderer : ISquidRenderer
    {
        private readonly Dictionary<string, int> nameToIdMap = new Dictionary<string, int>();
        private readonly Dictionary<int, ITexture> idToTextureMap = new Dictionary<int, ITexture>();

        #region Implementation of IGUIRenderer
        public void Dispose()
        {
            foreach (ITexture texture in idToTextureMap.Values)
                texture.Dispose();

            nameToIdMap.Clear();
            idToTextureMap.Clear();
        }

        public int GetTexture(string name)
        {
            int textureId;
            if (nameToIdMap.TryGetValue(name, out textureId))
                return textureId;

            ITexture newTexture = TiVEController.Backend.CreateTexture(100, 100);
            newTexture.Initialize();
            nameToIdMap.Add(name, newTexture.Id);
            idToTextureMap.Add(newTexture.Id, newTexture);
            return newTexture.Id;
        }

        public int GetFont(string name)
        {
            return 0;
        }

        public Point GetTextSize(string text, int font)
        {
            return Point.Zero;
        }

        public Point GetTextureSize(int texture)
        {
            return Point.Zero;
        }

        public void Scissor(int x, int y, int width, int height)
        {
            //GL.Scissor(x, y, width, height);
        }

        public void DrawBox(int x, int y, int width, int height, int color)
        {
        }

        public void DrawText(string text, int x, int y, int font, int color)
        {
        }

        public void DrawTexture(int texture, int x, int y, int width, int height, Rectangle source, int color)
        {
            idToTextureMap[texture].Activate();
            
        }

        public void StartBatch()
        {
            
        }

        public void EndBatch(bool final)
        {
            
        }
        #endregion
    }
}
