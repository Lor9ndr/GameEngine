using GameEngine.Extensions;
using GameEngine.GameObjects;
using GameEngine.GameObjects.Lights;
using GameEngine.RenderPrepearings.FrameBuffers.Base;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameEngine.RenderPrepearings.FrameBuffers
{
    public class DirectShadows: DepthTextureBuffer
    {
        #region Public Properties
        public float NearPlane = 1.0f;
        public float FarPlane = 10000.0f;
        #endregion

        #region Constructors

        public DirectShadows(FrameBuffer fbo, Shader depthShader, TextureUnit unit)
            : base(depthShader, fbo, unit)
        {
        }
        #endregion

        #region Public Methods
        public override void Setup()
        {
            Texture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, Texture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent, ShadowFrameBuffer.ShadowSize.X, ShadowFrameBuffer.ShadowSize.Y, 0, PixelFormat.DepthComponent, PixelType.Float, (IntPtr)null);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            float[] borderColor = { 1.0f, 1.0f, 1.0f, 1.0f };
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBorderColor, borderColor);

            FBO.Bind();
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, Texture, 0);
            GL.DrawBuffer(DrawBufferMode.None);
            GL.ReadBuffer(ReadBufferMode.None);
            if (!FBO.CheckState())
            {
                throw new Exception("ERROR");
            }           
        }

        public override void Render(Camera camera, WorldRenderer wr, Light light)
        {
            ConfigureShaderAndMatrices(DepthShader, camera, light);
            GL.Viewport(0, 0, ShadowFrameBuffer.ShadowSize.X, ShadowFrameBuffer.ShadowSize.Y);
            GL.Clear(ClearBufferMask.DepthBufferBit);
            wr.Render(camera, DepthShader ,false, false);
            FBO.Unbind();
        }
        public void ConfigureShaderAndMatrices(Shader shader, Camera camera, Light light)
        {
            Matrix4 lightProjection = Matrix4.CreateOrthographicOffCenter(-FarPlane, FarPlane, -FarPlane, FarPlane, 0.1f, FarPlane);
            Matrix4 lightView = Matrix4.LookAt(light.Transform.Position, light.Transform.Direction, Vector3.UnitY);
            Matrix4 lightSpaceMatrix = lightView.Multiply(lightProjection);
            shader.Use();
            shader.SetMatrix4("lightSpaceMatrix", lightSpaceMatrix);
            shader.SetFloat("far_plane", FarPlane);
            shader.SetFloat("near_plane", NearPlane);
        }
       
        #endregion



    }
}
