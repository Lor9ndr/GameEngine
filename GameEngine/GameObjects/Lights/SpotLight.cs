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
        public SpotLight( Vector3 position, Vector3 ambient, Vector3 diffuse,Vector3 specular, Vector3 lightColor, 
            Vector3 direction = default, Vector3 rotation = default, Vector3 scale = default, Mesh mesh = null, float velocity = 0, Matrix4 model = default) 
            : base(mesh, position, ambient, diffuse, specular, lightColor, direction, rotation, scale, velocity, model)
        {

        }
        public override void Render(Shader shader)
        {
            SetupModel(shader);
            shader.SetVector3("spotLight.position", Position);
            shader.SetVector3("spotLight.direction", Direction);
            shader.SetVector3("spotLight.ambient", Ambient);
            shader.SetVector3("spotLight.diffuse", Diffuse);
            shader.SetVector3("spotlight.specular", Specular);
            shader.SetVector3("spotLight.lightColor", LightColor);
            shader.SetFloat("spotLight.constant", 1.0f);
            shader.SetFloat("spotLight.linear", 0.09f);
            shader.SetFloat("spotLight.quadratic", 0.032f);
            shader.SetFloat("spotLight.cutOff", MathF.Cos(MathHelper.DegreesToRadians(12.5f)));
            shader.SetFloat("spotLight.outerCutOff", MathF.Cos(MathHelper.DegreesToRadians(15.0f)));
        }
    }
}
