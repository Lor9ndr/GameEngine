using Engine.Components;
using Engine.Rendering.GameObjects;
using OpenTK.Mathematics;
using System;
using System.Threading.Tasks;

namespace Engine.GameObjects.Lights
{
    public abstract class Light : GameObject
    {
        public float NearPlane { get => _nearPlane; set => _nearPlane = value; }

        public float FarPlane { get => _farPlane; set => _farPlane = value; }

        private float _farPlane;
        private float _nearPlane;

        private Matrix4 _lightSpaceMatrix;

        public Matrix4 LightSpaceMatrix { get => _lightSpaceMatrix; set => _lightSpaceMatrix = value; }


        public Light(Model model, LightData lightData, Transform transform)
            : base(model, transform)
        {
            LightData = lightData;
            ShadowData = new ShadowData();

        }
        public virtual Matrix4 GetProjection => throw new NotImplementedException();

        public virtual Task UpdateMatricesAsync() => throw new NotImplementedException();
        public void SetAmbient(Vector3 ambient)
        {
            LightData.PlusAmbient(ambient);
        }
        public override void Update(float dt)
        {
            base.Update(dt);
        }
        public override async Task FixedUpdate()
        {
            await base.FixedUpdate();
            await UpdateMatricesAsync();
        }
    }
}
