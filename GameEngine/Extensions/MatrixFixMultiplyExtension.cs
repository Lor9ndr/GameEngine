using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine.Extensions
{
    public static class MatrixFixMultiplyExtension
    {
        public static Matrix4 Multiply(this Matrix4 m1, Matrix4 m2)
        {
            Matrix4 result = Matrix4.Zero;
            for (int m1Row = 0; m1Row < 4; m1Row++)
            {
                for (int m2col = 0; m2col < 4; m2col++)
                {
                    for (int m1col = 0; m1col < 4; m1col++)
                    {
                        result[m1Row, m2col] += m1[m1Row, m1col] * m2[m1col, m2col];
                    }
                }
            }
            return result;
        }
    }
}
