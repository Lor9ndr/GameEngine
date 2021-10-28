using Engine.GLObjects.FrameBuffers.Interfaces;
using Engine.GLObjects.Textures;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.GLObjects.FrameBuffers
{
    public class FrameBuffer : IFrameBuffer
    {
        public int FBO { get => _fbo; set => _fbo = value; }
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
            _fbo = 0;
        }
        
        public void Bind() => GL.BindFramebuffer(FramebufferTarget.Framebuffer, _fbo);
        public void Unbind() => GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        public void Setup() => _fbo = GL.GenFramebuffer();
        public void Clear() => GL.Clear(_bufferMask);
        public void Activate()
        {
            Bind();
            SetViewPort();
            Clear();
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
                throw new Exception(GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer).ToString());
            }
        }
       

        public void SetViewPort() => GL.Viewport(0, 0, Size.X, Size.Y);
        public void AttachCubeMap(FramebufferAttachment attachment, PixelInternalFormat format, PixelType type )
        {
            _cubeMap = new CubeMap();
            _cubeMap.SetTexParameters(Size, format, type);
            GL.FramebufferTexture(FramebufferTarget.Framebuffer, attachment, _cubeMap.Handle, 0);
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
        public void DeleteBuffer()
        {
            if (_texture != null)
            {
                GL.DeleteTexture(_texture.Handle);
            }
            else
            {
                GL.DeleteTexture(_cubeMap.Handle);
            }
            GL.DeleteBuffer(_fbo);
        }
    }
}
