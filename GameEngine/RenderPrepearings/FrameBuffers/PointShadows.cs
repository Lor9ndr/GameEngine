using GameEngine.DefaultMeshes;
using GameEngine.Extensions;
using GameEngine.GameObjects;
using GameEngine.GameObjects.Lights;
using GameEngine.RenderPrepearings.FrameBuffers.Base;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine.RenderPrepearings.FrameBuffers
{
    public class PointShadows: DepthTextureBuffer
    {
        public static Matrix4 ShadowProjection 
            => Matrix4.CreatePerspectiveFieldOfView(
                MathHelper.DegreesToRadians(90),
                ShadowFrameBuffer.ShadowSize.X / ShadowFrameBuffer.ShadowSize.Y,
                NearPlane, 
                FarPlane);
        public static float NearPlane = 1.0f;
        public static float FarPlane = 100.0f;
        #region Constructors
        public PointShadows(FrameBuffer fbo, Shader depth, TextureUnit shadowTextureUnit) 
            : base(depth,fbo, shadowTextureUnit)
        {
        }
        #endregion

        public override void Setup()
        {
            Texture = GL.GenTexture();
            GL.BindTexture(TextureTarget.TextureCubeMap, Texture);
            for (int i = 0; i < 6; i++)
            {
                GL.TexImage2D(
                    TextureTarget.TextureCubeMapPositiveX + i,
                    0,
                    PixelInternalFormat.DepthComponent,
                    ShadowFrameBuffer.ShadowSize.X,
                    ShadowFrameBuffer.ShadowSize.Y,
                    0, PixelFormat.DepthComponent,
                    PixelType.Float, 
                    (IntPtr)null
                    );
            }

            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR, (int)TextureWrapMode.ClampToEdge);
            FBO.Bind();
            GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, Texture, 0);
            GL.DrawBuffer(DrawBufferMode.None);
            GL.ReadBuffer(ReadBufferMode.None);
            FBO.Unbind();
        }
        public override void Render(Camera camera, WorldRenderer wr)
        {

            FBO.Bind();
            GL.Viewport(0, 0, ShadowFrameBuffer.ShadowSize.X, ShadowFrameBuffer.ShadowSize.Y);
            GL.Clear(ClearBufferMask.DepthBufferBit);

            DepthShader.Use();
            var lights = wr.Lights.OfType<PointLight>().ToList();
            for (int i = 0; i < lights.Count; i++)
            {
                var shadowTransofrms = GetShadowTransform(lights[i]);
                for (int j = 0; j < shadowTransofrms.Count; j++)
                {
                    DepthShader.SetMatrix4($"shadowMatrices[{j}]", shadowTransofrms[j]);
                }
                DepthShader.SetVector3("lightPos", lights[i].Position);
            }
            DepthShader.SetFloat("far_plane", FarPlane);
            wr.Render(camera, DepthShader);
            FBO.Unbind();


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
    }
}
