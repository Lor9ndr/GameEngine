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
    public class ShadowFrameBuffer
    {
        public DirectShadows DirectShadows;
        public PointShadows PointShadows;

        private WorldRenderer _wr;
        private int _fbo;
        private TextureUnit PointTextureUnit = TextureUnit.Texture30;
        private TextureUnit DirectTextureUnit = TextureUnit.Texture31;
        public float NearPlane = 1.0f;
        public float FarPlane = 100.0f;
        public static Vector2i ShadowSize = new Vector2i(1024, 1024);
        public Shader DepthCubeShader = new Shader(Game.SHADOW_SHADERS_PATH + "DepthCubeMap.vs", Game.SHADOW_SHADERS_PATH + "DepthCubeMap.fr", Game.SHADOW_SHADERS_PATH + "DepthCubeMap.gs");
        public Shader DepthDirectShader = new Shader(Game.SHADOW_SHADERS_PATH + "Depth.vs", Game.SHADOW_SHADERS_PATH + "Depth.fr");
        public Shader LightShader = new Shader(Game.SHADOW_SHADERS_PATH + "SimpleShader.vs", Game.SHADOW_SHADERS_PATH + "SimpleShader.fr");
        public Shader DebugShader = new Shader(Game.SHADOW_SHADERS_PATH + "Debug.vs", Game.SHADOW_SHADERS_PATH + "Debug.fr");

        private Mesh _debug => FullScreenQuad.GetQuad();
        public ShadowFrameBuffer(WorldRenderer wr)
        {
            _wr = wr;
        }
        public void Setup()
        {
            _fbo = GL.GenFramebuffer();
            //DirectShadows = new DirectShadows(_fbo, DepthDirectShader, DirectTextureUnit);
            PointShadows = new PointShadows(_fbo, DepthCubeShader, PointTextureUnit);
            //DirectShadows.Setup();
            PointShadows.Setup();
            LightShader.Use();
            LightShader.SetInt("shadowMap", 31);
            LightShader.SetInt("shadowCubeMap", 30);
            DebugShader.SetInt("shadowMap", 31);
        }
        public void RenderBuffer(Camera camera)
        {
            // Write To buffer

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, _fbo);
            GL.Disable(EnableCap.CullFace);
            DirectShadows?.RenderBuffer(camera,_wr);
            PointShadows?.RenderBuffer(camera, _wr.Lights.OfType<PointLight>().ToList(), _wr);
            GL.Enable(EnableCap.CullFace);

            // Read Buffer and render scene as normal
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.Viewport(0, 0, Game.WIDTH, Game.HEIGHT);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            foreach (var item in _wr.Lights.OfType<DirectLight>())
            {
                DirectShadows?.ConfigureShaderAndMatrices(LightShader, camera, item);
            }
            DirectShadows?.BindTexture();
            PointShadows?.BindTexture();
            LightShader.SetFloat("far_plane", FarPlane);
            _wr.Render(camera, LightShader);

/*            DebugShader.Use();
            DebugShader.SetFloat("near_plane", DirectShadows.NearPlane);
            DebugShader.SetFloat("far_plane", DirectShadows.FarPlane);
            DebugShader.SetInt("shadowMap", 31);
            DirectShadows.BindTexture();
            _debug.Draw(PrimitiveType.TriangleStrip);*/


        }
    }
}
