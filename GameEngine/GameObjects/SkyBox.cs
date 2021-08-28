
using GameEngine.GameObjects.Base;
using GameEngine.Intefaces;
using GameEngine.Structs;
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

namespace GameEngine.GameObjects
{
    public class SkyBox: IRenderable
    {
        private Shader _skyBoxShader = new Shader(Game.SKYBOX_SHADER_PATH + ".vs", Game.SKYBOX_SHADER_PATH + ".fr");
        private Mesh skyBoxMesh;

        private readonly Vertex[] skyboxVertices = {
        new Vertex(-1.0f,  1.0f, -1.0f),
        new Vertex(-1.0f, -1.0f, -1.0f),
        new Vertex(1.0f, -1.0f, -1.0f),
        new Vertex(1.0f, -1.0f, -1.0f),
        new Vertex(1.0f,  1.0f, -1.0f),
        new Vertex(-1.0f,  1.0f, -1.0f),

        new Vertex(-1.0f, -1.0f,  1.0f),
        new Vertex(-1.0f, -1.0f, -1.0f),
        new Vertex(-1.0f,  1.0f, -1.0f),
        new Vertex(-1.0f,  1.0f, -1.0f),
        new Vertex(-1.0f,  1.0f,  1.0f),
        new Vertex(-1.0f, -1.0f,  1.0f),

        new Vertex(1.0f, -1.0f, -1.0f),
        new Vertex( 1.0f, -1.0f,  1.0f),
        new Vertex( 1.0f,  1.0f,  1.0f),
        new Vertex( 1.0f,  1.0f,  1.0f),
        new Vertex( 1.0f,  1.0f, -1.0f),
        new Vertex( 1.0f, -1.0f, -1.0f),

        new Vertex(-1.0f, -1.0f,  1.0f),
        new Vertex(-1.0f,  1.0f,  1.0f),
        new Vertex( 1.0f,  1.0f,  1.0f),
        new Vertex( 1.0f,  1.0f,  1.0f),
        new Vertex( 1.0f, -1.0f,  1.0f),
        new Vertex(-1.0f, -1.0f,  1.0f),

        new Vertex(-1.0f,  1.0f, -1.0f),
        new Vertex( 1.0f,  1.0f, -1.0f),
        new Vertex( 1.0f,  1.0f,  1.0f),
        new Vertex( 1.0f,  1.0f,  1.0f),
        new Vertex(-1.0f,  1.0f,  1.0f),
        new Vertex(-1.0f,  1.0f, -1.0f),

        new Vertex(-1.0f, -1.0f, -1.0f),
        new Vertex(-1.0f, -1.0f,  1.0f),
        new Vertex( 1.0f, -1.0f, -1.0f),
        new Vertex( 1.0f, -1.0f, -1.0f),
        new Vertex(-1.0f, -1.0f,  1.0f),
        new Vertex( 1.0f, -1.0f,  1.0f)
    };
        public readonly string[] faces =
        {
            "right.jpg",
            "left.jpg",
            "top.jpg",
            "bottom.jpg",
            "front.jpg",
            "back.jpg"
        };
        private readonly int _cubemapTexture;
      
        public SkyBox()
        {
            ResizeVerticesToSkybox();
            skyBoxMesh = new Mesh(skyboxVertices, setupLevel: Enums.SetupLevel.Position);
            _cubemapTexture = LoadCubeMap();
            _skyBoxShader.Use();
            _skyBoxShader.SetInt("skybox", 0);
        }
        private void ResizeVerticesToSkybox()
        {
            for (int i = 0; i < skyboxVertices.Length; i++)
            {
                skyboxVertices[i].Position *= 100000;
            }
        }
        private void ResizeVerticesToDefault() 
        {
            for (int i = 0; i < skyboxVertices.Length; i++)
            {
                skyboxVertices[i].Position /= 100000;
            }
        }
        private int LoadCubeMap()
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
            return handle;
        }

        public void Render(Camera camera)
        {
            GL.DepthFunc(DepthFunction.Lequal);
            _skyBoxShader.Use();
            var viewproj = camera.GetViewMatrix() * camera.GetProjectionMatrix();
            _skyBoxShader.SetMatrix4("viewproj", viewproj);
            GL.BindVertexArray(skyBoxMesh.ObjectSetupper.GetVAO());
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.TextureCubeMap, _cubemapTexture);
            GL.DrawArrays(PrimitiveType.Triangles, 0, skyBoxMesh.VerticesCount);
            GL.BindVertexArray(0);
            GL.DepthFunc(DepthFunction.Less);
        }

        public void Update() { }

        public void Render(Camera camera, Shader shader) => Render(camera);

        public void Render(Shader shader)
        {
            
        }
    }
}
