using Engine.Structs;
using OpenTK.Graphics.OpenGL4;
using System;

namespace Engine.GLObjects
{
    public class VBO : IDisposable
    {
        public int Vbo;
        private bool _isDisposed = false;

        public void Setup(Vertex[] vertices)
        {
            Game.EngineGL.GenBuffer(out Vbo)
                .BindBuffer(BufferTarget.ArrayBuffer, Vbo)
                .BufferData(BufferTarget.ArrayBuffer, vertices.Length * Vertex.Size, vertices, BufferUsageHint.StaticDraw);
        }
        public void SetupInstances(int instances)
        {
            Game.EngineGL.GenBuffer(out Vbo)
                .BindBuffer(BufferTarget.ArrayBuffer, Vbo)
                .BufferData(BufferTarget.ArrayBuffer, instances, IntPtr.Zero, BufferUsageHint.DynamicDraw);
        }
        public void Dispose()
        {
            if (!_isDisposed)
            {
                GL.DeleteBuffer(Vbo);
                _isDisposed = true;
            }

        }
    }
}
