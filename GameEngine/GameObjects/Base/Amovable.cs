using OpenTK.Mathematics;

namespace GameEngine.GameObjects.Base
{
    public abstract class AMovable
    {
        public Vector3 Position;
        public Vector3 Direction;
        public Vector3 Rotation;
        public Vector3 Scale;
        public float Velocity;
        public Matrix4 _model = Matrix4.Identity;
        public AMovable(Vector3 position, Vector3 direction, Vector3 rotation, Vector3 scale, float velocity, Matrix4 model)
        {
            Position = position;
            Direction = direction;
            Rotation = rotation;
            Scale = scale;
            Velocity = velocity;
            _model = model;
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
