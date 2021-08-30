using OpenTK.Mathematics;

namespace GameEngine.Bases.Components
{
    public class Transform : IComponent
    {
        private Vector3 _position;
        private Vector3 _direction;
        private Vector3 _rotation;
        private Vector3 _scale;
        private float _velocity;
        private Matrix4 _model;
        public Vector3 Position { get => _position; set => _position = value; }

        public Vector3 Direction { get => _direction; set => _direction = value; }
        public Vector3 Rotation { get => _rotation; set => _rotation = value; }
        public Vector3 Scale { get => _scale; set => _scale = value; }
        public float Velocity { get => _velocity; set => _velocity = value; }
        public Matrix4 Model { get => _model; set => _model = value; }
        public Transform(Vector3 position, Vector3 direction, Vector3 rotation, Vector3 scale, float velocity, Matrix4 model= default)
            :this(position,direction)
        {
            _rotation = rotation;
            _scale = scale;
            _velocity = velocity;
            _model = model;
        }
        public Transform(Vector3 position, Vector3 direction) 
            : this(position)
        {
            _direction = direction;
            

        }
        public Transform(Vector3 position)
        {
            _position = position;
            _direction = new Vector3(0);
            _scale = new Vector3(1);
            _velocity = 0;
            _rotation = new Vector3(0);
            _model = Matrix4.Zero;

        }




    }
}
