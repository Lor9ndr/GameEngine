using GameEngine.DefaultMeshes;
using GameEngine.Extensions;
using GameEngine.GameObjects;
using GameEngine.GameObjects.Lights;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine.RenderPrepearings.FrameBuffers
{
    public class DirectShadows
    {
        #region Private Properties
        private int _depthMapFBO;
        private int _depthMap;
        private Shader _depthShader;
        private List<Light> _lights = new List<Light>();
        private TextureUnit _shadowTextureUnit;

        #endregion

        #region Public Properties
        public float NearPlane = 1.0f;
        public float FarPlane = 1000.0f;
        #endregion

        #region Constructors
        public DirectShadows(int fbo, Shader depth, TextureUnit shadowTextureUnit)
        {
            _depthMapFBO = fbo;
            _depthShader = depth;
            _shadowTextureUnit = shadowTextureUnit;
        }

        #endregion

        #region Public Methods
        public void Setup()
        {
            _depthMap = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, _depthMap);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent, ShadowFrameBuffer.ShadowSize.X, ShadowFrameBuffer.ShadowSize.Y, 0, PixelFormat.DepthComponent, PixelType.Float, (IntPtr)null);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            float[] borderColor = { 1.0f, 1.0f, 1.0f, 1.0f };
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBorderColor, borderColor);

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, _depthMapFBO);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, _depthMap, 0);
            GL.DrawBuffer(DrawBufferMode.None);
            GL.ReadBuffer(ReadBufferMode.None);
            if (GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) == FramebufferErrorCode.FramebufferComplete)
            {
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            }
            else
            {
                throw new Exception("FrameBuffer not completed");
            }


        }

        public void RenderBuffer(Camera camera, WorldRenderer wr)
        {
            foreach (var item in _lights.OfType<DirectLight>())
            {
                ConfigureShaderAndMatrices(_depthShader, camera, item);
            }

            GL.Viewport(0, 0, ShadowFrameBuffer.ShadowSize.X, ShadowFrameBuffer.ShadowSize.Y);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, _depthMapFBO);
            GL.Clear(ClearBufferMask.DepthBufferBit);
            wr.Render(camera, _depthShader);
        }
        public void ConfigureShaderAndMatrices(Shader shader, Camera camera, Light light)
        {
            Matrix4 lightProjection = Matrix4.CreateOrthographicOffCenter(-FarPlane, FarPlane, -FarPlane, FarPlane, NearPlane, FarPlane);
            Matrix4 lightView = Matrix4.LookAt(light.Position, light.Direction, Vector3.UnitY);
            Matrix4 lightSpaceMatrix = lightProjection.Multiply(lightView);
            shader.Use();
            shader.SetMatrix4("lightSpaceMatrix", lightSpaceMatrix);
            shader.SetFloat("far_plane", FarPlane);
            shader.SetFloat("near_plane", NearPlane);
        }
        public void BindTexture()
        {
            GL.ActiveTexture(_shadowTextureUnit);
            GL.BindTexture(TextureTarget.Texture2D, _depthMap);
        }
        #endregion



    }
}
