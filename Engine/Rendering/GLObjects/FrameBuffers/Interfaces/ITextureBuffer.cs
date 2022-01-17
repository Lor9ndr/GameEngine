using Engine.GameObjects.Lights;

namespace Engine.GLObjects.FrameBuffers.Interfaces
{
    public interface ITextureBuffer
    {
        public int Texture { get; }
        public void Setup();
        public void Render(Camera camera, WorldRenderer wr, Light light);
    }
}
