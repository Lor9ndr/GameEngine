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
    public class PointShadows
    {
        private int _depthMapFBO;
        private int _depthCubeMap;
        private TextureUnit _shadowTextureUnit;

        private Shader _simpleShader;
        private Shader _depthShader; 
        public static Matrix4 ShadowProjection => Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(90), ShadowFrameBuffer.ShadowSize.X / ShadowFrameBuffer.ShadowSize.Y, NearPlane, FarPlane);
        public static float NearPlane = 1.0f;
        public static float FarPlane = 100.0f;
        #region Constructors
        public PointShadows(int fbo, Shader depth, TextureUnit shadowTextureUnit)
        {
            _depthMapFBO = fbo;
            _depthShader = depth;
            _shadowTextureUnit = shadowTextureUnit;
        }
        #endregion

        public void Setup()
        {
            _depthCubeMap = GL.GenTexture();
            GL.BindTexture(TextureTarget.TextureCubeMap, _depthCubeMap);
            for (int i = 0; i < 6; i++)
            {
                GL.TexImage2D(TextureTarget.TextureCubeMapPositiveX + i, 0, PixelInternalFormat.DepthComponent, ShadowFrameBuffer.ShadowSize.X, ShadowFrameBuffer.ShadowSize.Y, 0, PixelFormat.DepthComponent, PixelType.Float, (IntPtr)null);
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

        }
        public void RenderBuffer(Camera camera, List<PointLight>  lights, WorldRenderer wr)
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, _depthMapFBO);
            GL.Viewport(0, 0, ShadowFrameBuffer.ShadowSize.X, ShadowFrameBuffer.ShadowSize.Y);
            GL.Clear(ClearBufferMask.DepthBufferBit);

            _depthShader.Use();
            for (int i = 0; i < lights.Count; i++)
            {
                var shadowTransofrms = GetShadowTransform(lights[i]);
                for (int j = 0; j < shadowTransofrms.Count; j++)
                {
                    _depthShader.SetMatrix4($"shadowMatrices[{j}]", shadowTransofrms[j]);
                }
                _depthShader.SetVector3("lightPos", lights[i].Position);
            }
            _depthShader.SetFloat("far_plane", FarPlane);
            wr.Render(camera, _depthShader);


        }
        public static List<Matrix4> GetShadowTransform(PointLight Light) => new List<Matrix4>()
            {
                Matrix4.LookAt(Light.Position, Light.Position + new Vector3(1.0f, 0.0f, 0.0f),  new Vector3(0.0f, -1.0f, 0.0f)).Multiply(ShadowProjection),
                Matrix4.LookAt(Light.Position, Light.Position + new Vector3(-1.0f, 0.0f, 0.0f), new Vector3(0.0f, -1.0f, 0.0f)).Multiply(ShadowProjection),
                Matrix4.LookAt(Light.Position, Light.Position + new Vector3(0.0f, 1.0f, 0.0f),  new Vector3(0.0f, 0.0f, 1.0f)).Multiply(ShadowProjection),
                Matrix4.LookAt(Light.Position, Light.Position + new Vector3(0.0f, -1.0f, 0.0f), new Vector3(0.0f, 0.0f, -1.0f)).Multiply(ShadowProjection),
                Matrix4.LookAt(Light.Position, Light.Position + new Vector3(0.0f, 0.0f, 1.0f),  new Vector3(0.0f, -1.0f, 0.0f)).Multiply(ShadowProjection),
                Matrix4.LookAt(Light.Position, Light.Position + new Vector3(0.0f, 0.0f, -1.0f), new Vector3(0.0f, -1.0f, 0.0f)).Multiply(ShadowProjection)
            };
        public void BindTexture()
        {
            GL.ActiveTexture(_shadowTextureUnit);
            GL.BindTexture(TextureTarget.TextureCubeMap, _depthCubeMap);
        }

    }
}
