using Engine.GameObjects.Base;
using Engine.GLObjects;
using Engine.GLObjects.Textures;
using Engine.Rendering.Enums;
using Engine.Structs;
using GameEngine.Enums;
using OpenTK.Graphics.OpenGL4;
using System.Collections.Generic;
using System.Linq;

namespace Engine.Rendering.GameObjects
{
    /// <summary>
    /// Меш является частью модели 
    /// </summary>
    public class Mesh : OpenGLObject
    {
        /// <summary>
        /// Список используемых текстур
        /// </summary>
        public List<Texture> Textures = new List<Texture>();

        /// <summary>
        /// Основной конструктор меша
        /// </summary>
        /// <param name="vertices">Вершины меша</param>
        /// <param name="indices">Индексы вершин, если есть таковые</param>
        /// <param name="textures">Список текстур, если есть таковые</param>
        /// <param name="vao">Используем такой же объект массива вершин, если уже создан такой же объект</param>
        /// <param name="setupLevel">Уровень установки меша, только позиции вершин, вершины и координаты текстур и тд.</param>
        public Mesh(Vertex[] vertices, uint[] indices = null, List<Texture> textures = null, VAO vao = null, SetupLevel setupLevel = SetupLevel.Animation)
            : base(vertices, indices)
        {
            Textures = textures ?? new List<Texture>();
            if (Textures.Count(s => s.Type == TextureType.Normal) == 0 && Textures.Count(s => s.Type == TextureType.Skybox) == 0)
            {
                Textures.Add(Texture.GetDefaultNormalTexture);
            }
            Setup(setupLevel, vao);
        }


        /// <summary>
        /// Отрисовка меша в текущий активный шейдер
        /// </summary>
        /// <param name="shader">Собственно сам шейдер</param>
        /// <param name="flags">Флаги,что нужно рендерить,а что нет</param>
        /// <param name="target">Куда накладывать текстуры</param>
        /// <param name="type">Тип отрисовки</param>
        public void Render(Shader shader, RenderFlags flags, TextureTarget target = TextureTarget.Texture2D, PrimitiveType type = PrimitiveType.Triangles)
        {
            Game.EngineGL.UseShader(shader);
            if (flags.HasFlag(RenderFlags.Textures))
            {
                for (int i = 0; i < Textures?.Count; i++)
                {
                    string name = Textures[i].Type.ToString();
                    Textures[i].Use(TextureUnit.Texture0 + i, target);

                    Game.EngineGL
                        .SetShaderData($"material.{name}", i)
                        .SetShaderData(name, i);
                }
            }
            base.Draw(type);
            Game.EngineGL.BindTexture(target, 0);
        }

        /// <summary>
        /// Освобождение памяти, не уверен,что прям сильно нужно,
        /// тк компилятор и сам хорошо это делает
        /// </summary>
        public new void Dispose()
        {
            foreach (var item in Textures)
            {
                item.Dispose();
            }
            base.Dispose();
        }
    }
}
