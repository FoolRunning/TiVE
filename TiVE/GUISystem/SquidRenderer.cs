using System.Collections.Generic;
using ProdigalSoftware.TiVE.Core.Backend;
using ProdigalSoftware.TiVEPluginFramework;
using Squid;

namespace ProdigalSoftware.TiVE.GUISystem
{
    internal class SquidRenderer : ISquidRenderer
    {
        private readonly Dictionary<string, int> textureNameToIdMap = new Dictionary<string, int>();
        private readonly Dictionary<int, ITexture> idToTextureMap = new Dictionary<int, ITexture>();
        private readonly Dictionary<string, int> fontNameToIdMap = new Dictionary<string, int>();
        private readonly Dictionary<int, Font> idToFontMap = new Dictionary<int, Font>();
        private int lastFontId = 1;
        private int lastTextureId = 1;

        #region Implementation of ISquidRenderer
        public void Dispose()
        {
            foreach (ITexture texture in idToTextureMap.Values)
                texture.Dispose();

            foreach (Font font in idToFontMap.Values)
                font.Dispose();

            textureNameToIdMap.Clear();
            idToTextureMap.Clear();
            fontNameToIdMap.Clear();
            idToFontMap.Clear();
        }

        public int GetTexture(string name)
        {
            int textureId;
            if (textureNameToIdMap.TryGetValue(name, out textureId))
                return textureId;

            ITexture newTexture = TiVEController.Backend.CreateTexture(100, 100);
            newTexture.Initialize();

            int newId = lastTextureId++;
            textureNameToIdMap.Add(name, newId);
            idToTextureMap.Add(newId, newTexture);
            return newId;
        }

        public int GetFont(string name)
        {
            name = "Font";
            int fontId;
            if (fontNameToIdMap.TryGetValue(name, out fontId))
                return fontId;

            Font newFont = new Font("Font");
            newFont.Initialize();
            int newId = lastFontId++;
            fontNameToIdMap.Add(name, newId);
            idToFontMap.Add(newId, newFont);
            return newId;
        }

        public Point GetTextSize(string text, int fontId)
        {
            Font font;
            if (!idToFontMap.TryGetValue(fontId, out font))
                return Point.Zero;
            
            Vector2s textSize = font.MeasureText(text);
            return new Point(textSize.X, textSize.Y);
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

        public void DrawText(string text, int x, int y, int fontId, int color)
        {
            Font font;
            if (!idToFontMap.TryGetValue(fontId, out font))
                return;

            font.DrawText(text);
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
