using GameEngine.Intefaces.FrameBuffer;
using GameEngine.Textures;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine.RenderPrepearings.FrameBuffers.Base
{
    public class FrameBuffer : IFrameBuffer
    {
        public int FBO { get => _fbo; protected set { _fbo = value; } }
        public Vector2i Size { get => _size; set => _size = value; }
        public CubeMap CubeMap { get => _cubeMap;}
        public Texture Texture { get => _texture; }

        private Texture _texture;
        private CubeMap _cubeMap;
        private Vector2i _size;
        private ClearBufferMask _bufferMask;
        private int _fbo;

        public FrameBuffer( Vector2i size, ClearBufferMask bufferMask)
        {
            _size = size;
            _bufferMask = bufferMask;
        }

        public void Bind() => GL.BindFramebuffer(FramebufferTarget.Framebuffer, _fbo);
        public void Unbind() => GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        public virtual void Setup() => _fbo = GL.GenFramebuffer();


        public bool CheckState()
        {
            if (GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) == FramebufferErrorCode.FramebufferComplete)
            {
                Unbind();
                return true;
            }
            else
            {
                throw new Exception(GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer).ToString());
            }
        }
        public void Clear() => GL.Clear(_bufferMask);
        public void Activate()
        {
            Bind();
            SetViewPort();
            Clear();
        }

        public void SetViewPort() => GL.Viewport(0, 0, Size.X, Size.Y);
        public void AttachCubeMap(FramebufferAttachment attachment, PixelInternalFormat format, PixelType type )
        {
            _cubeMap = new CubeMap();
            _cubeMap.SetTexParameters(Size, format, type);
            GL.FramebufferTexture(FramebufferTarget.Framebuffer, attachment, _cubeMap.Handle,0);
            CheckState();
           
        }
        public void AttachTexture2DMap(FramebufferAttachment attachment, PixelInternalFormat format, PixelType type)
        {
            _texture = new Texture();
            _texture.SetTexParameters(Size, format, type);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, attachment,TextureTarget.Texture2D, _texture.Handle, 0);
           CheckState();
           
        }
        public void DisableColorBuffer()
        {
            GL.DrawBuffer(DrawBufferMode.None);
            GL.ReadBuffer(ReadBufferMode.None);
        }
    }
}
