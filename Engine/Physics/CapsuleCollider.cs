using BulletSharp;
using Engine.Components;
using Engine.Rendering.DefaultMeshes;
using Engine.Rendering.Enums;
using Engine.Rendering.GameObjects;

namespace Engine.Physics
{
    public class CapsuleCollider : CapsuleShape, IDrawableCollider
    {
        public GameObject BaseCollider { get; set; } = new GameObject(DefaultMesh.GetSphere);

        public CapsuleCollider(float radius, float height) : base(radius, height)
        {
            BaseCollider.Transform = new Transform();
            BaseCollider.Transform.Scale = new OpenTK.Mathematics.Vector3((float)radius + 0.1f, (float)height + 0.1f, (float)radius + 0.1f);
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
