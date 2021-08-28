using GameEngine.GameObjects.Lights;
using GameEngine.Intefaces.FrameBuffer;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine.RenderPrepearings.FrameBuffers.Base
{
    public abstract class TextureBuffer : ITextureBuffer
    {
        public FrameBuffer FBO => _fbo;
        public int Texture { get => _texture; protected set => _texture = value; }
        public TextureUnit TextureUnit { get; set; }

        private FrameBuffer _fbo;
        private int _texture;
        public TextureBuffer(FrameBuffer fbo,TextureUnit unit)
        {
            _fbo = fbo;
            TextureUnit = unit;
        }
        public virtual void Setup()
        {
            throw new NotImplementedException();
        }

        public virtual void Render(Camera camera, WorldRenderer wr, Light light)
        {
            throw new NotImplementedException();
        }
        public void BindTexture()
        {
            GL.ActiveTexture(TextureUnit);
            GL.BindTexture(TextureTarget.Texture2D, Texture);
        }
        public void BindTexture(TextureTarget target)
        {
            GL.ActiveTexture(TextureUnit);
            GL.BindTexture(target, Texture);
        }


    }
}
