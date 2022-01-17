using Engine.Rendering.Enums;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using PixelFormat = OpenTK.Graphics.OpenGL4.PixelFormat;

namespace Engine.GLObjects.Textures
{
    public class CubeMap : Texture
    {
        public CubeMap() : base()
        {

        }
        public CubeMap(int glHandle, TextureType type) : base(glHandle, type)
        {
        }
        public override void SetTexParameters(Vector2i size, PixelInternalFormat format, PixelType type)
        {
            Bind();
            for (int i = 0; i < 6; i++)
            {

                Game.EngineGL.TexImage2D(TextureTarget.TextureCubeMapPositiveX + i,
                    0, format, size.X, size.Y, 0, (PixelFormat)format, type, (IntPtr)null);
            }

            Game.EngineGL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear)
            .TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear)
            .TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge)
            .TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge)
            .TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR, (int)TextureWrapMode.ClampToEdge);
        }

        public static CubeMap LoadCubeMapFromFile(string[] faces, TextureType type)
        {
            Game.EngineGL.GenTexture(out int handle)
                .BindTexture(TextureTarget.TextureCubeMap, handle);

            for (int i = 0; i < faces.Length; i++)
            {
                string path = Game.SKYBOX_TEXTURES_PATH + faces[i];
                // Load the image
                using var image = new Bitmap(path);

                var data = image.LockBits(
                    new Rectangle(0, 0, image.Width, image.Height),
                    ImageLockMode.ReadOnly,
                    System.Drawing.Imaging.PixelFormat.Format32bppArgb);


                Game.EngineGL.TexImage2D(TextureTarget.TextureCubeMapPositiveX + i,
                    0,
                    PixelInternalFormat.Rgba,
                    image.Width,
                    image.Height,
                    0,
                    PixelFormat.Bgra,
                    PixelType.UnsignedByte,
                    data.Scan0);
            }
            Game.EngineGL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear)
                        .TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear)
                        .TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge)
                        .TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge)
                        .TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR, (int)TextureWrapMode.ClampToEdge);
            return new CubeMap(handle, type);
        }

        public override void Bind(TextureTarget type = default) => GL.BindTexture(TextureTarget.TextureCubeMap, Id);

    }
}
