using GameEngine.Bases;
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

        public PointLight(Mesh mesh, LightData lightData, Transform transform)
            : base(mesh, lightData,  transform)
        {
            _id++;
            PointLightID = _id;
            ShadowData.Shadow = new FrameBuffer(Game.ShadowSize, ClearBufferMask.DepthBufferBit);
            ShadowData.Shadow.Setup();
            ShadowData.Shadow.Bind();
            ShadowData.Shadow.DisableColorBuffer();
            ShadowData.Shadow.AttachCubeMap(FramebufferAttachment.DepthAttachment, PixelInternalFormat.DepthComponent, PixelType.Float);
            FarPlane = 100.0f;
            NearPlane = 1.0f;
            UpdateMatrices();

        }


        public override void Render(Shader shader, RenderFlags flags, int textureIdx)
        {
            shader.Use();
            SetupModel(shader);
            string name = $"pointLights[{PointLightID}].";

            Transform.Render(shader, name);
            LightData.Render(shader, name);
            shader.SetFloat(name + "constant", 1.0f);
            shader.SetFloat(name + "linear", 0.0014f);
            shader.SetFloat(name + "quadratic", 0.07f);

            shader.SetFloat(name + "farPlane", FarPlane);
            ShadowData.Shadow.CubeMap.Use(TextureUnit.Texture0 + textureIdx, TextureTarget.TextureCubeMap);
            shader.SetInt(name + "shadow", textureIdx);
            UpdateMatrices();
            if (flags.HasFlag(RenderFlags.Mesh))
            {
                DrawMesh(shader);
            }
        }
        public override Matrix4 GetProjection => Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(90), 1.0f, NearPlane, FarPlane);
        public override void UpdateMatrices()
        {
            for (int i = 0; i < LightSpaceMatrices.Length; i++)
            {
                LightSpaceMatrices[i] = Matrix4.LookAt(Transform.Position, Transform.Position + _directions[i], _ups[i]) * GetProjection;
            }
        }

    }
}
