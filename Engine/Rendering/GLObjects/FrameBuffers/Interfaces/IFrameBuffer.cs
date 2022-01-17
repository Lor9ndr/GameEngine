using System;

namespace Engine.GLObjects.FrameBuffers.Interfaces
{
    public interface IFrameBuffer : IDisposable
    {
        public int FBO { get; }
        public void Setup();
    }
}
