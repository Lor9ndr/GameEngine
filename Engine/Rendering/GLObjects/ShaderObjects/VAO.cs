using Engine.Structs;
using OpenTK.Graphics.OpenGL4;
using System;

namespace Engine.GLObjects
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
