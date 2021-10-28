using Engine.Rendering.Enums;
using OpenTK.Mathematics;

namespace Engine.Components
{
    public class Transform : IComponent
    {
        private Vector3 _position;
        private Vector3 _direction;
        private Vector3 _rotation;
        private Vector3 _scale;
        private Vector3 _velocity;
        private Matrix4 _model;
        private float _mass;

        public Vector3 Position { get => _position; set => _position = value; }
        public Vector3 Direction { get => _direction; set => _direction = value; }
        public Vector3 Rotation { get => _rotation; set => _rotation = value; }
        public Vector3 Scale { get => _scale; set => _scale = value; }
        public Vector3 Velocity { get => _velocity; set => _velocity = value; }
        public float Mass { get => _mass; set => _mass = value; }
        public Matrix4 Model { get => _model; set => _model = value; }

        public Transform(Vector3 position, Vector3 direction, Vector3 rotation, Vector3 scale, Vector3 velocity, float mass = default, Matrix4 model = default)
            : this(position, direction)
        {
            Rotation = rotation;
            Scale = scale;
            Velocity = velocity;
            Model = model;
        }
        public Transform(Vector3 position, Vector3 direction)
            : this(position)
        {
            Direction = direction;
        }
        public Transform(Vector3 position)
        {
            Position = position;
            Direction = new Vector3(0);
            Scale = new Vector3(1);
            Rotation = new Vector3(0);
            Model = Matrix4.Zero;
        }
        public void Render(Shader shader, RenderFlags flags)
        {
           
            shader.Use();
            var t2 = Matrix4.CreateTranslation(Position);
            var r1 = Matrix4.CreateRotationX(Rotation.X);
            var r2 = Matrix4.CreateRotationY(Rotation.Y);
            var r3 = Matrix4.CreateRotationZ(Rotation.Z);
            var s = Matrix4.CreateScale(Scale);
            Model = r1 * r2 * r3 * s * t2;
            shader.SetMatrix4("model", Model);
            if (flags.HasFlag(RenderFlags.ReverseNormals))
            {
                shader.SetInt("reverse_normals", 1);
            }
          
        }
        public void SetValuesShader(Shader shader, string name)
        {
            shader.SetVector3(name + "position", Position);
            shader.SetVector3(name + "direction", Direction);

        }
        public virtual void CreateRotation(Vector3 rotation)
        {
            Rotation += rotation;
        }

        public virtual void CreateScale(Vector3 scale)
        {
            Scale += scale;
        }

        public virtual void CreateTranslation(Vector3 transform)
        {
            Position += transform;
        }
        public virtual void Update()
        {
            Position += Direction * (Velocity * (float)Game.DeltaTime);
        }
    }
}
