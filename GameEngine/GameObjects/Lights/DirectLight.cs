using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine.GameObjects.Lights
{
    public class DirectLight : Light
    {
        public DirectLight(Mesh mesh, Vector3 position, Vector3 ambient, Vector3 diffuse,Vector3 specular, Vector3 lightColor, 
            Vector3 direction = default, Vector3 rotation = default, Vector3 scale = default, float velocity = 0, Matrix4 model = default)
            : base(mesh, position, ambient, diffuse, lightColor, specular, direction, rotation, scale, velocity, model)
        {
            if (direction == default)
            {
                Direction = new Vector3(0);
            }
        }

        public override void Render(Shader shader, bool drawMesh)
        {
            shader.SetVector3("dirLight.position", Position);
            shader.SetVector3("dirLight.direction", Direction);
            shader.SetVector3("dirLight.ambient", Ambient);
            shader.SetVector3("dirLight.diffuse", Diffuse);
            shader.SetVector3("dirLight.specular", Specular);
            shader.SetVector3("dirLight.lightColor", LightColor);
            SetupModel(shader);
            if (drawMesh)
            {
                DrawMesh(shader);
            }
        }
    }
}
