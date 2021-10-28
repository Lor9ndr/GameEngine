using Engine.Components;
using Engine.Rendering.Enums;
using Engine.Rendering.GameObjects;
using OpenTK.Mathematics;

namespace Engine.GameObjects.Lights
{
    public class PointLight : Light
    {
        private static int _pointLightId = -1;
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

        public PointLight(Model model, LightData lightData, Transform transform)
            : base(model, lightData,  transform)
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
            UpdateMatrices();

        }


        public override void Render(Shader shader, RenderFlags flags)
        {
            shader.Use();
            string name = $"pointLights[{PointLightID}].";
            LightData.Render(shader, name);
            shader.SetFloat(name + "constant", 1.0f);
            shader.SetFloat(name + "linear", 0.0014f);
            shader.SetFloat(name + "quadratic", 0.07f);
            shader.SetFloat(name + "farPlane", FarPlane);
            Transform.SetValuesShader(shader, name);
            ProcessShadow(shader);
            base.Render(shader,flags);
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
