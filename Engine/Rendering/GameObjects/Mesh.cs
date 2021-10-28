using Engine.GameObjects.Base;
using Engine.GLObjects;
using Engine.GLObjects.Textures;
using Engine.Rendering.Enums;
using Engine.Structs;
using GameEngine.Enums;
using OpenTK.Graphics.OpenGL4;
using System.Collections.Generic;

namespace Engine.Rendering.GameObjects
{
    public class Mesh : OpenGLObject
    {
        public List<Texture> Textures;
        public Mesh(Vertex[] vertices, int[] indices = null, List<Texture> textures = null, VAO vao = null, SetupLevel setupLevel = SetupLevel.Bitanget) 
            : base(vertices, indices)
        {
            Textures = textures;
            Setup(setupLevel, vao);
        }
        public void Render(Shader shader, RenderFlags flags, TextureTarget target = TextureTarget.Texture2D)
        {
            shader.Use();
            if (flags.HasFlag(RenderFlags.Textures))
            {
                for (int i = 0; i < Textures?.Count; i++)
                {
                    string name = Textures[i].Type;
                    shader.SetInt($"material.{name}", i);
                    shader.SetInt(name, i);
                    Textures[i].Use(TextureUnit.Texture0 + i, target);
                }
            }
            if (flags.HasFlag(RenderFlags.Mesh))
            {
                base.Draw();
            }

        }
    }
}
