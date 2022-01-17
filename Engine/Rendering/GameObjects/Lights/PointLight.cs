using Engine.Components;
using Engine.Rendering.Enums;
using OpenTK.Mathematics;
using System.Threading.Tasks;

namespace Engine.GameObjects.Lights
{
    public class PointLight : Light
    {
        private static int _pointLightId = -1;
        public int PointLightID;
        public Matrix4[] LightSpaceMatrices = new Matrix4[6];

        private readonly Vector3[] _directions =
        {
            new Vector3(1.0f,  0.0f,  0.0f ),
            new Vector3(-1.0f, 0.0f,  0.0f ),
            new Vector3(0.0f,  1.0f,  0.0f ),
            new Vector3(0.0f, -1.0f,  0.0f ),
            new Vector3(0.0f,  0.0f,  1.0f ),
            new Vector3(0.0f,  0.0f, -1.0f )
        };
        private readonly Vector3[] _ups =
        {
            new Vector3(0.0f, -1.0f,  0.0f ),
            new Vector3(0.0f, -1.0f,  0.0f ),
            new Vector3(0.0f,  0.0f,  1.0f ),
            new Vector3(0.0f,  0.0f, -1.0f ),
            new Vector3(0.0f, -1.0f,  0.0f ),
            new Vector3(0.0f, -1.0f,  0.0f )
        };

        public PointLight(Model model, LightData lightData, Transform transform)
            : base(model, lightData, transform)
        {
            _pointLightId++;
            PointLightID = _pointLightId;
            ShadowData = new ShadowData(ShadowData.GetDefaultFrameBuffer());
            ShadowData.Shadow.Setup();
            ShadowData.Shadow.Bind();
            ShadowData.Shadow.DisableColorBuffer();
            ShadowData.AttachCubeMap();
            FarPlane = 100.0f;
            NearPlane = 1.0f;
        }

        public override void Render(Shader shader, RenderFlags flags)
        {
            string name = $"pointLights[{PointLightID}].";
            Game.EngineGL.UseShader(shader)
                .SetShaderData("lightColor", LightData.Color);
            if (flags.HasFlag(RenderFlags.LightData))
            {
                Transform.SetLightValuesShader(shader, name);
                LightData.Render(shader, name, flags);
                Game.EngineGL.SetShaderData(name + "constant", 1.0f)
                    .SetShaderData(name + "linear", 0.0014f)
                    .SetShaderData(name + "quadratic", 0.07f)
                    .SetShaderData(name + "farPlane", FarPlane);
            }
            if (flags.HasFlag(RenderFlags.ProcessShadow) && Game.Shadows)
            {
                ShadowData.Render(this);
            }
            base.Render(shader, flags);
        }
        public override Matrix4 GetProjection => Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(90), 1.0f, NearPlane, FarPlane);
        public override async Task UpdateMatricesAsync()
        {
            for (int i = 0; i < LightSpaceMatrices.Length; i++)
            {
                await Task.Run(() =>
                {
                    LightSpaceMatrices[i] = Matrix4.LookAt(Transform.Position, Transform.Position + _directions[i], _ups[i]) * GetProjection;
                });
            }

        }
    }
}
