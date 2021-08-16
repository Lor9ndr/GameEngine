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
        public void Render(Shader shader)
        {
            int diffuseNr = 0;
            int specularNr = 0;
            int normalNr = 0;
            int heightNr = 0;
            for (int i = 0; i < Textures?.Count; i++)
            {
                string number = string.Empty;
                string name = Textures[i].Type;
                if (name == "texture_diffuse")
                    number = diffuseNr++.ToString();
                else if (name == "texture_specular")
                    number = specularNr++.ToString();
                else if (name == "texture_normal")
                    number = normalNr++.ToString();
                else if (name == "texture_height")
                    number = heightNr++.ToString();
                Textures[i].Use(TextureUnit.Texture0 + i);
                shader.SetInt($"material.{name + number}", i);
            }
            base.Draw();
        }
    }
}
