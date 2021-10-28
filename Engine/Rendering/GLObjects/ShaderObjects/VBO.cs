using Engine.Structs;
using OpenTK.Graphics.OpenGL4;
using System;

namespace Engine.GLObjects
{
    public class VBO
    {
        public int Vbo;
        public void Setup(Vertex[] vertices)
        {
            Vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, Vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * Vertex.Size, vertices, BufferUsageHint.StaticDraw);
        }
        public void SetupInstances(int instances)
        {
            Vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, Vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, instances, (IntPtr)null, BufferUsageHint.DynamicDraw);
        }
    }
}
