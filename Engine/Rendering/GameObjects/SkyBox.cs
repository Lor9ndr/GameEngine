using Engine.Components;
using Engine.GLObjects.Textures;
using Engine.Rendering.Enums;
using Engine.Rendering.GameObjects;
using Engine.Structs;
using GameEngine.Enums;
using OpenTK.Graphics.OpenGL4;
using System.Collections.Generic;
using System.Linq;

namespace Engine.GameObjects
{
    public class SkyBox: GameObject
    {
        private Shader _skyBoxShader = new Shader(Game.SKYBOX_SHADER_PATH + ".vs", Game.SKYBOX_SHADER_PATH + ".fr");

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
            Game.SKYBOX_TEXTURES_PATH + "right.jpg",
            Game.SKYBOX_TEXTURES_PATH + "left.jpg",
            Game.SKYBOX_TEXTURES_PATH + "top.jpg",
            Game.SKYBOX_TEXTURES_PATH + "bottom.jpg",
            Game.SKYBOX_TEXTURES_PATH + "front.jpg",
            Game.SKYBOX_TEXTURES_PATH + "back.jpg"
        };
        private readonly CubeMap _cubemapTexture;
      
        public SkyBox()
            :base(null)
        {
            ResizeVerticesToSkybox();
            _cubemapTexture = CubeMap.LoadCubeMapFromFile(faces,"skybox");
            List<Texture> _ = new List<Texture>() { _cubemapTexture };
            Model = new Model(new Mesh(skyboxVertices,textures: _ , setupLevel: SetupLevel.Position));
            
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
        public void Render( Camera camera, RenderFlags flags)
        {
            GL.DepthFunc(DepthFunction.Lequal);
            _skyBoxShader.Use();
            var viewproj = camera.GetViewMatrix() * camera.GetProjectionMatrix();
            _skyBoxShader.SetMatrix4("VP", viewproj);
            Model.Meshes.First().Render(_skyBoxShader, flags, TextureTarget.TextureCubeMap);
            GL.DepthFunc(DepthFunction.Less);
        }


        public virtual void Render(Camera camera, RenderFlags flags, Shader shader) => Render(camera,flags);
    }
}
