using Engine.Components;
using Engine.Rendering.Enums;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Threading.Tasks;

namespace Engine.GameObjects.Lights
{
    public class DirectLight : Light
    {
        public DirectLight(Model model, LightData lightData, Transform transform) : base(model, lightData, transform)
        {
            ShadowData = new ShadowData(ShadowData.GetDefaultFrameBuffer());
            ShadowData.Shadow.Setup();
            ShadowData.Shadow.Bind();
            ShadowData.Shadow.DisableColorBuffer();
            ShadowData.Shadow.AttachTexture2DMap(FramebufferAttachment.DepthAttachment, PixelInternalFormat.DepthComponent, PixelType.Float);
            FarPlane = 100.0f;
            NearPlane = 1.0f;
        }

        public override void Render(Shader shader, RenderFlags flags)
        {
            string name = "dirLight.";
            Game.EngineGL.UseShader(shader).SetShaderData("lightColor", LightData.Color);

            if (flags.HasFlag(RenderFlags.LightData))
            {
                Transform.SetLightValuesShader(shader, name);
                LightData.Render(shader, name, flags);
                Game.EngineGL.SetShaderData(name + "lightColor", LightData.Color)
                    .SetShaderData(name + "lightSpaceMatrix", LightSpaceMatrix)
                    .SetShaderData(name + "farPlane", FarPlane)
                    .SetShaderData("far_plane", FarPlane);
            }
            if (flags.HasFlag(RenderFlags.ProcessShadow) && Game.Shadows)
            {
                ShadowData.Render(this);
            }
            base.Render(shader, flags);
        }
        public override Matrix4 GetProjection => Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(179.0f), 1.0f, NearPlane, FarPlane);
        public override async Task UpdateMatricesAsync()
        {
            await Task.Run(() =>
            {
                Matrix4 view = Matrix4.LookAt(Transform.Position, Transform.Position + Transform.Direction, Vector3.UnitY);
                LightSpaceMatrix = view * GetProjection;
            });
        }
    }
}
