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
        public static object BindedVAO = 0;
        internal OpenGLObject(Vertex[] vertices, uint[] indices = null)
            => ObjectSetupper = new ObjectSetupper(vertices, indices);

        protected internal virtual void Setup(SetupLevel levels, VAO vao = default)
            => ObjectSetupper.SetAndBindVAO(levels, vao);

        internal virtual void Draw() => Draw(PrimitiveType.Triangles);
        internal virtual void Draw(PrimitiveType type)
        {
            Game.EngineGL.BindVAO(ObjectSetupper.GetVAOClass());
            if (ObjectSetupper.HasIndices)
            {
                Game.EngineGL.BindBuffer(BufferTarget.ElementArrayBuffer, ObjectSetupper.GetEBO());
                Game.EngineGL.DrawElements(type, ObjectSetupper.IndicesCount, DrawElementsType.UnsignedInt, 0);
            }
            else
            {
                Game.EngineGL.DrawArrays(type, 0, ObjectSetupper.VerticesCount);
            }
            Game.EngineGL.BindVAO(0);
        }

        public void Dispose()
        {
            ObjectSetupper.Dispose();
        }
    }
}


// Заготовка для частиц
/* int resultVao = ObjectSetupper.GetVAO();
 if (Vaos.ContainsKey(resultVao))
 {
     Vaos[resultVao] = (Vaos[resultVao].count + 1, false);
 }
 else
 {
     Vaos.Add(resultVao, (1, false));
 }
 Console.WriteLine($"VAO: {resultVao}, COUNT: {Vaos[resultVao].count}");*/
/* if (!Vaos[vao].isRendered)
               {
                   GL.BindBuffer(BufferTarget.ElementArrayBuffer, ObjectSetupper.GetEBO());
                   GL.DrawElementsInstanced(type, ObjectSetupper.IndicesCount, DrawElementsType.UnsignedInt, (IntPtr)null, Vaos[vao].count);
                   Vaos[vao] = (Vaos[vao].count, !Vaos[vao].isRendered);
                   GL.BindBuffer(BufferTarget.ElementArrayBuffer,0);
               }*/