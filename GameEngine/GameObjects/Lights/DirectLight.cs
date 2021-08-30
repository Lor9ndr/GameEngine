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
    public class DirectLight : Light
    {

        public DirectLight(Mesh mesh, LightData lightData, ShadowData shadowData, Transform transform) : base(mesh, lightData,shadowData, transform)
        {
        }
        public static new float NearPlane => -40f;

        public static new float FarPlane => 1000.0f;

        public override void Render(Shader shader, int textureidx, bool drawMesh)
        {
            shader.SetVector3("dirLight.position", Transform.Position);
            shader.SetVector3("dirLight.direction", Transform.Direction);
            shader.SetVector3("dirLight.ambient", LightData.Ambient);
            shader.SetVector3("dirLight.diffuse", LightData.Diffuse);
            shader.SetVector3("dirLight.specular", LightData.Specular);
            shader.SetVector3("dirLight.lightColor", LightData.Color);
            SetupModel(shader);
            if (drawMesh)
            {
                DrawMesh(shader);
            }
        }
    }
}
