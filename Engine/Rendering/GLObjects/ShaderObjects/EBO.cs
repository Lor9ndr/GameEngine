using OpenTK.Graphics.OpenGL4;
using System;

namespace Engine.GLObjects
{
    public class EBO : IDisposable
    {
        public int Ebo;
        public void Setup(uint[] indices)
        {
            Game.EngineGL.GenBuffer(out Ebo)
                .BindBuffer(BufferTarget.ElementArrayBuffer, Ebo)
                .BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(indices.Length * sizeof(uint)), indices, BufferUsageHint.StaticDraw);
        }
        public void Dispose()
        {
            GL.DeleteBuffer(Ebo);

        }
    }
}
