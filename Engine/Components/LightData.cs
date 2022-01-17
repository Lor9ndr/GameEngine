using Engine.Rendering.Enums;
using Engine.Rendering.GameObjects;
using OpenTK.Mathematics;

namespace Engine.Components
{
    public class LightData : IComponent
    {
        private Vector3 _color;
        private Vector3 _ambient;
        private Vector3 _diffuse;
        private Vector3 _specular;
        private float _intensity;

        public Vector3 Specular { get => _specular; set => _specular = value; }
        public Vector3 Diffuse { get => _diffuse; set => _diffuse = value; }
        public Vector3 Ambient { get => _ambient; set => _ambient = value; }
        public Vector3 Color { get => _color; set => _color = value; }
        public float Intensity { get => _intensity; set => _intensity = value; }
        public GameObject AttachedObject { get; set; }

        public LightData(Vector3 color, Vector3 ambient, Vector3 diffuse, Vector3 specular, float intensity)
        {
            _color = color;
            _ambient = ambient;
            _diffuse = diffuse;
            _specular = specular;
            _intensity = intensity;
        }

        public void Render(Shader shader, string name, RenderFlags flags)
        {
            if (flags.HasFlag(RenderFlags.LightData))
            {
                shader.SetVector3(name + "ambient", Ambient);
                shader.SetVector3(name + "diffuse", Diffuse);
                shader.SetVector3(name + "specular", Specular);
                shader.SetVector3(name + "lightColor", Color);
                shader.SetVector3("lightColor", Color);
                shader.SetFloat(name + "intensity", Intensity);
            }

        }

        public void PlusAmbient(Vector3 ambient) => Ambient += ambient;
        public void PlusSpecular(Vector3 specular) => Specular += specular;
        public void PlusDiffuse(Vector3 diffuse) => Diffuse += diffuse;
        public void PlusColor(Vector3 color) => Color += color;

        public void AttachGameObject(GameObject gameObject)
        {
            AttachedObject = gameObject;
        }
    }
}
