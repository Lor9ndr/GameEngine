using Engine.GLObjects.Textures;
using Engine.Rendering.Enums;
using Engine.Rendering.GameObjects;
using Engine.Structs;
using GameEngine.Enums;
using OpenTK.Graphics.OpenGL4;
using System.Collections.Generic;

namespace Engine.GameObjects
{
    public class SkyBox : GameObject
    {
        private readonly Shader _skyBoxShader = new Shader(Game.SKYBOX_SHADER_PATH + ".vs", Game.SKYBOX_SHADER_PATH + ".fr");

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
        private readonly CubeMap _cubemapTexture;

        public SkyBox()
            : base(null)
        {
            ResizeVerticesToSkybox();
            _cubemapTexture = CubeMap.LoadCubeMapFromFile(faces, TextureType.Skybox);
            List<Texture> _ = new List<Texture>() { _cubemapTexture };
            Model = new Model(new Mesh(skyboxVertices, textures: _, setupLevel: SetupLevel.Position), "SkyBox");

            Game.EngineGL.UseShader(_skyBoxShader).SetShaderData("skybox", 0);
        }
        private void ResizeVerticesToSkybox()
        {
            for (int i = 0; i < skyboxVertices.Length; i++)
            {
                skyboxVertices[i].Position *= 100000;
            }
        }
        public void Render(Camera camera, RenderFlags flags)
        {
            GL.DepthFunc(DepthFunction.Lequal);
            var viewproj = camera.GetViewMatrix() * camera.GetProjectionMatrix();
            Game.EngineGL.UseShader(_skyBoxShader).SetShaderData("VP", viewproj);
            Model.Render(_skyBoxShader, flags, TextureTarget.TextureCubeMap);
            GL.DepthFunc(DepthFunction.Less);
        }

        public new void Dispose()
        {
            Model.Dispose();
            _skyBoxShader.Dispose();

        }
        public virtual void Render(Camera camera, RenderFlags flags, Shader shader) => Render(camera, flags);
    }
}
