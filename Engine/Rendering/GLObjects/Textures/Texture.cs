using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using PixelFormat = OpenTK.Graphics.OpenGL4.PixelFormat;

namespace Engine.GLObjects.Textures
{
    public class Texture
    {
        public readonly int Handle;
        public readonly string Type;
        public readonly string Path;
        public Texture(int glHandle, string type, string path) : this(glHandle,type)
        {
            Path = path;
        }
        public Texture(int glHandle)
        {
            Handle = glHandle;
        }
        public Texture(int glHandle, string type) : this(glHandle)
        {
            Type = type;
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
                    PixelInternalFormat.SrgbAlpha,
                    image.Width,
                    image.Height,
                    0,
                    PixelFormat.Bgra,
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
        public virtual void SetTexParameters(Vector2i size, PixelInternalFormat format, PixelType type) => SetTexParameters(size, format,(PixelFormat)format, type);


        public virtual void SetTexParameters(Vector2i size, PixelInternalFormat format, PixelFormat anotherFormat , PixelType type)
        {
            Bind(TextureTarget.Texture2D);
            GL.TexImage2D(TextureTarget.Texture2D, 0, format, size.X, size.Y, 0, anotherFormat, type, (IntPtr)null);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            float[] borderColor = { 1.0f, 1.0f, 1.0f, 1.0f };
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBorderColor, borderColor);
        }
        public void Use(TextureUnit unit)
        {
            GL.ActiveTexture(unit);
            GL.BindTexture(TextureTarget.Texture2D, Handle);
        }
        public void Use(TextureUnit unit, TextureTarget taget)
        {
            GL.ActiveTexture(unit);
            GL.BindTexture(taget, Handle);
        }
    }
}
