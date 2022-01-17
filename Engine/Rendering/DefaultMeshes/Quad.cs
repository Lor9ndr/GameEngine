using Engine.Rendering.GameObjects;
using Engine.Structs;
using OpenTK.Mathematics;

namespace Engine.Rendering.DefaultMeshes
{
    public static class Quad
    {
        private static readonly Vertex[] _quadVertices = {
            // positions        // texture Coords
            new Vertex(new Vector3(-1.0f,  1.0f, 0.0f),new Vector2( 0.0f, 1.0f)),
            new Vertex(new Vector3(-1.0f, -1.0f, 0.0f),new Vector2( 0.0f, 0.0f)),
            new Vertex(new Vector3( 1.0f,  1.0f, 0.0f), new Vector2(1.0f, 1.0f)),
            new Vertex(new Vector3(1.0f, -1.0f, 0.0f),new Vector2( 1.0f, 0.0f)),
            };
        private static readonly uint[] _quadIndices =
        {
            0, 1, 2, 2, 3, 0,
        };
        private static readonly Mesh _quad = new Mesh(_quadVertices, _quadIndices);

        public static Mesh GetQuad() => new(_quad.ObjectSetupper.GetVertices(), _quad.ObjectSetupper.GetIndices(), vao: _quad.ObjectSetupper.GetVAOClass());
    }
}
