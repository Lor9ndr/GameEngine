using Engine.Components;
using OpenTK.Mathematics;

namespace Engine.Physics
{
    public struct Face
    {
        private readonly CollisionMesh _mesh;
        private readonly int _i1;
        private readonly int _i2;
        private readonly int _i3;
        private readonly Vector3 _baseNormal;
        private readonly Vector3 _norm;

        public Face(CollisionMesh mesh, int i1, int i2, int i3, Vector3 baseNormal, Vector3 norm)
        {
            _mesh = mesh;
            _i1 = i1;
            _i2 = i2;
            _i3 = i3;
            _baseNormal = baseNormal;
            _norm = norm;
        }
        public bool CollideWithFace(RigidBody thisRb, Face face, RigidBody faceRb, Vector3 retNorm)
        {
            /*Vector3 P1 = thisRb.Transform.Model * _mesh.Points[_i1];
            Vector3 P1 = thisRb.Transform.Model * _mesh.Points[_i1];
            Vector3 P1 = thisRb.Transform.Model * _mesh.Points[_i1];*/
            return true;
        }
        public bool collidesWithSphere(RigidBody thisRB, BoundingRegion br, Vector3 retNorm)
        {
            return true;
        }
    }
}
