using BulletSharp;
using Engine.Components;
using Engine.Rendering.DefaultMeshes;
using Engine.Rendering.Enums;
using Engine.Rendering.GameObjects;
using OpenTK.Mathematics;

namespace Engine.Physics
{
    public class SphereCollider : SphereShape, IComponent, IDrawableCollider
    {
        public GameObject BaseCollider { get; set; } = new GameObject(DefaultMesh.GetSphere);

        public SphereCollider(float radius) : base(radius)
        {
            BaseCollider.Transform.Scale = new Vector3((float)radius + 0.1f);
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
