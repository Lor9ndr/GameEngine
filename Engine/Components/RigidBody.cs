using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Components
{
    public class RigidBody : IComponent
    {
        private Transform _transform;
        public int ID;
        private static int _id = 0;
        private Vector3 _acceleration;
        public const float COLLISION_THRESHOLD =  0.05f;
        public Transform Transform { get => _transform; set => _transform = value; }
        public Vector3 Acceleration { get => _acceleration; set => _acceleration = value; }
        public Vector3 Size { get => _size; set => _size = value; }

        private Vector3 _size;
        private float _lastCollision;
        public int LastCollisionID;

        public RigidBody(Transform transform, Vector3 acceleration)
        {
            Transform = transform;
            Acceleration = acceleration;
            ID = _id;
            _id++;
        }
        public void ApplyForce(Vector3 force)
        {
            Acceleration += force / Transform.Mass;
        }
        public void ApplyForce(Vector3 direction, float magnitude) => ApplyForce(direction * magnitude);

        public void ApplyAcceleration(Vector3 acceleration) => Acceleration += acceleration;

        public void ApplyAcceleration(Vector3 direction, float magnitude) => ApplyAcceleration(direction * magnitude);

        public void ApplyImpulse(Vector3 force, float dt) => Transform.Velocity += force / Transform.Mass * dt;

        public void ApplyImpulse(Vector3 direction, float magnitude, float dt) => ApplyAcceleration(direction * magnitude, dt);

        public void TransferEnergy(float joules, Vector3 direction)
        {
            if (joules == 0)
            {
                return;
            }
            Vector3 deltaV = MathF.Sqrt(2*MathF.Abs(joules)/Transform.Mass)* direction;
            Transform.Velocity += joules > 0 ? deltaV : -deltaV;
        }
        public void HandleCollision(RigidBody rb, Vector3 norm)
        {
            if (_lastCollision >= COLLISION_THRESHOLD || LastCollisionID != rb.LastCollisionID)
            {
                var vel = new System.Numerics.Vector3(Transform.Velocity.X, Transform.Velocity.Y, Transform.Velocity.Z);
                var nrml = new System.Numerics.Vector3(norm.X, norm.Y, norm.Z);
                var value = System.Numerics.Vector3.Reflect(vel, nrml);
                Transform.Velocity = new Vector3(value.X, value.Y, value.Z);
            }
            LastCollisionID = rb.LastCollisionID;
        }

    }
}
