using Engine.GameObjects;
using Engine.GLObjects.Textures;
using Engine.Rendering.GameObjects;
using Engine.Structs;
using GameEngine.Enums;
using OpenTK;
using OpenTK.Mathematics;
using System.Collections.Generic;

namespace Engine.Rendering.DefaultMeshes
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
        public static Model GetModel() => new Model(_mesh);
        public static Model GetTexturedModel(Texture texture)
        {
            var textures = new List<Texture>() { texture };
            return new Model(new Mesh(_verttices, textures: textures, vao: _mesh.ObjectSetupper.GetVAOClass(),setupLevel:SetupLevel.TexCoords));
        }
    }
}
