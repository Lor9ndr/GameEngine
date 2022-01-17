using Engine.Components;
using Engine.GameObjects;
using Engine.Physics;
using Engine.Rendering.Enums;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Engine.Rendering.GameObjects
{
    public class GameObject : IDisposable
    {
        public Model Model;
        public Transform Transform;
        public EngineRigidBody RigidBody;
        public LightData LightData;
        public ShadowData ShadowData;

        public List<GameObject> Childrens = new List<GameObject>();
        public GameObject Parent;
        public GameObject()
        {
        }
        public GameObject(Model model, EngineRigidBody rb = null)
        {
            Model = model;
            Transform = new Transform(Vector3.Zero);
            RigidBody = rb;
        }

        public GameObject(Model model, Transform transform, EngineRigidBody rb = null)
            : this(model, rb)
        {
            Transform = transform;

        }

        public GameObject(Model model, Transform transform, LightData lightData, EngineRigidBody rb = null)
            : this(model, transform, rb)
        {
            LightData = lightData;
        }

        public GameObject(Model model, Transform transform, LightData lightData, EngineRigidBody rb, ShadowData shadowData)
            : this(model, transform, lightData, rb)
        {
            ShadowData = shadowData;
        }

        public virtual void OnLoad()
        {

        }

        public virtual void Render(Shader shader, RenderFlags flags) => Render(shader, flags, TextureTarget.Texture2D);
        public virtual void Render(Shader shader, RenderFlags flags, TextureTarget target = TextureTarget.Texture2D, PrimitiveType type = PrimitiveType.Triangles)
        {
            Transform.Render(shader, flags);
            Model.Render(shader, flags, target, type);
        }
        public virtual async Task FixedUpdate()
        {
            if (Model is not null && Model.Animator is not null)
                await Model.Animator.FixedUpdate();
        }
        public virtual void Update(float dt)
        {
            Transform.Update(0, RigidBody);
            if (Parent is not null)
            {
                Transform.Model = Parent.Transform.Model * Transform.Model;
            }
            foreach (var child in Childrens)
            {
                child.Update(0);
            }
            if (RigidBody is not null)
            {
                RigidBody.Update();
            }
            if (Model?.Animator is not null)
            {
                Model.Animator.UpdateAnimation(dt);
            }
        }
        public void AddChildren(GameObject go)
        {
            go.Parent = this;
            this.Childrens.Add(go);
        }
        public void AddRigidBody(EngineRigidBody rb)
        {
            RigidBody = rb;
            //AddChildren(rb);
        }
        public void Dispose()
        {
            Model.Dispose();
            ShadowData.Dispose();
        }
    }
}
