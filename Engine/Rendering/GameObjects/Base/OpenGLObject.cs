using Engine.GLObjects;
using Engine.Rendering.GLObjects;
using Engine.Structs;
using GameEngine.Enums;
using OpenTK.Graphics.OpenGL4;
using System;

namespace Engine.GameObjects.Base
{
    public abstract class OpenGLObject : IDisposable
    {
        public ObjectSetupper ObjectSetupper;
        public OpenGLObject(Vertex[] vertices, int[] indices = null)
        {
            ObjectSetupper = new ObjectSetupper(vertices, indices);

        }
        protected virtual void Setup(SetupLevel levels, VAO vao = default)
        {
            ObjectSetupper.SetAndBindVAO(levels, vao);
        }
        protected virtual void Setup(int noInstances)
        {

        }

        public virtual void Draw() => Draw(PrimitiveType.Triangles);
        public virtual void Draw(PrimitiveType type)
        {
            GL.BindVertexArray(ObjectSetupper.GetVAO());
            if (ObjectSetupper.HasIndices)
            {
                GL.DrawElements(type, ObjectSetupper.IndicesCount, DrawElementsType.UnsignedInt,0);
            }
            else
            {
                GL.DrawArrays(type, 0, ObjectSetupper.VerticesCount);
            }
            GL.BindVertexArray(0);

        }
        public virtual void Draw(Camera camera) => Draw();

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
