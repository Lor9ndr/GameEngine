using Engine.Components;
using Engine.Rendering.Enums;
using OpenTK.Mathematics;
using System;
using System.Threading.Tasks;

namespace Engine.GameObjects.Lights
{
    public class SpotLight : Light
    {

        public float CutOff { get => MathF.Cos(MathHelper.DegreesToRadians(_cutOff)); set => _cutOff = value; }
        public float OuterCutOff { get => MathF.Cos(MathHelper.DegreesToRadians(outerCutOff)); set => outerCutOff = value; }

        public int SpotLightId;

        private static int _spotLightId;
        private readonly float _constant;
        private readonly float _linear;
        private readonly float _quadratic;
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
            NearPlane = 4.0f;
            SpotLightId = _spotLightId;
            _spotLightId++;
        }


        public override void Render(Shader shader, RenderFlags flags)
        {
            string name = $"spotLights[{SpotLightId}].";
            Game.EngineGL.UseShader(shader)
                .SetShaderData("lightColor", LightData.Color);
            if (flags.HasFlag(RenderFlags.LightData))
            {
                Transform.SetLightValuesShader(shader, name);
                LightData.Render(shader, name, flags);
                Game.EngineGL.SetShaderData(name + "constant", _constant)
                .SetShaderData(name + "linear", _linear)
                .SetShaderData(name + "quadratic", _quadratic)
                .SetShaderData(name + "cutOff", CutOff)
                .SetShaderData(name + "outerCutOff", OuterCutOff)
                .SetShaderData(name + "farPlane", FarPlane)
                .SetShaderData(name + "lightSpaceMatrix", LightSpaceMatrix);
            }

            if (flags.HasFlag(RenderFlags.ProcessShadow) && Game.Shadows)
            {
                ShadowData.Render(this);
            }
            base.Render(shader, flags);

        }

        public override Matrix4 GetProjection => Matrix4.CreatePerspectiveFieldOfView(CutOff,1, NearPlane, FarPlane);
        public override async Task UpdateMatricesAsync()
        {
            Matrix4 view = Matrix4.LookAt(Transform.Position, Transform.Position + Transform.Direction, Vector3.UnitY);
            LightSpaceMatrix = view * GetProjection;
            await Task.CompletedTask;
        }
    }
}
