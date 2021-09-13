using GameEngine.GameObjects;
using GameEngine.Intefaces;
using GameEngine.RenderPrepearings;
using GameEngine.Structs;
using OpenTK.Mathematics;
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
        new Vertex(new Vector3(-1.0f,  1.0f, -1.0f), new Vector2(0.0f, 0.0f)),
        new Vertex(new Vector3(-1.0f, -1.0f, -1.0f), new Vector2(1.0f, 0.0f)),
        new Vertex(new Vector3(1.0f, -1.0f, -1.0f), new Vector2(1.0f, 1.0f)),
        new Vertex(new Vector3(1.0f, -1.0f, -1.0f), new Vector2(1.0f, 1.0f)),
        new Vertex(new Vector3(1.0f,  1.0f, -1.0f), new Vector2(0.0f, 1.0f)),
        new Vertex(new Vector3(-1.0f,  1.0f, -1.0f),new Vector2(0.0f, 0.0f)),

        new Vertex(new Vector3(-1.0f, -1.0f,  1.0f), new Vector2(0.0f, 0.0f)),
        new Vertex(new Vector3(-1.0f, -1.0f, -1.0f), new Vector2(1.0f, 0.0f)),
        new Vertex(new Vector3(-1.0f,  1.0f, -1.0f), new Vector2(1.0f, 1.0f)),
        new Vertex(new Vector3(-1.0f,  1.0f, -1.0f), new Vector2(1.0f, 1.0f)),
        new Vertex(new Vector3(-1.0f,  1.0f,  1.0f), new Vector2(0.0f, 1.0f)),
        new Vertex(new Vector3(-1.0f, -1.0f,  1.0f), new Vector2(0.0f, 0.0f)),

        new Vertex(new Vector3(1.0f, -1.0f, -1.0f), new Vector2(1.0f, 0.0f)),
        new Vertex(new Vector3( 1.0f, -1.0f,  1.0f), new Vector2(1.0f, 1.0f)),
        new Vertex(new Vector3( 1.0f,  1.0f,  1.0f), new Vector2(0.0f, 1.0f)),
        new Vertex(new Vector3( 1.0f,  1.0f,  1.0f), new Vector2(0.0f, 1.0f)),
        new Vertex(new Vector3( 1.0f,  1.0f, -1.0f), new Vector2(0.0f, 0.0f)),
        new Vertex(new Vector3( 1.0f, -1.0f, -1.0f), new Vector2(1.0f, 0.0f)),

        new Vertex(new Vector3(-1.0f, -1.0f,  1.0f), new Vector2(1.0f, 0.0f)),
        new Vertex(new Vector3(-1.0f,  1.0f,  1.0f), new Vector2(1.0f, 1.0f)),
        new Vertex(new Vector3( 1.0f,  1.0f,  1.0f), new Vector2(0.0f, 1.0f)),
        new Vertex(new Vector3( 1.0f,  1.0f,  1.0f), new Vector2(0.0f, 1.0f)),
        new Vertex(new Vector3( 1.0f, -1.0f,  1.0f), new Vector2(0.0f, 0.0f)),
        new Vertex(new Vector3(-1.0f, -1.0f,  1.0f), new Vector2(1.0f, 0.0f)),

        new Vertex(new Vector3(-1.0f,  1.0f, -1.0f), new Vector2(0.0f, 1.0f)),
        new Vertex(new Vector3( 1.0f,  1.0f, -1.0f), new Vector2(1.0f, 1.0f)),
        new Vertex(new Vector3( 1.0f,  1.0f,  1.0f), new Vector2(1.0f, 0.0f)),
        new Vertex(new Vector3( 1.0f,  1.0f,  1.0f), new Vector2(1.0f, 0.0f)),
        new Vertex(new Vector3(-1.0f,  1.0f,  1.0f), new Vector2(0.0f, 0.0f)),
        new Vertex(new Vector3(-1.0f,  1.0f, -1.0f), new Vector2(0.0f, 1.0f)),

        new Vertex(new Vector3(-1.0f, -1.0f, -1.0f), new Vector2(0.0f, 1.0f)),
        new Vertex(new Vector3(-1.0f, -1.0f,  1.0f), new Vector2(1.0f, 1.0f)),
        new Vertex(new Vector3( 1.0f, -1.0f, -1.0f), new Vector2(1.0f, 0.0f)),
        new Vertex(new Vector3( 1.0f, -1.0f, -1.0f), new Vector2(1.0f, 0.0f)),
        new Vertex(new Vector3(-1.0f, -1.0f,  1.0f), new Vector2(0.0f, 0.0f)),
        new Vertex(new Vector3( 1.0f, -1.0f,  1.0f), new Vector2(0.0f, 1.0f))
    };

        private static Mesh _mesh = new Mesh(_verttices);
        public static Mesh GetMesh() => _mesh;
        public static Mesh GetTexturedMesh(Texture texture)
        {
            var textures = new List<Texture>() { texture };
            return new Mesh(_verttices, textures: textures, vao: _mesh.ObjectSetupper.GetVAOClass(),setupLevel:Enums.SetupLevel.TexCoords);
        }
    }
}
