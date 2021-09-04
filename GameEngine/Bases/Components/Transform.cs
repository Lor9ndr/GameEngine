using OpenTK.Mathematics;

namespace GameEngine.Bases.Components
{
    public class Transform : IComponent
    {
        public Vector3 Position;
        public Vector3 Direction;
        public Vector3 Rotation;
        public Vector3 Scale;
        public float Velocity;
        public Matrix4 Model;
        public Transform(Vector3 position, Vector3 direction, Vector3 rotation, Vector3 scale, float velocity, Matrix4 model= default)
            :this(position,direction)
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
            Velocity = 0;
            Rotation = new Vector3(0);
            Model = Matrix4.Zero;
        }
    }
}
