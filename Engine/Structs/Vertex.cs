using OpenTK.Mathematics;

namespace Engine.Structs
{
    public struct Vertex
    {
        public const int MAX_BONE_INFLUENCE = 4;
        public static int Size => ((3 + 3 + 2 + 3 + 3 + 4) * sizeof(float)) + 4 * sizeof(int);
        public Vector3 Position;
        public Vector3 Normal;
        public Vector2 TexCoords;
        public Vector3 Tangent;
        public Vector3 Bitangent;
        public Vector4i BoneIDs;
        public Vector4 Weights;
        public Vertex(Vector3 position)
        {
            Position = position;
            Normal = new Vector3();
            TexCoords = new Vector2();
            Tangent = new Vector3();
            Bitangent = new Vector3();
            BoneIDs = new Vector4i(-1);
            Weights = new Vector4(0);
        }
        public Vertex(Vector3 position, Vector2 texCoords)
            : this(position)
        {
            Position = position;
            TexCoords = texCoords;
        }
        public Vertex(Vector3 position, Vector3 normal, Vector2 texCoords)
            : this(position, texCoords)
        {
            Position = position;
            Normal = normal;
            TexCoords = texCoords;
        }


        public Vertex(float positionX, float positionY, float positionZ)
            : this(new Vector3(positionX, positionY, positionZ))
        {
        }
        public Vertex(Vector3 position, Vector3 normal, Vector2 texCoords, Vector3 tangent, Vector3 bitangent)
            : this(position, normal, texCoords)
        {
            Tangent = tangent;
            Bitangent = bitangent;
        }

        public Vertex(Vector3 position, Vector3 normal, Vector2 texCoords, Vector3 tangent, Vector3 bitangent, Vector4i boneIDs)
            : this(position, normal, texCoords, tangent, bitangent)
        {
            BoneIDs = boneIDs;
        }

        public Vertex(Vector3 position, Vector3 normal, Vector2 texCoords, Vector3 tangent, Vector3 bitangent, Vector4i boneIDs, Vector4 weights)
            : this(position, normal, texCoords, tangent, bitangent, boneIDs)
        {
            Weights = weights;
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
