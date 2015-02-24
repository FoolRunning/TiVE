namespace ProdigalSoftware.TiVE.Renderer
{
    /// <summary>
    /// Represents a valid display resolution
    /// </summary>
    internal sealed class DisplaySetting
    {
        /// <summary>
        /// Gets the width of the display
        /// </summary>
        public readonly int Width;
        /// <summary>
        /// Gets the height of the display
        /// </summary>
        public readonly int Height;
        /// <summary>
        /// The refresh rate of the display
        /// </summary>
        public readonly int RefreshRate;

        /// <summary>
        /// Creates a new DisplaySetting with the specified settings
        /// </summary>
        public DisplaySetting(int width, int height, int refresh)
        {
            Width = width;
            Height = height;
            RefreshRate = refresh;
        }

        public override int GetHashCode()
        {
            return Width << 20 | Height << 8 | RefreshRate << 0;
        }

        public override bool Equals(object obj)
        {
            DisplaySetting other = obj as DisplaySetting;
            return other != null && other.Width == Width && other.Height == Height && other.RefreshRate == RefreshRate;
        }

        public override string ToString()
        {
            return string.Format("{0}x{1} - {2}Hz", Width, Height, RefreshRate);
        }
    }
}
