using GameEngine.Bases;
using GameEngine.Bases.Components;
using GameEngine.RenderPrepearings;
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

        public static new float NearPlane => 0.1f;

        public static new float FarPlane => 100.0f;

        public SpotLight(LightData lightData, Transform transform , float cutOff = 0, float outerCutOff = 0, Mesh mesh= null) 
            : base(mesh, lightData, transform)
        {
            CutOff = cutOff;
            OuterCutOff = outerCutOff;
        }


        public override void Render(Shader shader, int textureidx, bool drawMesh = false)
        {
            SetupModel(shader);
            shader.SetVector3("spotLight.position", Transform.Position);
            shader.SetVector3("spotLight.direction", Transform.Direction);
            shader.SetVector3("spotLight.ambient", LightData.Ambient);
            shader.SetVector3("spotLight.diffuse", LightData.Diffuse);
            shader.SetVector3("spotlight.specular", LightData.Specular);
            shader.SetVector3("spotLight.lightColor", LightData.Color);
            shader.SetFloat("spotLight.constant", 1.0f);
            shader.SetFloat("spotLight.linear", 0.09f);
            shader.SetFloat("spotLight.quadratic", 0.032f);
            shader.SetFloat("spotLight.cutOff", CutOff);
            shader.SetFloat("spotLight.outerCutOff", CutOff);
           
        }
    }
}
