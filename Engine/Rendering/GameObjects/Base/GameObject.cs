using Engine;
using Engine.Components;
using Engine.GameObjects;
using Engine.Rendering.Enums;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Collections.Generic;

namespace Engine.Rendering.GameObjects
{
    public class GameObject
    {
        private Model _model;
        private Transform _transform;
        private RigidBody _rigidBody;
        private LightData _lightData;
        private ShadowData _shadowData;
        public GameObject(Model model)
        {
            Model = model;
            _transform = new Transform(Vector3.Zero);
        }

        public GameObject(Model model, Transform transform) 
            : this(model)
        {
            _transform = transform;
        }

        public GameObject(Model model, Transform transform, RigidBody rigidBody) 
            : this(model, transform)
        {
            _rigidBody = rigidBody;
        }

        public GameObject(Model model, Transform transform, RigidBody rigidBody, LightData lightData)
            : this(model, transform, rigidBody)
        {
            _lightData = lightData;
        }

        public GameObject(Model model, Transform transform, RigidBody rigidBody, LightData lightData, ShadowData shadowData) 
            : this(model, transform, rigidBody, lightData)
        {
            _shadowData = shadowData;
        }

        public Transform Transform { get => _transform; set => _transform = value; }
        public RigidBody RigidBody { get => _rigidBody; set => _rigidBody = value; }
        public LightData LightData { get => _lightData; set => _lightData = value; }
        public ShadowData ShadowData { get => _shadowData; set => _shadowData = value; }
        public Model Model { get => _model; set => _model = value; }

        public virtual void Render(Shader shader, RenderFlags flags) => Render(shader,flags, TextureTarget.Texture2D);
        public virtual void Render(Shader shader, RenderFlags flags, TextureTarget target = TextureTarget.Texture2D)
        {
            Transform.Render(shader, flags);
            Model.Render(shader,flags);
        }

        public void Update()
        {
            Transform.Update();
        }
    }
}
