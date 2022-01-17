using BulletSharp.Math;
using OpenTK.Mathematics;

namespace Engine
{
    /// <summary>
    /// Класс предназначен для удобного взимодействия с библиотекой <see cref="BulletSharp"/>
    /// </summary>
    public static class BulletExtensions
    {
        /// <summary>
        /// Преобразование <see cref="BulletSharp.Math.Vector3"/> в <see cref="OpenTK.Mathematics.Vector3"/>
        /// </summary>
        public static BulletSharp.Math.Vector3 GetVector(OpenTK.Mathematics.Vector3 vector)
            => new BulletSharp.Math.Vector3(vector.X, vector.Y, vector.Z);

        /// <summary>
        /// Преобразование <see cref="OpenTK.Mathematics.Vector3"/>  в <see cref="BulletSharp.Math.Vector3"/> 
        /// </summary>
        public static OpenTK.Mathematics.Vector3 GetVector(BulletSharp.Math.Vector3 vector)
            => new OpenTK.Mathematics.Vector3((float)vector.X, (float)vector.Y, (float)vector.Z);

        /// <summary>
        /// Преобразование <see cref="Matrix4"/>  в <see cref="Matrix"/>
        /// </summary>
        public static Matrix GetMatrix(Matrix4 mat) => new Matrix
                (
                mat.M11,
                mat.M12,
                mat.M13,
                mat.M14,

                mat.M21,
                mat.M22,
                mat.M23,
                mat.M24,

                mat.M31,
                mat.M32,
                mat.M33,
                mat.M34,

                mat.M41,
                mat.M42,
                mat.M43,
                mat.M44
                );

        /// <summary>
        /// Преобразование <see cref="Matrix"/>  в <see cref="Matrix4"/>
        /// </summary>
        public static Matrix4 GetMatrix(Matrix mat)
            => new Matrix4
                (
                (float)mat.M11,
                (float)mat.M12,
                (float)mat.M13,
                (float)mat.M14,

                (float)mat.M21,
                (float)mat.M22,
                (float)mat.M23,
                (float)mat.M24,

                (float)mat.M31,
                (float)mat.M32,
                (float)mat.M33,
                (float)mat.M34,

                (float)mat.M41,
                (float)mat.M42,
                (float)mat.M43,
                (float)mat.M44
                );
    }
}
