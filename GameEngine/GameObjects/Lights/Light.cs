using GameEngine.Attribute;
using GameEngine.Bases;
using GameEngine.Bases.Components;
using GameEngine.Extensions;
using GameEngine.GameObjects.Base;
using GameEngine.RenderPrepearings;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine.GameObjects.Lights
{
    public abstract class Light : AMovable
    {
        private LightData _lightData;
        private ShadowData _shadowData;
        private Mesh _mesh;
        public float NearPlane { get => _nearPlane; set => _nearPlane = value; }

        public float FarPlane { get => _farPlane; set => _farPlane = value; }

        private float _farPlane;
        private float _nearPlane;

        private Matrix4 _lightSpaceMatrix;

        internal Mesh Mesh { get => _mesh; set => _mesh = value; }
        public Matrix4 LightSpaceMatrix { get => _lightSpaceMatrix; set => _lightSpaceMatrix = value; }

        public LightData LightData { get => _lightData; set => _lightData = value; }
        public ShadowData ShadowData { get => _shadowData; set => _shadowData = value; }

        public Light(Mesh mesh, LightData lightData, Transform transform)
            : base(transform)
        {
            Mesh = mesh;
            LightData = lightData;
            ShadowData = new ShadowData();
            AddComponent(LightData);
            AddComponent(ShadowData);

        }
        public virtual void SetupModel(Shader shader)
        {
            var t2 = Matrix4.CreateTranslation(Transform.Position);
            var r1 = Matrix4.CreateRotationX(Transform.Rotation.X);
            var r2 = Matrix4.CreateRotationY(Transform.Rotation.Y);
            var r3 = Matrix4.CreateRotationZ(Transform.Rotation.Z);
            var s = Matrix4.CreateScale(Transform.Scale);
            Transform.Model = r1.Multiply(r2).Multiply(r3).Multiply(s).Multiply(t2);
            shader.SetMatrix4("model", Transform.Model);
        }
        public virtual void Render(Shader shader, RenderFlags flags, int textureidx = 0)
        {
            shader.Use();
            SetupModel(shader);
            if (flags.HasFlag(RenderFlags.Mesh))
            {
                DrawMesh(shader);
            }
        }
        public virtual void DrawMesh(Shader shader)
        {
            shader.SetInt("reverse_normals", 1);
            Mesh.Draw();
            shader.SetInt("reverse_normals", 0);
        }

        public virtual Matrix4 GetProjection => throw new NotImplementedException();


        public virtual void UpdateMatrices() => throw new NotImplementedException();
        public void SetAmbient(Vector3 ambient)
        {
            LightData.PlusAmbient(ambient);
        }
    }
}
