using Engine.Rendering.GameObjects;
using Engine.Structs;
using OpenTK.Mathematics;

namespace Engine.Rendering.DefaultMeshes
{
    public static class FullScreenQuad
    {
        private static Vertex[] _quadVertices = {
            // positions        // texture Coords
            new Vertex(new Vector3( -1.0f,  1.0f, 0.0f),new Vector2(0,1)),
            new Vertex(new Vector3(-1.0f, -1.0f, 0.0f),new Vector2(0.0f, 0.0f)),
            new Vertex(new Vector3(1.0f,  1.0f, 0.0f), new Vector2(1.0f,1.0f)),
            new Vertex(new Vector3(1.0f, -1.0f, 0.0f),new Vector2(1.0f, 0.0f)),
            };
        private static uint[] _quadIndices =
        {
              0, 1, 2, 2, 3, 0,
        };
        private static Mesh _fsQuad = new Mesh(_quadVertices, _quadIndices);
        public static Mesh GetQuad() => _fsQuad;

    }
}
