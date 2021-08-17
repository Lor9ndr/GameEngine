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
    public class PointShadows
    {
        public Vector2i ShadowSize = new Vector2i(2048, 2048);
        public float NearPlane = 0.1f;
        public float FarPlane = 25.0f;

        private WorldRenderer _wr;
        private List<Light> _pointLights = new List<Light>();
        private int _depthMapFBO;
        private int _depthCubeMap;
        private TextureUnit _shadowTextureUnit = TextureUnit.Texture30;
        private Mesh _debugScreen = FullScreenQuad.GetQuad();

        private Shader _simpleShader = new Shader(Game.POINT_SHADOW_SHADERS_PATH + "SimpleShader.vs", Game.POINT_SHADOW_SHADERS_PATH + "SimpleShader.fr");
        private Shader _depthShader = new Shader(Game.POINT_SHADOW_SHADERS_PATH + "Depth.vs", Game.POINT_SHADOW_SHADERS_PATH + "Depth.fr", Game.POINT_SHADOW_SHADERS_PATH + "Depth.gs");
        private Shader _debugShader = new Shader(Game.SHADOW_SHADERS_PATH + "DepthScreen.vs", Game.SHADOW_SHADERS_PATH + "DepthScreen.fr");

        public PointShadows(int shadowWidth, int shadowHeight, WorldRenderer wr)
          : this(wr)
        {
            ShadowSize = new Vector2i(shadowWidth, shadowHeight);

        }
        public PointShadows(WorldRenderer wr)
        {
            _wr = wr;
            _pointLights.AddRange(_wr.Lights.OfType<PointLight>().ToList());
        }
        public PointShadows(Vector2i shadowSize, WorldRenderer wr)
            : this(wr)
        {
            ShadowSize = shadowSize;
        }
        public void Setup()
        {
            _depthMapFBO = GL.GenFramebuffer();
            _depthCubeMap = GL.GenTexture();
            GL.BindTexture(TextureTarget.TextureCubeMap, _depthCubeMap);
            for (int i = 0; i < 6; i++)
            {
                GL.TexImage2D(TextureTarget.TextureCubeMapPositiveX + i, 0, PixelInternalFormat.DepthComponent, ShadowSize.X, ShadowSize.Y, 0, PixelFormat.DepthComponent, PixelType.Float, (IntPtr)null);
            }
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR, (int)TextureWrapMode.ClampToEdge);

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, _depthMapFBO);
            GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, _depthCubeMap, 0);
            GL.DrawBuffer(DrawBufferMode.None);
            GL.ReadBuffer(ReadBufferMode.None);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            _simpleShader.Use();
            _simpleShader.SetInt("shadowMap", 30);
        }
        public void RenderBuffer(Camera camera)
        {
            _pointLights.Clear();
            _pointLights.AddRange(_wr.Lights.OfType<PointLight>().ToList());
            _pointLights.AddRange(_wr.Lights.OfType<SpotLight>().ToList());
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit );

            List<List<Matrix4>> shadowTransforms = new List<List<Matrix4>>();
            foreach (var item in _pointLights)
            {
                Matrix4 shadowProj = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(90), ShadowSize.X / ShadowSize.Y, NearPlane, FarPlane);
                shadowTransforms.Add(
                new List<Matrix4>()
                {
                     Matrix4.LookAt(item.Position, item.Position + new Vector3(1.0f, 0.0f, 0.0f), new Vector3(0.0f, -1.0f, 0.0f)) * shadowProj ,
                     Matrix4.LookAt(item.Position, item.Position + new Vector3(-1.0f, 0.0f, 0.0f), new Vector3(0.0f, -1.0f, 0.0f))* shadowProj,
                     Matrix4.LookAt(item.Position, item.Position + new Vector3(0.0f, 1.0f, 0.0f), new Vector3(0.0f, 0.0f, 1.0f))* shadowProj,
                     Matrix4.LookAt(item.Position, item.Position + new Vector3(0.0f, -1.0f, 0.0f), new Vector3(0.0f, 0.0f, -1.0f))* shadowProj,
                    Matrix4.LookAt(item.Position, item.Position + new Vector3(0.0f, 0.0f, 1.0f), new Vector3(0.0f, -1.0f, 0.0f))* shadowProj,
                     Matrix4.LookAt(item.Position, item.Position + new Vector3(0.0f, 0.0f, -1.0f), new Vector3(0.0f, -1.0f, 0.0f))* shadowProj,
                });
            }
           

            GL.Viewport(0, 0, ShadowSize.X, ShadowSize.Y);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, _depthMapFBO);
            GL.Clear(ClearBufferMask.DepthBufferBit);

            _depthShader.Use();

            for (int i = 0; i < 6; i++)
            {
                _depthShader.SetMatrix4($"shadowMatrices[{i}]", shadowTransforms[0][i]);
            }
            _depthShader.SetVector3("lightPos", _pointLights[0].Position);
            _depthShader.SetFloat("far_plane", FarPlane);
            _wr.Render(camera, _depthShader);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

            GL.Viewport(0, 0, Game.WIDTH, Game.HEIGHT);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            _simpleShader.Use();
            _simpleShader.SetFloat("far_plane", FarPlane);
            _simpleShader.SetInt("shadows", Game.Shadows);
            GL.ActiveTexture(_shadowTextureUnit);
            GL.BindTexture(TextureTarget.TextureCubeMap, _depthCubeMap);
            _wr.Render(camera, _simpleShader);
        }

    }
}
