using Engine.Components;
using Engine.GameObjects;
using Engine.Rendering.Enums;
using Engine.Rendering.GameObjects;
using Engine.Structs;
using OpenTK.Mathematics;
using System.Collections.Generic;

namespace Engine.Physics
{
    public class Box:GameObject
    {
        public List<Vector3> Positions;
        public List<Vector3> Sizes;

        public Box(Transform transform = default)
            :base(null, transform)
        {

            Vertex[] vertices = new Vertex[]
            {
                new Vertex( 0.5f,  0.5f,  0.5f),
                new Vertex(-0.5f,  0.5f,  0.5f),
                new Vertex(-0.5f, -0.5f,  0.5f),
                new Vertex( 0.5f, -0.5f,  0.5f),
                new Vertex( 0.5f, 0.5f, -0.5f),
                new Vertex(-0.5f, 0.5f, -0.5f),
                new Vertex(-0.5f, -0.5f, -0.5f),
                new Vertex( 0.5f, -0.5f, -0.5f)
            };
            int[] indices =
            {
                 // front face (+ve z)
                 0, 1,
                 1, 2,
                 2, 3,
                 3, 0,
                 // back face (-ve z)
                 4, 5,
                 5, 6,
                 6, 7,
                 7, 4,
                 // right face (+ve x)
                 0, 4,
                 3, 7,
                 // left face (-ve x)
                 1, 5,
                 2, 6
            };
            Model =  new Model(new Mesh(vertices, indices));
        }
    }
}