using Engine.Components;
using Engine.Rendering.Enums;
using Engine.Rendering.GameObjects;
using OpenTK.Mathematics;
using System;

namespace Engine.GameObjects.Lights
{
    public class SpotLight : Light
    {

        public float CutOff { get => MathF.Cos(MathHelper.DegreesToRadians(_cutOff)); set => _cutOff = value; }
        public float OuterCutOff { get => MathF.Cos(MathHelper.DegreesToRadians(outerCutOff)); set => outerCutOff = value; }

        public int ID;

        private static int _spotLightId = 0;
        private float _constant;
        private float _linear;
        private float _quadratic;
        private float _cutOff;
        private float outerCutOff;

        public SpotLight(LightData lightData,
                         Transform transform,
                         float constant = 1.0f,
                         float linear = 0.08f,
                         float quadratic = 0.32f,
                         float cutOff = 12.5f,
                         float outerCutOff = 20.0f,
                         Model model = null) 
            : base(model, lightData, transform)
        {
            _constant = constant;
            _linear = linear;
            _quadratic = quadratic;
            CutOff = cutOff;
            OuterCutOff = outerCutOff;
            ShadowData = new ShadowData(ShadowData.GetDefaultFrameBuffer());
            ShadowData.Shadow.Setup();
            ShadowData.Shadow.Bind();
            ShadowData.Shadow.DisableColorBuffer();
            ShadowData.AttachTexture2DMap();
            FarPlane = 100.0f;
            NearPlane = 2.0f;
            UpdateMatrices();
            ID = _spotLightId;
            _spotLightId++;
        }


        public override void Render(Shader shader, RenderFlags flags)
        {
            string name = $"spotLights[{ID}].";

            Transform.SetValuesShader(shader, name);
            LightData.Render(shader, name);

            shader.SetFloat(name + "constant", _constant);
            shader.SetFloat(name + "linear", _linear);
            shader.SetFloat(name + "quadratic", _quadratic);
            shader.SetFloat(name + "cutOff", CutOff);
            shader.SetFloat(name + "outerCutOff", OuterCutOff);
            shader.SetFloat(name + "farPlane", FarPlane);
            shader.SetMatrix4(name + "lightSpaceMatrix", LightSpaceMatrix);
            ProcessShadow(shader);
            base.Render(shader, flags);
        }

        public override Matrix4 GetProjection => Matrix4.CreatePerspectiveFieldOfView(CutOff * 2.0f, 1.0f, NearPlane, FarPlane);
        public override void UpdateMatrices()
        {
            Matrix4 view = Matrix4.LookAt(Transform.Position, Transform.Position + Transform.Direction * FarPlane, Vector3.UnitY);
            LightSpaceMatrix = view * GetProjection;
        }
    }
}
