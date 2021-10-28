using Engine.Components;
using Engine.Rendering.Enums;
using Engine.Rendering.GameObjects;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            UpdateMatrices();

        }

        public override void Render(Shader shader,RenderFlags flags)
        {
            string name = "dirLight.";
            Transform.SetValuesShader(shader, name);
            LightData.Render(shader, name);

            shader.SetVector3(name + "lightColor", LightData.Color);
            shader.SetMatrix4(name + "lightSpaceMatrix", LightSpaceMatrix);
            shader.SetFloat(name + "farPlane", FarPlane);
            shader.SetFloat("far_plane", FarPlane);

            ProcessShadow(shader);
            base.Render(shader, flags);
        }
        public override Matrix4 GetProjection => Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(179.0f),1.0f, NearPlane, FarPlane);
        public override void UpdateMatrices()
        {
            Matrix4 view = Matrix4.LookAt(Transform.Position, Transform.Direction, Vector3.UnitY);
            LightSpaceMatrix =  view * GetProjection;
        }
    }
}
