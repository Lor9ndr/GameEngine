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
        public PointLight(Mesh mesh, Vector3 position, Vector3 ambient, Vector3 diffuse, Vector3 specular, Vector3 lightColor, 
            Vector3 direction = default, Vector3 rotation = default, Vector3 scale = default, float velocity = 0, Matrix4 model = default)
            : base(mesh, position, ambient, diffuse, specular,  lightColor, direction, rotation, scale, velocity, model)
        {
            _id++;
            PointLightID = _id;
        }
        public override void Render(Shader shader, bool drawMesh = false)
        {
            shader.Use();
            shader.SetVector3($"pointLights[{PointLightID}].position", Position);
            shader.SetVector3($"pointLights[{PointLightID}].ambient", Ambient);
            shader.SetVector3($"pointLights[{PointLightID}].diffuse", Diffuse);
            shader.SetVector3($"pointLights[{PointLightID}].specular", Specular);
            shader.SetVector3($"pointLights[{PointLightID}].lightColor", LightColor);
            shader.SetFloat($"pointLights[{PointLightID}].constant", 1.0f);
            shader.SetFloat($"pointLights[{PointLightID}].linear", 0.5f);
            shader.SetFloat($"pointLights[{PointLightID}].quadratic", 0.064f);
            shader.SetVector3("lightColor", LightColor);
            SetupModel(shader);
            if (drawMesh)
            {
                DrawMesh(shader);
            }
        }
      
    }
}
