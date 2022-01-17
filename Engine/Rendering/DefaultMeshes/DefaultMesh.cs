using Engine.GameObjects;
using Engine.GLObjects.Textures;
using Engine.Rendering.GameObjects;
using Engine.Structs;
using GameEngine.Enums;
using OpenTK.Mathematics;
using System.Collections.Generic;

namespace Engine.Rendering.DefaultMeshes
{
    public static class DefaultMesh
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

        private static readonly Mesh _mesh = new Mesh(_verttices, null);
        public static Model GetCube() => new Model(_mesh, "Cube");
        public static Model GetDefaultCube = new Model(_mesh, Texture.GetDefaultTextures, "Cube");
        public static Model GetTexturedCube(Texture texture)
        {
            var textures = new List<Texture>() { texture };
            return new Model(new Mesh(_verttices, textures: textures, vao: _mesh.ObjectSetupper.GetVAOClass(), setupLevel: SetupLevel.TexCoords), "CubeTextured");
        }
        public static Model GetSphere = new(Game.OBJ_PATH + "Sphere.obj");
    }
}
