using Engine.Structs;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
namespace Engine.GLObjects
{
    public class VAO : IDisposable
    {
        public int Vao;
        public static Dictionary<int, int> Vaos = new Dictionary<int, int>();
        private bool _isDisposed = false;

        public void Dispose()
        {
            if (!_isDisposed)
            {
                GL.DeleteVertexArray(Vao);
                _isDisposed = true;
            }
        }

        public void Setup(Vertex[] vertices)
        {
            Game.EngineGL.GenVertexArray(out Vao).BindVAO(Vao);
            /*
                        if (Vaos.ContainsKey(Vao))
                        {
                            Vaos[Vao] ++;
                        }
                        else
                        {
                            Vaos.Add(Vao,1);
                        }*/
        }
    }
}
