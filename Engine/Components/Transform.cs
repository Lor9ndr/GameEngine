using Engine.Physics;
using Engine.Rendering.Enums;
using OpenTK.Mathematics;
using System.Threading.Tasks;

namespace Engine.Components
{
    public struct Transform : IComponent
    {
        private Vector3 _position;
        private Vector3 _direction;
        private Vector3 _rotation;
        private Vector3 _scale;
        private Vector3 _velocity;
        private Matrix4 _model;
        public Vector3 Position { get => _position; set => _position = value; }
        public Vector3 Direction { get => _direction; set => _direction = value; }
        public Vector3 Rotation { get => _rotation; set => _rotation = value; }
        public Vector3 Scale { get => _scale; set => _scale = value; }
        public Vector3 Velocity { get => _velocity; set => _velocity = value; }
        public Matrix4 Model { get => _model; set => _model = value; }
        public Transform(Vector3 position)
        {
            _position = position;
            _direction = default;
            _scale = new Vector3(1);
            _rotation = default;
            _model = Matrix4.Identity;
            _velocity = new Vector3(0);
        }

        public Transform(Vector3 position, Vector3 direction)
            : this(position) => _direction = direction;

        public Transform(Vector3 position, Vector3 direction, Vector3 rotation)
            : this(position, direction) => _rotation = rotation;

        public Transform(Vector3 position, Vector3 direction, Vector3 rotation, Vector3 scale)
            : this(position, direction, rotation) => _scale = scale;

        public Transform(Vector3 position, Vector3 direction, Vector3 rotation, Vector3 scale, Vector3 velocity)
            : this(position, direction, rotation, scale) => _velocity = velocity;


        public void Render(Shader shader, RenderFlags flags)
            => Game.EngineGL.UseShader(shader)
                            .SetShaderData("model", Model);

        public void SetLightValuesShader(Shader shader, string name)
            => Game.EngineGL
            .SetShaderData(name + "position", Position)
            .SetShaderData(name + "direction", Direction);

        public void CreateRotation(Vector3 rotation) => Rotation += rotation;

        public void CreateScale(Vector3 scale) => Scale += scale;

        public void CreateTranslation(Vector3 transform) => Position += transform;
        public void Update(float dt, EngineRigidBody rb)
        {
            Matrix4 mat = Matrix4.Identity;
            bool hasRb = rb is not null && rb.MotionState is not null;
            if (hasRb)
            {
                mat = BulletExtensions.GetMatrix(rb.MotionState.WorldTransform);
                Position = mat.ExtractTranslation();
            }
            var t1 = Matrix4.CreateTranslation(Position);
            var r1 = Matrix4.CreateRotationX(Rotation.X);
            var r2 = Matrix4.CreateRotationY(Rotation.Y);
            var r3 = Matrix4.CreateRotationZ(Rotation.Z);
            var s = Matrix4.CreateScale(Scale);
            Model = r1 * r2 * r3 * s * t1;
            if (hasRb)
            {
                Model = mat.ClearTranslation() * Model;
            }
        }
        public async Task FixedUpdate(EngineRigidBody rb)
        {
            /*lock (locker)
            {
                Matrix4 mat = default;
                if (rb is not null && rb.MotionState is not null)
                {
                    mat = BulletExtensions.GetMatrixByOpenTKMat(rb.MotionState.WorldTransform);
                    Position = mat.ExtractTranslation();
                }
                var t1 = Matrix4.CreateTranslation(Position);
                var r1 = Matrix4.CreateRotationX(Rotation.X);
                var r2 = Matrix4.CreateRotationY(Rotation.Y);
                var r3 = Matrix4.CreateRotationZ(Rotation.Z);
                var s = Matrix4.CreateScale(Scale);
                Model = r1 * r2 * r3 * s * t1;
                if (mat != default)
                {
                    Model = mat.ClearTranslation() * Model;
                }
            }
            await Task.CompletedTask;*/
        }

    }
}
