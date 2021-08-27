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

        public float CutOff { get => MathF.Cos(MathHelper.DegreesToRadians(_cutOff)); set => _cutOff = value; }
        private float outerCutOff;

        public float OuterCutOff { get => MathF.Cos(MathHelper.DegreesToRadians(outerCutOff)); set => outerCutOff = value; }
        public SpotLight( Vector3 position, Vector3 ambient, Vector3 diffuse,Vector3 specular, Vector3 lightColor,float cutOff,float outerCutOff,
            Vector3 direction = default, Vector3 rotation = default, Vector3 scale = default, Mesh mesh = null, float velocity = 0, Matrix4 model = default) 
            : base(mesh, position, ambient, diffuse, specular, lightColor, direction, rotation, scale, velocity, model)
        {
            CutOff = cutOff;
            OuterCutOff = outerCutOff;
        }
        public override void Render(Shader shader, bool drawMesh = false)
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
            shader.SetFloat("spotLight.cutOff", CutOff);
            shader.SetFloat("spotLight.outerCutOff", CutOff);
           
        }
    }
}
