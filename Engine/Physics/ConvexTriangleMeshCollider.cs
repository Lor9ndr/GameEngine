using BulletSharp;
using Engine.Rendering.Enums;
using Engine.Rendering.GameObjects;

namespace Engine.Physics
{
    public class ConvexTriangleMeshCollider : ConvexTriangleMeshShape, IDrawableCollider
    {

        public GameObject BaseCollider { get; set; } = new GameObject();

        public ConvexTriangleMeshCollider(StridingMeshInterface meshInterface, bool calcAabb = true) : base(meshInterface, calcAabb)
        {
           
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
