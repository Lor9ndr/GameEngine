using BulletSharp;
using BulletSharp.Math;
using Engine.Components;
using Engine.Rendering.DefaultMeshes;
using Engine.Rendering.Enums;
using Engine.Rendering.GameObjects;

namespace Engine.Physics
{
    public class BoxCollider : BoxShape, IDrawableCollider
    {
        public GameObject BaseCollider { get; set; } = new GameObject(DefaultMesh.GetDefaultCube);

        public BoxCollider(Vector3 boxHalfExtents)
            : base(boxHalfExtents)
        {
            BaseCollider.Transform = new Transform();
            BaseCollider.Transform.Scale = new OpenTK.Mathematics.Vector3((float)boxHalfExtents.X + 0.1f, (float)boxHalfExtents.Y + 0.1f, (float)boxHalfExtents.Z + 0.1f);
        }

        public BoxCollider(float boxHalfExtent)
            : base(boxHalfExtent)
        {
            BaseCollider.Transform = new Transform();
            BaseCollider.Transform.Scale = new OpenTK.Mathematics.Vector3((float)boxHalfExtent + 0.1f);
        }

        public BoxCollider(float boxHalfExtentX, float boxHalfExtentY, float boxHalfExtentZ)
            : base(boxHalfExtentX, boxHalfExtentY, boxHalfExtentZ)
        {
            BaseCollider.Transform = new Transform();
            BaseCollider.Transform.Scale = new OpenTK.Mathematics.Vector3((float)boxHalfExtentX + 0.1f, (float)boxHalfExtentY + 0.1f, (float)boxHalfExtentZ + 0.1f);
        }


        public void Draw(Shader shader, RenderFlags flags)
        {
            BaseCollider.Render(shader, flags, OpenTK.Graphics.OpenGL4.TextureTarget.Texture2D, OpenTK.Graphics.OpenGL4.PrimitiveType.Lines);
        }
        public void Update(float deltaTime, EngineRigidBody rb)
        {
            BaseCollider.Transform.Update(deltaTime, rb);
        }
    }
}
