using GameEngine.Enums;
using GameEngine.GameObjects.Base;
using GameEngine.RenderPrepearings;
using GameEngine.Structs;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine.GameObjects
{
    public class Mesh : AGameObject
    {
        public List<Texture> Textures;
        public Mesh(Vertex[] vertices, int[] indices = null, List<Texture> textures = null, VAO vao = null, SetupLevel setupLevel = SetupLevel.Bitanget) 
            : base(vertices, indices)
        {
            Textures = textures;
            Setup(setupLevel, vao);
        }
        public void Render(Shader shader, TextureTarget target = TextureTarget.Texture2D)
        {
            shader.Use();
            for (int i = 0; i < Textures?.Count; i++)
            {
                string name = Textures[i].Type;
                shader.SetInt($"material.{name}", i);
                shader.SetInt(name, i);

                Textures[i].Use(TextureUnit.Texture0 + i, target);
            }
            base.Draw();

        }
    }
}
