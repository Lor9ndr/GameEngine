using OpenTK.Mathematics;

namespace Engine.Extensions
{
    public static class AssimpExtensions
    {
        public static Matrix4 GetMatrix4FromAssimpMatrix(this Assimp.Matrix4x4 mat)
        {
            Matrix4 result = new Matrix4();
            result.Column0 = new Vector4(mat.A1, mat.A2, mat.A3, mat.A4);
            result.Column1 = new Vector4(mat.B1, mat.B2, mat.B3, mat.B4);
            result.Column2 = new Vector4(mat.C1, mat.C2, mat.C3, mat.C4);
            result.Column3 = new Vector4(mat.D1, mat.D2, mat.D3, mat.D4);
            return result;
        }
        /*public static Matrix4 GetMatrix4FromAssimpMatrix(this Assimp.Matrix4x4 mat)
      {
          Matrix4 result = new Matrix4();
          result.Column0 = new Vector4(mat.A1, mat.B1, mat.C1, mat.D1);
          result.Column1 = new Vector4(mat.A2, mat.B2, mat.C2, mat.D2);
          result.Column2 = new Vector4(mat.A3, mat.B3, mat.C3, mat.D3);
          result.Column3 = new Vector4(mat.A4, mat.B4, mat.C4, mat.D4);
          return result;
      }*/
    }
}
