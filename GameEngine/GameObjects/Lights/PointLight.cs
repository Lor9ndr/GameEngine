using GameEngine.Bases.Components;
using GameEngine.Extensions;
using GameEngine.RenderPrepearings.FrameBuffers.Base;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Collections.Generic;

namespace GameEngine.GameObjects.Lights
{
    public class PointLight : Light
    {
        private static int _id = -1;
        public int PointLightID;
        public  Matrix4[] LightSpaceMatrices = new Matrix4[6];

        private Vector3[] _directions =
        {
            new Vector3(1.0f,  0.0f,  0.0f ),
            new Vector3(-1.0f,  0.0f,  0.0f ),
            new Vector3(0.0f,  1.0f,  0.0f ),
            new Vector3(0.0f, -1.0f,  0.0f ),
            new Vector3(0.0f,  0.0f,  1.0f ),
            new Vector3(0.0f,  0.0f, -1.0f )
        };
        private Vector3[] _ups =
        {
            new Vector3(0.0f, -1.0f,  0.0f ),
            new Vector3(0.0f, -1.0f,  0.0f ),
            new Vector3(0.0f,  0.0f,  1.0f ),
            new Vector3(0.0f,  0.0f, -1.0f ),
            new Vector3(0.0f, -1.0f,  0.0f ),
            new Vector3(0.0f, -1.0f,  0.0f )
        };

        public PointLight(Mesh mesh, LightData lightData, ShadowData shadowData, Transform transform)
            : base(mesh, lightData, shadowData, transform)
        {
            _id++;
            PointLightID = _id;
            ShadowData.Shadow = new FrameBuffer(Game.ShadowSize, ClearBufferMask.DepthBufferBit);
            ShadowData.Shadow.Setup();
            ShadowData.Shadow.Bind();
            ShadowData.Shadow.DisableColorBuffer();
            ShadowData.Shadow.AttachCubeMap(FramebufferAttachment.DepthAttachment, PixelInternalFormat.DepthComponent, PixelType.Float);
            UpdateMatrices();

        }

        public static new float NearPlane => 1.0f;
        public static new float FarPlane => 100.0f;

        public override void Render(Shader shader,int textureIdx, bool drawMesh = false)
        {
            shader.Use();
            SetupModel(shader);
            string name = $"pointLights[{PointLightID}].";
            shader.SetVector3(name + "position", Transform.Position);
            shader.SetVector3(name + "ambient", LightData.Ambient);
            shader.SetVector3(name + "diffuse", LightData.Diffuse);
            shader.SetVector3(name + "specular", LightData.Specular);
            shader.SetVector3(name + "lightColor", LightData.Color);
            shader.SetFloat(name + "constant", 1.0f);
            shader.SetFloat(name + "linear", 0.0014f);
            shader.SetFloat(name + "quadratic", 0.000007f);
            shader.SetVector3("lightColor", LightData.Color);
            shader.SetVector3("lightPos", Transform.Position);
            shader.SetFloat(name + "farPlane", FarPlane);
            GL.ActiveTexture(TextureUnit.Texture0 + textureIdx);
            ShadowData.Shadow.CubeMap.Bind();
            shader.SetInt(name + "shadow", textureIdx);
            shader.SetFloat("far_plane", FarPlane);
            UpdateMatrices();

            if (drawMesh)
            {
                DrawMesh(shader);
            }
        }

        public void RenderShadowData(Shader shader, int textureIdx)
        {
            ShadowData.Render(shader, this, textureIdx);
        }
        public override Matrix4 GetProjection => Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(90), Game.ShadowSize.X/Game.ShadowSize.Y, NearPlane, FarPlane);
        public override void UpdateMatrices()
        {
            for (int i = 0; i < LightSpaceMatrices.Length; i++)
            {
                LightSpaceMatrices[i] =  Matrix4.LookAt(Transform.Position, Transform.Position + _directions[i], _ups[i]).Multiply(GetProjection);
            }
        }

    }
}
