using GameEngine.GameObjects;
using GameEngine.Intefaces.FrameBuffer;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine.RenderPrepearings.FrameBuffers.Base
{
    public abstract class DepthTextureBuffer : TextureBuffer, IDepthBuffer
    {
        public Shader DepthShader { get; set; }
        public DepthTextureBuffer(Shader depthShader,FrameBuffer fbo, TextureUnit unit)
            : base(fbo,unit)
        {
            DepthShader = depthShader;
        }
    }
}
