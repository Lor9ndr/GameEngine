using GameEngine.Enums;
using GameEngine.RenderPrepearings;
using GameEngine.Structs;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Runtime.InteropServices;

namespace GameEngine
{
    public class ObjectSetupper
    {
        private SetupLevel _level;
        private Vertex[] _vertices;
        private int[] _indices;
        private VAO _vao;
        private readonly VBO _vbo;
        private EBO _ebo;

        public ObjectSetupper(Vertex[] vertices, int[] indices = null)
        {
            _vertices = vertices;
            _indices = indices;
            _vbo = new VBO();

        }
        public void SetAndBindVAO(SetupLevel levels, VAO vao= default)
        {
            _level = levels;
            if (vao != default)
            {
                _vao = vao;
            }
            else
            {
                _vao = new VAO();
                _vao.Setup(_vertices);
            }
            _vbo.Setup(_vertices);

            if (_indices != null)
            {
                _ebo = new EBO();
                _ebo.Setup(_indices);
            }
            SetupALevel();

            GL.VertexArrayVertexBuffer(GetVAO(), 0, GetVBO(), (IntPtr)null, Vertex.Size);
            GL.BindVertexArray(0);
        }
        private void SetupALevel()
        {
            int lvl = (int)_level;
            if (lvl >= 1)
            {
                GL.EnableVertexAttribArray(0);
                GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, Vertex.Size, Marshal.OffsetOf<Vertex>("Position"));

                if (lvl >= 2)
                {
                    GL.EnableVertexAttribArray(1);
                    GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, Vertex.Size, Marshal.OffsetOf<Vertex>("Normal"));

                    if (lvl >= 3)
                    {
                        GL.EnableVertexAttribArray(2);
                        GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, Vertex.Size, Marshal.OffsetOf<Vertex>("TexCoords"));

                        if (lvl >= 4)
                        {
                            GL.EnableVertexAttribArray(3);
                            GL.VertexAttribPointer(3, 3, VertexAttribPointerType.Float, false, Vertex.Size, Marshal.OffsetOf<Vertex>("Tangent"));

                            if (lvl >= 5)
                            {
                                GL.EnableVertexAttribArray(4);
                                GL.VertexAttribPointer(4, 3, VertexAttribPointerType.Float, false, Vertex.Size, Marshal.OffsetOf<Vertex>("Bitangent"));
                            }
                        }
                    }
                }
            }
        }
        public int GetVAO() => _vao.Vao;
        public VAO GetVAOClass() => _vao;
        public int GetVBO() => _vbo.Vbo;
        public int GetEBO() => _ebo.Ebo;
    }
}
