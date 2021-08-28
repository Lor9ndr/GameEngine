using GameEngine.Bases;
using GameEngine.RenderPrepearings;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine.GameObjects.Lights
{
    public class PointLight : Light
    {
        private static int _id = -1;
        public int PointLightID;


        public PointLight(Mesh mesh, LightData lightData, ShadowData shadowData, Transform transform)
            : base(mesh, lightData, shadowData, transform)
        {
            _id++;
            PointLightID = _id;
        }
        public override void Render(Shader shader, bool drawMesh = false)
        {
            shader.Use();
            SetupModel(shader);
            shader.SetVector3($"pointLights[{PointLightID}].position", Transform.Position);
            shader.SetVector3($"pointLights[{PointLightID}].ambient", LightData.Ambient);
            shader.SetVector3($"pointLights[{PointLightID}].diffuse", LightData.Diffuse);
            shader.SetVector3($"pointLights[{PointLightID}].specular", LightData.Specular);
            shader.SetVector3($"pointLights[{PointLightID}].lightColor", LightData.Color);
            shader.SetFloat($"pointLights[{PointLightID}].constant", 1.0f);
            shader.SetFloat($"pointLights[{PointLightID}].linear", 0.5f);
            shader.SetFloat($"pointLights[{PointLightID}].quadratic", 0.064f);
            shader.SetVector3("lightColor", LightData.Color);
            if (drawMesh)
            {
                DrawMesh(shader);
            }
        }
      
    }
}
