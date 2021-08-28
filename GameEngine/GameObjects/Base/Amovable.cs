using GameEngine.Attribute;
using GameEngine.Bases;
using OpenTK.Mathematics;

namespace GameEngine.GameObjects.Base
{
    public abstract class AMovable:BaseObject
    {
        protected Transform _transform;
        public AMovable(Transform transform )
        {
            Transform = transform;
            AddComponent(Transform);
        }

        public Transform Transform { get => _transform; set => _transform = value; }

        public virtual void CreateRotation(Vector3 rotation)
        {
            Transform.Rotation += rotation;
        }

        public virtual void CreateScale(Vector3 scale)
        {
            Transform.Scale += scale;
        }

        public virtual void CreateTranslation(Vector3 transform)
        {
            Transform.Position += transform;
        }
        public virtual void Update()
        {
            Transform.Position += Transform.Direction * (Transform.Velocity * (float)Game.DeltaTime);
        }
    }
}
