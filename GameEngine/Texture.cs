﻿using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace GameEngine
{
    public class Texture
    {
        public readonly int Handle;
        public readonly string Type;
        public readonly string Path;
        public Texture(int glHandle, string type, string path)
        {
            Handle = glHandle;
            Type = type;
            Path = path;
        }
        public Texture()
        {
            Handle = GL.GenTexture();
        }

        public static Texture LoadFromFile(string path, string type, string directory)
        {
            if (!string.IsNullOrEmpty(directory))
            {
                path = directory + '/' + path;
            }
            // Generate handle
            int handle = GL.GenTexture();
            // Bind the handle
            GL.BindTexture(TextureTarget.Texture2D, handle);


            // Load the image
            using (Bitmap image = new(path))
            {

                var data = image.LockBits(
                    new Rectangle(0, 0, image.Width, image.Height),
                    ImageLockMode.ReadOnly,
                    System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                GL.TexImage2D(TextureTarget.Texture2D,
                    0,
                    PixelInternalFormat.Rgba,
                    image.Width,
                    image.Height,
                    0,
                    OpenTK.Graphics.OpenGL4.PixelFormat.Bgra,
                    PixelType.UnsignedByte,
                    data.Scan0);
            }


            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);


            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            return new Texture(handle, type, path);
        }

        public virtual void Bind(TextureTarget type) => GL.BindTexture(type, Handle);
        public virtual void SetTexParameters(Vector2i size, PixelInternalFormat format, PixelType type) => throw new NotImplementedException();

        public void Use(TextureUnit unit)
        {
            GL.ActiveTexture(unit);
            GL.BindTexture(TextureTarget.Texture2D, Handle);
        }
    }
}
