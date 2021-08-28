using OpenTK.Mathematics;

namespace GameEngine.Bases
{
    public class Transform : IComponent
    {
        private Vector3 _position;
        private Vector3 _direction;
        private Vector3 _rotation;
        private Vector3 _scale;
        private float _velocity;
        private Matrix4 _model = Matrix4.Zero;
        public Vector3 Position { get => _position; set => _position = value; }

        public Vector3 Direction { get => _direction; set => _direction = value; }
        public Vector3 Rotation { get => _rotation; set => _rotation = value; }
        public Vector3 Scale { get => _scale; set => _scale = value; }
        public float Velocity { get => _velocity; set => _velocity = value; }
        public Matrix4 Model { get => _model; set => _model = value; }
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
            Scale = new Vector3(1);
            Velocity = 0;
            Rotation = new Vector3(0);
            Model = Matrix4.Zero;

        }




    }
}
