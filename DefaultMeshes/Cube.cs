using GameEngine.GameObjects;
using GameEngine.Intefaces;
using GameEngine.RenderPrepearings;
using GameEngine.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine.DefaultMeshes
{
    public static class Cube
    {
        private static readonly Vertex[] _verttices = {
        // positions
        new Vertex(-1.0f,  1.0f, -1.0f),
        new Vertex(-1.0f, -1.0f, -1.0f),
        new Vertex(1.0f, -1.0f, -1.0f),
        new Vertex(1.0f, -1.0f, -1.0f),
        new Vertex(1.0f,  1.0f, -1.0f),
        new Vertex(-1.0f,  1.0f, -1.0f),

        new Vertex(-1.0f, -1.0f,  1.0f),
        new Vertex(-1.0f, -1.0f, -1.0f),
        new Vertex(-1.0f,  1.0f, -1.0f),
        new Vertex(-1.0f,  1.0f, -1.0f),
        new Vertex(-1.0f,  1.0f,  1.0f),
        new Vertex(-1.0f, -1.0f,  1.0f),

        new Vertex(1.0f, -1.0f, -1.0f),
        new Vertex( 1.0f, -1.0f,  1.0f),
        new Vertex( 1.0f,  1.0f,  1.0f),
        new Vertex( 1.0f,  1.0f,  1.0f),
        new Vertex( 1.0f,  1.0f, -1.0f),
        new Vertex( 1.0f, -1.0f, -1.0f),

        new Vertex(-1.0f, -1.0f,  1.0f),
        new Vertex(-1.0f,  1.0f,  1.0f),
        new Vertex( 1.0f,  1.0f,  1.0f),
        new Vertex( 1.0f,  1.0f,  1.0f),
        new Vertex( 1.0f, -1.0f,  1.0f),
        new Vertex(-1.0f, -1.0f,  1.0f),

        new Vertex(-1.0f,  1.0f, -1.0f),
        new Vertex( 1.0f,  1.0f, -1.0f),
        new Vertex( 1.0f,  1.0f,  1.0f),
        new Vertex( 1.0f,  1.0f,  1.0f),
        new Vertex(-1.0f,  1.0f,  1.0f),
        new Vertex(-1.0f,  1.0f, -1.0f),

        new Vertex(-1.0f, -1.0f, -1.0f),
        new Vertex(-1.0f, -1.0f,  1.0f),
        new Vertex( 1.0f, -1.0f, -1.0f),
        new Vertex( 1.0f, -1.0f, -1.0f),
        new Vertex(-1.0f, -1.0f,  1.0f),
        new Vertex( 1.0f, -1.0f,  1.0f)
    };

        private static Mesh _mesh = new Mesh(_verttices);
        public static Mesh GetMesh() => _mesh;
    }
}
