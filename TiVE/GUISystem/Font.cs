using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using ProdigalSoftware.TiVE.Core.Backend;
using ProdigalSoftware.TiVEPluginFramework;

namespace ProdigalSoftware.TiVE.GUISystem
{
    internal sealed class Font : IDisposable
    {
        private const string FontImageExtension = ".png";
        private const string FontInfoExtension = ".txt";

        private static readonly char[] spaceCharacter = { ' ' };
        private readonly Dictionary<char, CharInfo> charactersInImage = new Dictionary<char, CharInfo>();
        private readonly string fontName;
        private readonly ITexture texture;

        public Font(string fontName)
        {
            this.fontName = fontName;
            
            using (TextReader reader = new StreamReader(TiVEController.ResourceLoader.OpenFile(fontName + FontInfoExtension)))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    string[] lineParts = line.Split(spaceCharacter, StringSplitOptions.RemoveEmptyEntries);
                    if (lineParts.Length < 11)
                        continue;

                    char character = (char)0;
                    short x = 0;
                    short y = 0;
                    short width = 0;
                    short height = 0;
                    float xOffset = 0.0f;
                    float yOffset = 0.0f;
                    float xAdvance = 0.0f;
                    foreach (string part in lineParts)
                    {
                        if (part.StartsWith("char", StringComparison.OrdinalIgnoreCase))
                            continue;

                        int equalIndex = part.IndexOf('=');
                        if (equalIndex == -1)
                            continue;

                        string key = part.Substring(0, equalIndex).ToLowerInvariant();
                        float value = float.Parse(part.Substring(equalIndex + 1));
                        switch (key)
                        {
                            case "id": character = (char)value; break;
                            case "x": x = (short)value; break;
                            case "y": y = (short)value; break;
                            case "width": width = (short)value; break;
                            case "height": height = (short)value; break;
                            case "xoffset": xOffset = value; break;
                            case "yoffset": yOffset = value; break;
                            case "xadvance": xAdvance = value; break;
                        }
                    }

                    if (character != (char)0)
                        charactersInImage.Add(character, new CharInfo(x, y, width, height, xOffset, yOffset, xAdvance));
                }
            }

            using (Stream stream = TiVEController.ResourceLoader.OpenFile(fontName + FontImageExtension))
            using (Bitmap fontImage = new Bitmap(stream))
            {
                BitmapData data = fontImage.LockBits(new Rectangle(0, 0, fontImage.Width, fontImage.Height),
                    ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

                texture = TiVEController.Backend.CreateTexture(fontImage.Width, fontImage.Height, GetBytes(data));
                fontImage.UnlockBits(data);
            }

            // Save font image with correct pixel brightness
            //Bitmap fontImage = new Bitmap(Path.Combine(fontDir, fontName + FontImageExtension));
            //Bitmap newBitmap = new Bitmap(fontImage.Width, fontImage.Height, fontImage.PixelFormat);
            //for (int i = 0; i < fontImage.Width; i++)
            //{
            //    for (int j = 0; j < fontImage.Height; j++)
            //    {
            //        Color originalColor = fontImage.GetPixel(i, j);
            //        newBitmap.SetPixel(i, j, Color.FromArgb(originalColor.A, 255, 255, 255));
            //    }
            //}
            //newBitmap.Save(Path.Combine(fontDir, fontName + "new" + FontImageExtension), ImageFormat.Png);
        }

        public void Dispose()
        {
            texture.Dispose();
        }

        public void Initialize()
        {
            texture.Initialize();
        }

        public Vector2s MeasureText(string text)
        {
            float width = 0;
            short height = 0;
            foreach (char c in text)
            {
                CharInfo info = GetInfo(c);
                width += info.XAdvance;
                height = Math.Max(height, info.Height);
            }

            return new Vector2s((short)width, height);
        }

        public void DrawText(string text)
        {
            texture.Activate();
        }

        private CharInfo GetInfo(char c)
        {
            CharInfo info;
            charactersInImage.TryGetValue(c, out info);
            return info ?? CharInfo.Empty;
        }

        private static byte[] GetBytes(BitmapData data)
        {
            Debug.Assert(data.PixelFormat == PixelFormat.Format32bppArgb);

            int byteCount = data.Width * data.Height * 4;
            byte[] bytes = new byte[byteCount];
            Marshal.Copy(data.Scan0, bytes, 0, byteCount);
            return bytes;
        }

        private sealed class CharInfo
        {
            public static readonly CharInfo Empty = new CharInfo(0, 0, 0, 0, 0.0f, 0.0f, 0.0f);

            public readonly float XOffset;
            public readonly float YOffset;
            public readonly float XAdvance;
            public readonly short Width;
            public readonly short Height;

            private readonly short x;
            private readonly short y;

            public CharInfo(short x, short y, short width, short height, float xOffset, float yOffset, float xAdvance)
            {
                this.x = x;
                this.y = y;
                Width = width;
                Height = height;
                XOffset = xOffset;
                YOffset = yOffset;
                XAdvance = xAdvance;
            }
        }
    }
}
