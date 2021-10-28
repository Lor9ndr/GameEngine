using OpenTK;
using OpenTK.Mathematics;

namespace Engine.Structs
{
    public struct Vertex
    {
        public static int Size => (3 + 3 + 2 + 3 + 3) * sizeof(float);
        public static int SizePositionOnly => 3 * sizeof(float);
        public Vector3 Position;
        public Vector3 Normal;
        public Vector2 TexCoords;
        public Vector3 Tangent;
        public Vector3 Bitangent;
        public Vertex(Vector3 position)
        {
            Position = position;
            Normal = new Vector3();
            TexCoords = new Vector2();
            Tangent = new Vector3();
            Bitangent = new Vector3();
        }
        public Vertex(Vector3 position, Vector2 texCoords)
        {
            Position = position;
            Normal = new Vector3();
            TexCoords = texCoords;
            Tangent = new Vector3();
            Bitangent = new Vector3();
        }
        public Vertex(Vector3 position,Vector3 normal, Vector2 texCoords)
        {
            Position = position;
            Normal = normal;
            TexCoords = texCoords;
            Tangent = new Vector3();
            Bitangent = new Vector3();
        }


        public Vertex(float positionX, float positionY, float positionZ)
        {
            Position = new Vector3(positionX,positionY,positionZ);
            Normal = new Vector3();
            TexCoords = new Vector2();
            Tangent = new Vector3();
            Bitangent = new Vector3();
        }
        public Vertex(Vector3 position, Vector3 normal, Vector2 texCoords, Vector3 tangent, Vector3 bitangent)
        {
            Position = position;
            Normal = normal;
            TexCoords = texCoords;
            Tangent = tangent;
            Bitangent = bitangent;
        }
    }
    public static class VertexExtensions
    {
        public static float[] ConvertToFloatArray(this Vertex[] v)
        {
            float[] res = new float[v.Length * Vertex.Size / sizeof(float)];
            for (int i = 0; i < res.Length - Vertex.Size / sizeof(float); i++)
            {
                res[i] = v[i].Position.X;
                res[i + 1] = v[i].Position.Y;
                res[i + 2] = v[i].Position.Z;

                res[i + 3] = v[i].Normal.X;
                res[i + 4] = v[i].Normal.Y;
                res[i + 5] = v[i].Normal.Z;

                res[i + 6] = v[i].TexCoords.X;
                res[i + 7] = v[i].TexCoords.Y;

                res[i + 8] = v[i].Tangent.X;
                res[i + 9] = v[i].Tangent.Y;
                res[i + 10] = v[i].Tangent.Z;

                res[i + 11] = v[i].Bitangent.X;
                res[i + 12] = v[i].Bitangent.Y;
                res[i + 13] = v[i].Bitangent.Z;
            }
            return res;
        }
       /* public static Vertex[] ConvertFromFloatArray(this float[] ar, bool hasNormals= false, bool hasTexCoords=false, bool hasTangets=false, bool hasBitangents=false)
        {
            Vertex[] result = new Vertex[ar.Length / Vertex.Size / sizeof(float)];
            for (int i = 0; i < ar.Length - ; i++)
            {

                if (true)
                {

                }
            }
        }*/
    }
}
