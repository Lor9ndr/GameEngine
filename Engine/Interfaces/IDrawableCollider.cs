using Engine.Physics;
using Engine.Rendering.Enums;
using Engine.Rendering.GameObjects;

namespace Engine
{
    internal interface IDrawableCollider
    {
        public GameObject BaseCollider { get; set; }
        void Draw(Shader shader, RenderFlags flags);
        void Update(float deltaTime, EngineRigidBody rb);
    }
}
