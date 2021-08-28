using GameEngine.GameObjects;
using GameEngine.Structs;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine.DefaultMeshes
{
    public static class FullScreenQuad
    {
        private static Vertex[] _quadVertices = {
            // positions        // texture Coords
            new Vertex(new Vector3(-1.0f,  1.0f, 0.0f),new Vector2( 0.0f, 1.0f)),
            new Vertex(new Vector3(-1.0f, -1.0f, 0.0f),new Vector2( 0.0f, 0.0f)),
            new Vertex(new Vector3( 1.0f,  1.0f, 0.0f), new Vector2(1.0f, 1.0f)),
            new Vertex(new Vector3(1.0f, -1.0f, 0.0f),new Vector2( 1.0f, 0.0f)),
            };
        private static int[] _quadIndices =
        {
              0, 1, 3,
              1, 2, 3,
        };
        private static Mesh _fsQuad = new Mesh(_quadVertices, _quadIndices);
        public static Mesh GetQuad() => new Mesh(_fsQuad.Vertices, _fsQuad.Indices, vao: _fsQuad.ObjectSetupper.GetVAOClass());

    }
}
