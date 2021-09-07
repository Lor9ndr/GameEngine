using GameEngine.Bases;
using GameEngine.Bases.Components;
using GameEngine.RenderPrepearings;
using GameEngine.RenderPrepearings.FrameBuffers.Base;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine.GameObjects.Lights
{
    public class SpotLight : Light
    {
        private float _cutOff;

        private float outerCutOff;
        public float CutOff { get => MathF.Cos(MathHelper.DegreesToRadians(_cutOff)); set => _cutOff = value; }
        public float OuterCutOff { get => MathF.Cos(MathHelper.DegreesToRadians(outerCutOff)); set => outerCutOff = value; }

        public static new float NearPlane = 2.0f;

        public static new float FarPlane = 100f;

        private static int _spotLightId = 0;
        public int ID;
        private float _constant;
        private float _linear;
        private float _quadratic;

        public SpotLight(LightData lightData,
                         Transform transform,
                         float constant = 1.0f,
                         float linear = 0.08f,
                         float quadratic = 0.32f,
                         float cutOff = 12.5f,
                         float outerCutOff = 20.0f,
                         Mesh mesh = null) 
            : base(mesh, lightData, transform)
        {
            _constant = constant;
            _linear = linear;
            _quadratic = quadratic;
            CutOff = cutOff;
            OuterCutOff = outerCutOff;
            ShadowData.Shadow = new FrameBuffer(Game.ShadowSize, ClearBufferMask.DepthBufferBit);
            ShadowData.Shadow.Setup();
            ShadowData.Shadow.Bind();
            ShadowData.Shadow.DisableColorBuffer();
            ShadowData.Shadow.AttachTexture2DMap(FramebufferAttachment.DepthAttachment, PixelInternalFormat.DepthComponent, PixelType.Float);
            UpdateMatrices();
            ID = _spotLightId;
            _spotLightId++;
        }


        public override void Render(Shader shader, RenderFlags flags, int textureIdx)
        {
            string name = $"spotLights[{ID}].";
            shader.SetVector3(name + "position", Transform.Position);
            shader.SetVector3(name + "direction", Transform.Direction);
            shader.SetVector3(name + "ambient", LightData.Ambient);
            shader.SetVector3(name + "diffuse", LightData.Diffuse);
            shader.SetVector3(name + "specular", LightData.Specular);
            shader.SetVector3(name + "lightColor", LightData.Color);
            shader.SetFloat(name + "constant", _constant);
            shader.SetFloat(name + "linear", _linear);
            shader.SetFloat(name + "quadratic", _quadratic);
            shader.SetFloat(name + "cutOff", CutOff);
            shader.SetFloat(name + "outerCutOff", OuterCutOff);
            shader.SetFloat(name + "farPlane", FarPlane);
            shader.SetMatrix4(name + "lightSpaceMatrix", LightSpaceMatrix);
            GL.ActiveTexture(TextureUnit.Texture0 + textureIdx);
            ShadowData.Shadow.Texture.Bind(TextureTarget.Texture2D);
            shader.SetInt(name + "shadow", textureIdx);
            SetupModel(shader);
            UpdateMatrices();
           
        }
        public override Matrix4 GetProjection => Matrix4.CreatePerspectiveFieldOfView(CutOff * 2.0f, 1.0f, NearPlane,FarPlane);
        public override void UpdateMatrices()
        {
            Matrix4 view = Matrix4.LookAt(Transform.Position, Transform.Position + Transform.Direction, Vector3.UnitY);
            LightSpaceMatrix =  view * GetProjection;
        }
    }
}
