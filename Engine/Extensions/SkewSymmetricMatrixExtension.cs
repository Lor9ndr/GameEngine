using OpenTK.Mathematics;

namespace Engine.Extensions
{
    public class SkewSymmetricMatrixExtension
    {
        public static Matrix4 SkewSymmetricMatrix(Vector3 vector)
        {
            Matrix4 result = Matrix4.Identity;

            result[0, 0] = 0;
            result[0, 1] = -vector.Z;
            result[0, 2] = vector.Y;
            result[1, 0] = vector.Z;
            result[1, 1] = 0;
            result[1, 2] = -vector.X;
            result[2, 0] = -vector.Y;
            result[2, 1] = vector.X;
            result[2, 2] = 0;

            return result;
        }
    }
}
