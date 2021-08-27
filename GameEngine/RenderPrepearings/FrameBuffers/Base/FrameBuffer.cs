using GameEngine.Intefaces.FrameBuffer;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine.RenderPrepearings.FrameBuffers.Base
{
    public abstract class FrameBuffer : IFrameBuffer
    {
        public int FBO { get => _fbo; protected set => _fbo = value; }
        public WorldRenderer WR => _wr;
        private int _fbo;
        private WorldRenderer _wr;
        public FrameBuffer(WorldRenderer wr) => _wr = wr;
        private bool _isBinded = false;

        public void Bind() 
        {
            if (!_isBinded)
            {
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, _fbo);
                _isBinded = true;
            }
            else
            {
                Unbind();
                Bind();
            }
        }
        public void Unbind()
        {
            if (_isBinded)
            {
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
                _isBinded = false;
            }
        }
        public virtual void Setup()
        {
            _fbo = GL.GenFramebuffer();
        }
        public bool CheckState()
        {
            if (GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) == FramebufferErrorCode.FramebufferComplete)
            {
                Unbind();
                return true;
            }
            else
            {
                Console.WriteLine(GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer));
                return false;
            }
        }
        public virtual void Render(Camera camera)
        {
        }
    }
}
