using GameEngine.DefaultMeshes;
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
    public class ShadowBuffers
    {
        #region Private Properties
        private int _depthMapFBO;
        private int _depthMap;
        private Shader _depthShader;
        private Shader _lightShader;
        private Shader _debugShader;
        private DirectLight _dirLight;
        private Mesh _debugScreen = FullScreenQuad.GetQuad(); 
        private WorldRenderer _wr;
        #endregion

        #region Public Properties
        public Vector2i ShadowSize = new Vector2i(1024, 1024);
        public float NearPlane = 0.1f;
        public float FarPlane = 7.5f;
        #endregion

        #region Constructors
        public ShadowBuffers(int shadowWidth, int shadowHeight, WorldRenderer wr)
            :this(wr)
        {
            ShadowSize = new Vector2i(shadowWidth, shadowHeight);

        }
        public ShadowBuffers(WorldRenderer wr)
        {
            _wr = wr;
            _dirLight = _wr.Lights.OfType<DirectLight>().FirstOrDefault();
        }
        public ShadowBuffers(Vector2i shadowSize, WorldRenderer wr)
            :this(wr)
        {
            ShadowSize = shadowSize;
        }
        #endregion

        #region Public Methods
        public void Setup()
        {
            _depthMapFBO = GL.GenFramebuffer();

            _depthMap = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, _depthMap);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent, ShadowSize.X, ShadowSize.Y, 0, PixelFormat.DepthComponent, PixelType.Float, (IntPtr)null);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
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
            }
            _depthShader = new Shader(Game.SHADOW_SHADERS_PATH + "Depth.vs", Game.SHADOW_SHADERS_PATH + "Depth.fr");
            _lightShader = new Shader(Game.SHADOW_SHADERS_PATH + "SimpleShader.vs", Game.SHADOW_SHADERS_PATH + "SimpleShader.fr");
            _debugShader = new Shader(Game.SHADOW_SHADERS_PATH + "DepthScreen.vs", Game.SHADOW_SHADERS_PATH + "DepthScreen.fr");


        }

        public void RenderBuffer(Camera camera)
        {
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);
            GL.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.Viewport(0, 0, ShadowSize.X, ShadowSize.Y);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, _depthMapFBO);
            GL.Clear(ClearBufferMask.DepthBufferBit);
            ConfigureShaderAndMatrices(_depthShader);
            _wr.Render(camera, _depthShader);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

            /*_debugShader.Use();
            _debugShader.SetFloat("near_plane", NearPlane);
            _debugShader.SetFloat("far_plane", FarPlane);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, _depthMap);
            _debugScreen.Draw(PrimitiveType.TriangleStrip);
*/
            GL.Viewport(0, 0, Game.WIDTH, Game.HEIGHT);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            ConfigureShaderAndMatrices(_lightShader);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, _depthMap);
            _wr.Render(camera, _lightShader);
        }
        #endregion
        private void ConfigureShaderAndMatrices(Shader shader)
        {
            Matrix4 lightProjection = Matrix4.CreateOrthographicOffCenter(10, -10, 10, -10,  NearPlane, FarPlane);
            Matrix4 lightView = Matrix4.LookAt(_dirLight.Position, _dirLight.Direction, Vector3.UnitY);
            Matrix4 lightSpaceMatrix =  lightView * lightProjection;
            shader.Use();
            shader.SetMatrix4("lightSpaceMatrix", lightSpaceMatrix);
            shader.SetFloat("far_plane", FarPlane);
            shader.SetFloat("near_plane", NearPlane);
        }
    }
}
