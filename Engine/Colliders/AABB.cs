using OpenTK.Mathematics;

namespace Engine.Colliders
{
    internal class AABB
    {
        private Vector3 _min;
        private Vector3 _max;
        public bool IsValid => _min.X < _max.X && _min.Y < _max.Y && _min.Z < _max.Z;
        public Vector3 Center => (_max + _min) / 2;

        public void Fix()
        {
            float tmp;
            for (int i = 0; i < 3; i++)
            {
                if (_min[i] > _max[i])
                {
                    tmp = _min[i];
                    _min[i] = _max[i];
                    _max[i] = tmp;
                }
            }
        }
    }
}
