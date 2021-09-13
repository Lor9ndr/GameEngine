using GameEngine.Enums;
using GameEngine.RenderPrepearings;
using GameEngine.Structs;
using OpenTK.Graphics.OpenGL4;
using System;

namespace GameEngine.GameObjects.Base
{
    public abstract class AGameObject : IDisposable
    {
        public ObjectSetupper ObjectSetupper;
        public AGameObject(Vertex[] vertices, int[] indices = null)
        {
            ObjectSetupper = new ObjectSetupper(vertices, indices);

        }
        protected virtual void Setup(SetupLevel levels, VAO vao = default)
        {
            ObjectSetupper.SetAndBindVAO(levels, vao);
        }

        public virtual void Draw() => Draw(PrimitiveType.Triangles);
        public virtual void Draw(PrimitiveType type)
        {
            GL.BindVertexArray(ObjectSetupper.GetVAO());
            GL.DrawArrays(type, 0, ObjectSetupper.VerticesCount);
            GL.BindVertexArray(0);

        }
        public virtual void Draw(Camera camera) => Draw();

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
