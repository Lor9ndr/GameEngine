using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine.Intefaces.FrameBuffer
{
    public interface IFrameBuffer
    {
        public int FBO { get; }
        public void Setup();
        public void Render(Camera camera);
    }
}
