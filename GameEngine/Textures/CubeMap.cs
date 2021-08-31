using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PixelFormat = OpenTK.Graphics.OpenGL4.PixelFormat;

namespace GameEngine.Textures
{
    public class CubeMap : Texture
    {
        public CubeMap() : base()
        {

        }
        public CubeMap(int glHandle,string type) : base(glHandle, type)
        {

        }
        public override void SetTexParameters(Vector2i size, PixelInternalFormat format, PixelType type)
        {
            Bind();
            for (int i = 0; i < 6; i++)
            {
              
                GL.TexImage2D(TextureTarget.TextureCubeMapPositiveX + i,
                    0,format,size.X,size.Y,0, (PixelFormat)format,type,(IntPtr)null);
            }

            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR, (int)TextureWrapMode.ClampToEdge);
        }
        public static CubeMap LoadCubeMapFromFile(string[] faces, string type)
        {
            int handle = GL.GenTexture();
            GL.BindTexture(TextureTarget.TextureCubeMap, handle);

            for (int i = 0; i < faces.Length; i++)
            {
                string path = Game.SKYBOX_TEXTURES_PATH + faces[i];
                // Load the image
                using var image = new Bitmap(path);

                var data = image.LockBits(
                    new Rectangle(0, 0, image.Width, image.Height),
                    ImageLockMode.ReadOnly,
                    System.Drawing.Imaging.PixelFormat.Format32bppArgb);


                GL.TexImage2D(TextureTarget.TextureCubeMapPositiveX + i,
                    0,
                    PixelInternalFormat.Rgba,
                    image.Width,
                    image.Height,
                    0,
                    PixelFormat.Bgra,
                    PixelType.UnsignedByte,
                    data.Scan0);
            }
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR, (int)TextureWrapMode.ClampToEdge);
            return new CubeMap(handle, type);
        }
       
        public override void Bind(TextureTarget type = default) => GL.BindTexture(TextureTarget.TextureCubeMap, Handle);

    }
}
