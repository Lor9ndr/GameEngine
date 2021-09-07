using GameEngine.Bases;
using GameEngine.Intefaces;
using GameEngine.Structs;
using GameEngine.Textures;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;

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
            Game.SKYBOX_TEXTURES_PATH + "right.jpg",
            Game.SKYBOX_TEXTURES_PATH + "left.jpg",
            Game.SKYBOX_TEXTURES_PATH + "top.jpg",
            Game.SKYBOX_TEXTURES_PATH + "bottom.jpg",
            Game.SKYBOX_TEXTURES_PATH + "front.jpg",
            Game.SKYBOX_TEXTURES_PATH + "back.jpg"
        };
        private readonly CubeMap _cubemapTexture;
      
        public SkyBox()
        {
            ResizeVerticesToSkybox();
            _cubemapTexture = CubeMap.LoadCubeMapFromFile(faces,"skybox");
            List<Texture> _ = new List<Texture>() { _cubemapTexture };
            skyBoxMesh = new Mesh(skyboxVertices,textures: _ , setupLevel: Enums.SetupLevel.Position);
            
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
            skyBoxMesh.Render(_skyBoxShader,flags, TextureTarget.TextureCubeMap);
            GL.DepthFunc(DepthFunction.Less);
        }

        public void Update() { }

        public void Render(Camera camera, RenderFlags flags, Shader shader) => Render(camera,flags);

        public void Render(Shader shader,RenderFlags flags) => throw new NotImplementedException();


        public void Dispose()
        {
            skyBoxMesh.Dispose();
        }
    }
}
