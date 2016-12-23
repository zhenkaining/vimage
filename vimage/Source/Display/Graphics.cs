﻿using SFML.Graphics;
using System.Collections.Generic;
using System;
using DevIL.Unmanaged;
using Tao.OpenGl;
using System.IO;
using System.Threading;

namespace vimage
{
    /// <summary>
    /// Graphics Manager.
    /// Loads and stores Textures and AnimatedImageDatas.
    /// </summary>
    class Graphics
    {
        private static List<Texture> Textures = new List<Texture>();
        private static List<string> TextureFileNames = new List<string>();

        private static List<AnimatedImageData> AnimatedImageDatas = new List<AnimatedImageData>();
        private static List<string> AnimatedImageDataFileNames = new List<string>();

        public const uint MAX_TEXTURES = 20;
        public const uint MAX_ANIMATIONS = 6;
        public static int TextureMaxSize = (int)Texture.MaximumSize;

        public static Sprite GetSprite(string fileName, bool smooth = false)
        {
            Sprite sprite = new Sprite(GetTexture(fileName));
            sprite.Texture.Smooth = smooth;

            return sprite;
        }
        public static Texture GetTexture(string fileName)
        {
            int index = TextureFileNames.IndexOf(fileName);

            if (index >= 0)
            {
                // Texture Already Exists
                // move it to the end of the array and return it
                Texture texture = Textures[index];
                string name = TextureFileNames[index];

                Textures.RemoveAt(index);
                TextureFileNames.RemoveAt(index);
                Textures.Add(texture);
                TextureFileNames.Add(name);

                return Textures[Textures.Count - 1];
            }
            else
            {
                // New Texture
                Texture texture = null;
                int imageID = IL.GenerateImage();
                IL.BindImage(imageID);

                IL.Enable(ILEnable.AbsoluteOrigin);
                IL.SetOriginLocation(DevIL.OriginLocation.UpperLeft);

                bool loaded = false;
                using (FileStream fileStream = File.OpenRead(fileName))
                    loaded = IL.LoadImageFromStream(fileStream);


                if (IL.GetImageInfo().Width > TextureMaxSize || IL.GetImageInfo().Height > TextureMaxSize)
                {
                    System.Windows.Forms.MessageBox.Show("Image exceeds the GPU's maximum supported texture size (" + TextureMaxSize + ").", "vimage");
                    return null;
                }

                if (loaded)
                {
                    texture = GetTextureFromBoundImage();

                    Textures.Add(texture);
                    TextureFileNames.Add(fileName);

                    // Limit amount of Textures in Memory
                    if (Textures.Count > MAX_TEXTURES)
                    {
                        Textures[0].Dispose();
                        Textures.RemoveAt(0);
                        TextureFileNames.RemoveAt(0);
                    }
                }
                IL.DeleteImages(new ImageID[] { imageID });

                return texture;
            }
        }
        private static Texture GetTextureFromBoundImage(int imageNum = 0)
        {
            IL.ActiveImage(imageNum);
            
            bool success = IL.ConvertImage(DevIL.DataFormat.RGBA, DevIL.DataType.UnsignedByte);
            
            if (!success)
                return null;

            int width = IL.GetImageInfo().Width;
            int height = IL.GetImageInfo().Height;

            Texture texture = new Texture((uint)width, (uint)height);
            Texture.Bind(texture);
            {
                Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MAG_FILTER, Gl.GL_LINEAR);
                Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MIN_FILTER, Gl.GL_LINEAR);
                Gl.glTexImage2D(
                    Gl.GL_TEXTURE_2D, 0, IL.GetInteger(ILIntegerMode.ImageBytesPerPixel),
                    width, height, 0,
                    IL.GetInteger(ILIntegerMode.ImageFormat), ILDefines.IL_UNSIGNED_BYTE,
                    IL.GetData()
                    );
            }
            Texture.Bind(null);

            return texture;
        }

        public static Sprite GetSpriteFromIcon(string fileName)
        {
            try
            {
                System.Drawing.Icon icon = new System.Drawing.Icon(fileName, 256, 256);
                System.Drawing.Bitmap iconImage = ExtractVistaIcon(icon);
                if (iconImage == null)
                    iconImage = icon.ToBitmap();

                Sprite iconSprite;

                using (MemoryStream iconStream = new MemoryStream())
                {
                    iconImage.Save(iconStream, System.Drawing.Imaging.ImageFormat.Png);
                    iconSprite = new Sprite(new Texture(iconStream));
                }

                return iconSprite;
            }
            catch (Exception) { }
            return null;
        }
        // http://stackoverflow.com/questions/220465/using-256-x-256-vista-icon-in-application/1945764#1945764
        // Based on: http://www.codeproject.com/KB/cs/IconExtractor.aspx
        // And a hint from: http://www.codeproject.com/KB/cs/IconLib.aspx
        public static System.Drawing.Bitmap ExtractVistaIcon(System.Drawing.Icon icoIcon)
        {
            System.Drawing.Bitmap bmpPngExtracted = null;
            try
            {
                byte[] srcBuf = null;
                using (MemoryStream stream = new MemoryStream())
                { icoIcon.Save(stream); srcBuf = stream.ToArray(); }
                const int SizeICONDIR = 6;
                const int SizeICONDIRENTRY = 16;
                int iCount = BitConverter.ToInt16(srcBuf, 4);
                for (int iIndex = 0; iIndex < iCount; iIndex++)
                {
                    int iWidth = srcBuf[SizeICONDIR + SizeICONDIRENTRY * iIndex];
                    int iHeight = srcBuf[SizeICONDIR + SizeICONDIRENTRY * iIndex + 1];
                    int iBitCount = BitConverter.ToInt16(srcBuf, SizeICONDIR + SizeICONDIRENTRY * iIndex + 6);
                    if (iWidth == 0 && iHeight == 0 && iBitCount == 32)
                    {
                        int iImageSize = BitConverter.ToInt32(srcBuf, SizeICONDIR + SizeICONDIRENTRY * iIndex + 8);
                        int iImageOffset = BitConverter.ToInt32(srcBuf, SizeICONDIR + SizeICONDIRENTRY * iIndex + 12);
                        MemoryStream destStream = new MemoryStream();
                        BinaryWriter writer = new BinaryWriter(destStream);
                        writer.Write(srcBuf, iImageOffset, iImageSize);
                        destStream.Seek(0, SeekOrigin.Begin);
                        bmpPngExtracted = new System.Drawing.Bitmap(destStream); // This is PNG! :)
                        break;
                    }
                }
            }
            catch { return null; }
            return bmpPngExtracted;
        }

        /// <param name="filename">Animated Image (ie: animated gif).</param>
        public static AnimatedImage GetAnimatedImage(string fileName)
        {
            return new AnimatedImage(GetAnimatedImageData(fileName));
        }
        /// <param name="filename">Animated Image (ie: animated gif).</param>
        public static AnimatedImageData GetAnimatedImageData(string fileName)
        {
            int index = AnimatedImageDataFileNames.IndexOf(fileName);

            if (index >= 0)
            {
                // AnimatedImageData Already Exists
                // move it to the end of the array and return it
                AnimatedImageData data = AnimatedImageDatas[index];
                string name = AnimatedImageDataFileNames[index];

                AnimatedImageDatas.RemoveAt(index);
                AnimatedImageDataFileNames.RemoveAt(index);
                AnimatedImageDatas.Add(data);
                AnimatedImageDataFileNames.Add(name);

                return AnimatedImageDatas[AnimatedImageDatas.Count-1];
            }
            else
            {
                // New AnimatedImageData
                System.Drawing.Image image = System.Drawing.Image.FromFile(fileName);
                AnimatedImageData data = new AnimatedImageData();

                //// Get Frame Duration
                int frameDuration = 0;
                try
                {
                    System.Drawing.Imaging.PropertyItem frameDelay = image.GetPropertyItem(0x5100);
                    frameDuration = (frameDelay.Value[0] + frameDelay.Value[1] * 256) * 10;
                }
                catch { }
                if (frameDuration > 10)
                    data.FrameDuration = frameDuration;
                else
                    data.FrameDuration = AnimatedImage.DEFAULT_FRAME_DURATION;
                
                //// Store AnimatedImageData
                AnimatedImageDatas.Add(data);
                AnimatedImageDataFileNames.Add(fileName);

                // Limit amount of Animations in Memory
                if (AnimatedImageDatas.Count > MAX_ANIMATIONS)
                {
                    for (int i = 0; i < AnimatedImageDatas[0].Frames.Count; i++)
                        AnimatedImageDatas[0].Frames[i].Dispose();
                    
                    AnimatedImageDatas.RemoveAt(0);
                    AnimatedImageDataFileNames.RemoveAt(0);
                }

                //// Get Frames
                LoadingAnimatedImage loadingAnimatedImage = new LoadingAnimatedImage(image, data);
                Thread loadFramesThread = new Thread(new ThreadStart(loadingAnimatedImage.LoadFrames));
                loadFramesThread.IsBackground = true;
                loadFramesThread.Start();

                while (data.Frames.Count <= 0); // wait for at least one frame to be loaded
                
                return data;
            }
        }

    }

    class LoadingAnimatedImage
    {
        private System.Drawing.Image Image;
        private ImageManipulation.OctreeQuantizer Quantizer;
        private AnimatedImageData Data;

        public LoadingAnimatedImage(System.Drawing.Image image, AnimatedImageData data)
        {
            Image = image;
            Data = data;
        }

        public void LoadFrames()
        {
            System.Drawing.Imaging.FrameDimension frameDimension = new System.Drawing.Imaging.FrameDimension(Image.FrameDimensionsList[0]);
            Data.FramesCount = Image.GetFrameCount(frameDimension);

            for (int i = 0; i < Image.GetFrameCount(frameDimension); i++)
            {
                Image.SelectActiveFrame(frameDimension, i);
                Quantizer = new ImageManipulation.OctreeQuantizer(255, 8);

                System.Drawing.Bitmap quantized = Quantizer.Quantize(Image);
                MemoryStream stream = new MemoryStream();
                quantized.Save(stream, System.Drawing.Imaging.ImageFormat.Gif);
                Data.Frames.Add(new Texture(stream));

                stream.Dispose();

                Data.Frames[i].Smooth = true;
            }
        }
    }

}
