using GameEngine.Structs;
using OpenTK.Graphics.OpenGL4;
using System;

namespace GameEngine.RenderPrepearings
{
    public class VAO
    {
        public int Vao;
        public void Setup(Vertex[] vertices)
        {
            Vao = GL.GenVertexArray();
            GL.BindVertexArray(Vao);
        }
    }
}
