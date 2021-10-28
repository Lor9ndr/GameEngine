using Engine.GameObjects.Lights;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.GLObjects.FrameBuffers.Interfaces
{
    public interface ITextureBuffer
    {
        public int Texture { get; }
        public void Setup();
        public void Render(Camera camera, WorldRenderer wr, Light light);
    }
}
