using OpenTK.Mathematics;

namespace GameEngine.Bases
{
    public class LightData:IComponent
    {
        private Vector3 _color;
        private Vector3 _ambient;
        private Vector3 _diffuse;
        private Vector3 _specular;

        public Vector3 Specular { get => _specular; set => _specular = value; }
        public Vector3 Diffuse { get => _diffuse; set => _diffuse = value; }
        public Vector3 Ambient { get => _ambient; set => _ambient = value; }
        public Vector3 Color { get => _color; set => _color = value; }

        public LightData(Vector3 color, Vector3 ambient, Vector3 diffuse, Vector3 specular)
        {
            Color = color;
            Ambient = ambient;
            Diffuse = diffuse;
            Specular = specular;
        }

        public void PlusAmbient(Vector3 ambient) => Ambient += ambient;
        public void PlusSpecular(Vector3 specular) => Specular += specular;
        public void PlusDiffuse(Vector3 diffuse) => Diffuse += diffuse;
        public void PlusColor(Vector3 color) => Color += color;

    }
}
