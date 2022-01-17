using Engine.GLObjects;
using Engine.Structs;
using GameEngine.Enums;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Runtime.InteropServices;

namespace Engine.Rendering.GLObjects
{
    public class ObjectSetupper : IDisposable
    {
        protected SetupLevel _level;
        public Vertex[] Vertices;
        protected uint[] _indices;
        protected VAO _vao;
        protected VBO _vbo;
        protected EBO _ebo;
        private bool _isDisposed = false;
        public ObjectSetupper(Vertex[] vertices, uint[] indices = default)
        {
            Vertices = vertices;
            _indices = indices;
            _vbo = new VBO();

        }
        public virtual void SetAndBindVAO(SetupLevel levels, VAO vao = default)
        {
            _level = levels;
            if (vao != default)
            {
                _vao = vao;
            }
            else
            {
                _vao = new VAO();
                _vao.Setup(Vertices);
            }
            _vbo.Setup(Vertices);

            if (_indices != null)
            {
                _ebo = new EBO();
                _ebo.Setup(_indices);
            }
            SetupALevel();

            Game.EngineGL
                .BindBuffer(BufferTarget.ArrayBuffer, 0)
                .BindVAO();
        }

        public void SetupALevel()
        {
            int lvl = (int)_level;
            Game.EngineGL.BindVAO(_vao);
            if (lvl >= 1)
            {
                GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, Vertex.Size, Marshal.OffsetOf<Vertex>("Position"));
                GL.EnableVertexAttribArray(0);
                if (lvl >= 2)
                {
                    GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, Vertex.Size, Marshal.OffsetOf<Vertex>("Normal"));
                    GL.EnableVertexAttribArray(1);
                    if (lvl >= 3)
                    {
                        GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, Vertex.Size, Marshal.OffsetOf<Vertex>("TexCoords"));
                        GL.EnableVertexAttribArray(2);
                        if (lvl >= 4)
                        {
                            GL.VertexAttribPointer(3, 3, VertexAttribPointerType.Float, false, Vertex.Size, Marshal.OffsetOf<Vertex>("Tangent"));
                            GL.EnableVertexAttribArray(3);
                            if (lvl >= 5)
                            {
                                GL.VertexAttribPointer(4, 3, VertexAttribPointerType.Float, false, Vertex.Size, Marshal.OffsetOf<Vertex>("Bitangent"));
                                GL.EnableVertexAttribArray(4);
                                if (lvl >= 6)
                                {
                                    GL.VertexAttribIPointer(5, 4, VertexAttribIntegerType.Int, Vertex.Size, Marshal.OffsetOf<Vertex>("BoneIDs"));
                                    GL.EnableVertexAttribArray(5);

                                    GL.VertexAttribPointer(6, 4, VertexAttribPointerType.Float, true, Vertex.Size, Marshal.OffsetOf<Vertex>("Weights"));
                                    GL.EnableVertexAttribArray(6);
                                }
                            }
                        }
                    }
                }
            }
        }

        public int IndicesCount
        {
            get
            {
                return _indices is not null ? _indices.Length : 0;
            }
        }
        public int VerticesCount => Vertices.Length;
        public bool HasIndices => _indices != null && _indices.Length > 0;
        public int GetVAO() => _vao.Vao;
        public VAO GetVAOClass() => _vao;
        public int GetVBO() => _vbo.Vbo;
        public int GetEBO() => _ebo.Ebo;

        public void Dispose()
        {
            if (!_isDisposed)
            {
                _vao.Dispose();
                _vbo.Dispose();
                _ebo?.Dispose();
                _isDisposed = true;
            }
        }

        public Vertex[] GetVertices() => Vertices;
        public uint[] GetIndices() => _indices;
    }
}
