using BulletSharp;
using Engine.Components;
using Engine.Rendering.Enums;
using OpenTK.Mathematics;
using System.Collections.Generic;

namespace Engine.Physics
{
    public class EngineRigidBody : RigidBody, IComponent
    {
        private static List<EngineRigidBody> _bodies = new List<EngineRigidBody>();

        public EngineRigidBody(RigidBodyConstructionInfo constructionInfo) : base(constructionInfo)
        {
            _bodies.Add(this);
        }

        public void Render(Shader shader, RenderFlags flags)
        {
            Game.EngineGL.UseShader(shader); ;
            if ("Ground".Equals(UserObject))
            {
                Game.EngineGL.SetShaderData("lightColor", new Vector3(0, 255, 0));
            }

            if (ActivationState == ActivationState.ActiveTag)
                Game.EngineGL.SetShaderData("lightColor", new Vector3(255, 0, 0));
            else
                Game.EngineGL.SetShaderData("lightColor", new Vector3(0, 255, 0));

            (CollisionShape as IDrawableCollider).Draw(shader, flags);
        }

        public void Update()
        {
            (CollisionShape as IDrawableCollider).Update(0, this);
        }


    }
}
