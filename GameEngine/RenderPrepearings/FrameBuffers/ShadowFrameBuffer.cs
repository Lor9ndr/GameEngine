using GameEngine.DefaultMeshes;
using GameEngine.GameObjects;
using GameEngine.GameObjects.Lights;
using GameEngine.RenderPrepearings.FrameBuffers.Base;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Linq;

namespace GameEngine.RenderPrepearings.FrameBuffers
{
    public class ShadowFrameBuffer : FrameBuffer
    {
        public DirectShadows DirectShadows;
        public PointShadows PointShadows;

        private Mesh _debug => FullScreenQuad.GetQuad();
        private TextureUnit PointTextureUnit = TextureUnit.Texture30;
        private TextureUnit DirectTextureUnit = TextureUnit.Texture31;
        public static Vector2i ShadowSize = new Vector2i(512, 512);
        public Shader DepthCubeShader = new Shader(Game.SHADOW_SHADERS_PATH + "DepthCubeMap.vs", Game.SHADOW_SHADERS_PATH + "DepthCubeMap.fr", Game.SHADOW_SHADERS_PATH + "DepthCubeMap.gs");
        public Shader DepthDirectShader = new Shader(Game.SHADOW_SHADERS_PATH + "Depth.vs", Game.SHADOW_SHADERS_PATH + "Depth.fr");
        public Shader LightShader = new Shader(Game.SHADOW_SHADERS_PATH + "SimpleShader.vs", Game.SHADOW_SHADERS_PATH + "SimpleShader.fr");
        public Shader DebugShader = new Shader(Game.SHADOW_SHADERS_PATH + "Debug.vs", Game.SHADOW_SHADERS_PATH + "Debug.fr");

        public ShadowFrameBuffer(WorldRenderer wr) : base(wr)
        {
        }

        public override void Setup()
        {
            FBO = GL.GenFramebuffer();
            DirectShadows = new DirectShadows(this, DepthDirectShader, DirectTextureUnit);
            PointShadows = new PointShadows(this, DepthCubeShader, PointTextureUnit);
            DirectShadows.Setup();
            PointShadows.Setup();
            LightShader.Use();
            LightShader.SetInt("shadowCubeMap", 30);
            LightShader.SetInt("shadowMap", 31);
            DebugShader.SetInt("shadowMap", 31);
        }
        public void RenderBuffer(Camera camera)
        {
            // Write To buffer
            Bind();
            DirectShadows?.Render(camera, WR, WR.Lights.OfType<DirectLight>().First());
            foreach (var item in WR.Lights.OfType<PointLight>())
            {
                PointShadows?.Render(camera, WR, item);
            }
            Unbind();


            // Read Buffer and render scene as normal
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);
            GL.FrontFace(FrontFaceDirection.Ccw);
            GL.Viewport(0, 0, Game.Width, Game.Height);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            DirectShadows?.ConfigureShaderAndMatrices(LightShader, camera, WR.Lights.First(s => s.GetType() == typeof(DirectLight)));
            DirectShadows?.BindTexture();
            PointShadows?.BindTexture(TextureTarget.TextureCubeMap);
            LightShader.SetFloat("far_plane", PointShadows.FarPlane);
            WR.Render(camera, LightShader, true, false);


        }
    }
}
