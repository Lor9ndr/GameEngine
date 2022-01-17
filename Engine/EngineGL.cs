using Engine.GLObjects;
using Engine.GLObjects.FrameBuffers;
using Engine.GLObjects.Textures;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Engine
{
    /// <summary>
    /// Класс для работы с OpenGL
    /// </summary>
    public class EngineGL
    {
        private CullFaceMode _currentCullFaceMode;
        private VAO _bindedVao;
        private bool _depthMask;
        private Shader _activeShader;
        private EngineViewport _activeViewport;
        private MaterialFace _activePolygonFace = MaterialFace.FrontAndBack;


        private PolygonMode activePolygonMode = OpenTK.Graphics.OpenGL4.PolygonMode.Fill;
        private TextureUnit _activeTextureUnit;

        /// <summary>
        /// Вьюпорт
        /// </summary>
        public struct EngineViewport
        {
            /// <summary>
            /// начальная координата X
            /// </summary>
            public int X;

            /// <summary>
            /// начальная координата Y
            /// </summary>
            public int Y;

            /// <summary>
            /// Ширина
            /// </summary>
            public int Width;

            /// <summary>
            /// Высота
            /// </summary>
            public int Height;

            /// <summary>
            /// Оператор сравнения вьюпорта
            /// </summary>
            public static bool operator ==(EngineViewport view1, EngineViewport view2) => view1.Equals(view2);

            /// <summary>
            /// Оператор сравнения вьюпорта
            /// </summary>
            public static bool operator !=(EngineViewport view1, EngineViewport view2) => !view1.Equals(view2);

            /// <summary>
            /// Функция сравнения
            /// </summary>
            public override bool Equals([NotNullWhen(true)] object obj) => GetHashCode() == obj.GetHashCode();

            /// <summary>
            /// Функция, возращающая хэшкод
            /// </summary>
            public override int GetHashCode() => X.GetHashCode() ^ Y.GetHashCode() ^ Width.GetHashCode() ^ Height.GetHashCode();
            /// <summary>
            /// Базовый конструктор
            /// </summary>
            /// <param name="x">Начальная координата X</param>
            /// <param name="y">Начальная координата Y</param>
            /// <param name="width">Ширина</param>
            /// <param name="height">Высота</param>
            public EngineViewport(int x, int y, int width, int height)
            {
                X = x;
                Y = y;
                Width = width;
                Height = height;
            }
        }

        #region Public Properties

        /// <summary>
        /// Отправленные в OpenGL буфферы,
        /// где ключ <see cref="BufferTarget"/> - это целевой буффер 
        /// и значение <see cref="int"/> - индекс буффера
        /// </summary>
        public Dictionary<BufferTarget, int> BindedBuffer = new Dictionary<BufferTarget, int>();

        /// <summary>
        /// Текующий режим отрисовки полигонов
        /// </summary>
        public PolygonMode ActivePolygonMode
        {
            get => activePolygonMode;
            set
            {
                if (activePolygonMode != value)
                {
                    activePolygonMode = value;
                    PolygonMode(ActivePolygonFace, value);
                }
            }
        }

        /// <summary>
        /// Текущая сторона отрисовки полигона
        /// </summary>
        public MaterialFace ActivePolygonFace
        {
            get => _activePolygonFace;
            set
            {
                if (_activePolygonFace != value)
                {
                    _activePolygonFace = value;
                    PolygonMode(value, ActivePolygonMode);
                }
            }
        }

        /// <summary>
        /// Текущий вьюпорт
        /// </summary>
        public EngineViewport ActiveViewport
        {
            get => _activeViewport;
            set
            {
                if (_activeViewport != value)
                {
                    _activeViewport = value;
                    Viewport(value.X, value.Y, value.Width, value.Height);
                }
            }
        }

        /// <summary>
        /// Текущее значение маски глубины
        /// </summary>
        public bool DepthMaskValue
        {
            get => _depthMask;
            set
            {
                if (_depthMask != value)
                {
                    _depthMask = value;
                    DepthMask(value);
                }
            }
        }

        /// <summary>
        /// Текущий режим отсечения граней
        /// </summary>
        public CullFaceMode CurrentCullFaceMode
        {
            get => _currentCullFaceMode;
            set
            {
                if (_currentCullFaceMode != value)
                {
                    _currentCullFaceMode = value;
                    CullFace(value);
                }
            }
        }

        /// <summary>
        /// Текущий объект массива вершин
        /// </summary>
        public VAO BindedVao
        {
            get => _bindedVao;
            set
            {
                if (_bindedVao != value)
                {
                    _bindedVao = value;
                    BindVAO(value);
                }
            }
        }

        /// <summary>
        /// Активный шейдер
        /// </summary>
        public Shader ActiveShader
        {
            get => _activeShader;
            set
            {
                if (_activeShader != value)
                {
                    _activeShader = value;
                    UseShader(value);
                }
            }
        }

        /// <summary>
        /// Текущий юнит текстуры
        /// </summary>
        public TextureUnit ActiveTextureUnit
        {
            get => _activeTextureUnit;
            set
            {
                if (_activeTextureUnit != value)
                {
                    _activeTextureUnit = value;
                    ActiveTexture(value);
                }
            }
        }

        /// <summary>
        /// Отправленный индекс текстуры может быть не равен Texture.Handle
        /// </summary>
        public int BindedTexID { get; private set; }

        /// <summary>
        /// Отправленный класс текстуры, индекс берется из BindedTexture.Handle
        /// </summary>
        public Texture BindedTexture { get; private set; }

        /// <summary>
        /// Текущий буффер кадра
        /// </summary>
        public FrameBuffer CurrentFrameBuffer { get; private set; }
        #endregion

        /// <summary>
        /// Генерируем индентификатор массива вершинного объекта 
        /// </summary>
        /// <param name="vao">возвращаемое значение</param>
        public EngineGL GenVertexArray(out int vao)
        {
            vao = GL.GenVertexArray();
            return this;
        }

        /// <summary>
        /// включить <see cref="EnableCap"/>
        /// </summary>
        /// <param name="cap">Что включаем</param>
        public EngineGL Enable(EnableCap cap)
        {
            GL.Enable(cap);
            return this;
        }

        /// <summary>
        /// Отключаем <see cref="EnableCap"/>
        /// </summary>
        /// <param name="cap">Что отключаем</param>
        public EngineGL Disable(EnableCap cap)
        {
            GL.Disable(cap);
            return this;
        }

        /// <summary>
        /// Маска цвета
        /// </summary>
        /// <param name="r">red</param>
        /// <param name="g">green</param>
        /// <param name="b">blue</param>
        /// <param name="a">alpha</param>
        public EngineGL ColorMask(bool r, bool g, bool b, bool a)
        {
            GL.ColorMask(r, g, b, a);
            return this;
        }

        /// <summary>
        /// Режим отсечения грани
        /// </summary>
        /// <param name="mode">
        /// <code>if (mode == CullFaceMode.Front)</code> - отсекаем передние грани
        /// <code>if (mode == CullFaceMode.Back)</code> - отсекаем задние грани
        /// <code>if (mode == CullFaceMode.FrontAndBack)</code>- отсекаем и то и другое. Но я хз когда это может пригодится
        /// </param>
        public EngineGL CullFace(CullFaceMode mode)
        {

            if (CurrentCullFaceMode != mode)
            {
                GL.CullFace(mode);
                CurrentCullFaceMode = mode;
            }
            return this;
        }

        /// <summary>
        /// Маска глубины
        /// </summary>
        /// <param name="value">включить/выключить</param>
        public EngineGL DepthMask(bool value)
        {
            GL.DepthMask(value);
            DepthMaskValue = value;
            return this;
        }

        /// <summary>
        /// Отправка Объекта массива вершин
        /// </summary>
        /// <param name="vao">класс с индексом объекта массива вершин</param>
        public EngineGL BindVAO(VAO vao)
        {
            if (BindedVao != vao)
            {
                GL.BindVertexArray(vao.Vao);
                BindedVao = vao;
            }
            return this;
        }

        /// <summary>
        /// Отправка Объекта массива вершин
        /// </summary>
        /// <param name="vao">индекс объект массива вершин</param>
        public EngineGL BindVAO(int vao = 0)
        {
            GL.BindVertexArray(vao);
            if (vao == 229)
            {

            }
            BindedVao = null;
            return this;
        }

        /// <summary>
        /// Отправка буффера
        /// </summary>
        /// <param name="target">Целевой буффер</param>
        /// <param name="buffer">индекс буффера</param>
        public EngineGL BindBuffer(BufferTarget target, int buffer)
        {
            if (!BindedBuffer.ContainsKey(target))
            {
                BindedBuffer.Add(target, buffer);
                GL.BindBuffer(target, buffer);
                return this;
            }
            if (BindedBuffer[target] != buffer)
            {
                GL.BindBuffer(target, buffer);
            }
            return this;
        }

        /// <summary>
        /// Отрисовка элементов, когда есть EBO буффер
        /// </summary>
        /// <param name="type">тип отрисовки</param>
        /// <param name="count">количество индексов для вершин</param>
        /// <param name="deType">тип данных индексов</param>
        /// <param name="indices">индексы, если уже забинжен VAO и EBO находится внутри VAO, то стоит указать 0</param>
        public EngineGL DrawElements(PrimitiveType type, int count, DrawElementsType deType, int indices)
        {
            GL.DrawElements(type, count, deType, indices);
            return this;
        }

        /// <summary>
        /// Отрисовка с помощью только VAO объекта
        /// Может отрисовываться неправильно, если изначально объект был с индексами вершин,
        /// а ты собираешься отрисовывать только вершины без индексов,
        /// так как индексы помогают уменьшить кол-во вершин и повторять уже существующие вершины
        /// </summary>
        /// <param name="type">Тип отрисовки</param>
        /// <param name="first">Начиная с этой вершины,отсчет от 0</param>
        /// <param name="count">Кол-во вершин</param>
        public EngineGL DrawArrays(PrimitiveType type, int first, int count)
        {
            GL.DrawArrays(type, first, count);
            return this;
        }

        /// <summary>
        /// Отправка данных в последний активный шейдер
        /// </summary>
        /// <typeparam name="T">Тип отправляемый в шейдер</typeparam>
        /// <param name="name">название uniform в шейдере</param>
        /// <param name="data">собственно сами данные,которые будут отправляться в шейдер</param>
        public EngineGL SetShaderData<T>(string name, T data)
            where T : struct
        {
            if (data is Vector3 data3)
            {
                ActiveShader.SetVector3(name, data3);
            }
            else if (data is Matrix4 dataMat4)
            {
                ActiveShader.SetMatrix4(name, dataMat4);
            }
            else if (data is bool dataBool)
            {
                ActiveShader.SetInt(name, dataBool);
            }
            else if (data is int dataInt)
            {
                ActiveShader.SetInt(name, dataInt);
            }
            else if (data is float dataFloat)
            {
                ActiveShader.SetFloat(name, dataFloat);
            }
            else if (data is Vector2 data2)
            {
                ActiveShader.SetVector2(name, data2);
            }
            return this;
        }

        /// <summary>
        /// Установка цвета
        /// </summary>
        /// <param name="r">red</param>
        /// <param name="g">green</param>
        /// <param name="b">blue</param>
        /// <param name="a">alpha</param>
        public EngineGL ClearColor(float r, float g, float b, float a)
        {
            GL.ClearColor(r, g, b, a);
            return this;
        }

        /// <summary>
        /// Включение дебаггера
        /// </summary>
        public EngineGL DebugMessageCallback(DebugProc proc, IntPtr userPtr)
        {
            GL.DebugMessageCallback(proc, userPtr);
            return this;
        }

        /// <summary>
        /// Очистка маски буффера 
        /// </summary>
        /// <param name="mask">какую маску очищаем </param>
        public EngineGL Clear(ClearBufferMask mask)
        {
            GL.Clear(mask);
            return this;
        }

        /// <summary>
        /// Активирование шейдера,после активации шейдера, 
        /// данные будут отправляться с помощью <see cref="SetShaderData{T}(string, T)"/> именно в этот активный шейдер
        /// </summary>
        /// <param name="shader">Сам шейдер</param>
        public EngineGL UseShader(Shader shader)
        {
            if (ActiveShader != shader)
            {
                shader.Use();
                ActiveShader = shader;
            }
            return this;
        }

        /// <summary>
        /// Установка вьюпорта, идет вроде слева снизу система координат,
        /// то есть слева снизу будет координата (0,0)
        /// </summary>
        /// <param name="x">Начальная координата X</param>
        /// <param name="y">Начальная координата Y</param>
        /// <param name="width">Ширина</param>
        /// <param name="height">Высота</param>
        public EngineGL Viewport(int x, int y, int width, int height)
        {
            var viewport = new EngineViewport(x, y, width, height);
            if (ActiveViewport != viewport)
            {
                GL.Viewport(x, y, width, height);
                ActiveViewport = viewport;
            }
            return this;
        }

        /// <summary>
        /// Установка вьюпорта, идет вроде слева снизу система координат,
        /// то есть слева снизу будет координата (0,0)
        /// </summary>
        /// <param name="viewport">Вьюпорт</param>
        public EngineGL Viewport(EngineViewport viewport)
        {
            if (ActiveViewport != viewport)
            {
                GL.Viewport(viewport.X, viewport.Y, viewport.Width, viewport.Height);
                ActiveViewport = viewport;
            }
            return this;
        }

        /// <summary>
        /// Установка режима отрисовки
        /// </summary>
        /// <param name="face">какую сторону отрисовывать</param>
        /// <param name="mode">в каком режиме отрисовывать</param>
        public EngineGL PolygonMode(MaterialFace face, PolygonMode mode)
        {
            if (activePolygonMode != mode || ActivePolygonFace != face)
            {
                GL.PolygonMode(face, mode);
                ActivePolygonFace = face;
                ActivePolygonMode = mode;
            }
            return this;
        }

        /// <summary>
        /// Активация юнит текстуры
        /// </summary>
        /// <param name="unit">Какой юнит активируем текстуры</param>
        /// <returns></returns>
        public EngineGL ActiveTexture(TextureUnit unit)
        {
            if (unit != ActiveTextureUnit)
            {
                GL.ActiveTexture(unit);
                ActiveTextureUnit = unit;
            }
            return this;
        }

        /// <summary>
        /// Отправка индекса текстуры
        /// </summary>
        /// <param name="target">тип текстуры</param>
        /// <param name="textureID">индекс текстуры</param>
        public EngineGL BindTexture(TextureTarget target, int textureID)
        {
            if (textureID != BindedTexID)
            {
                GL.BindTexture(target, textureID);
                BindedTexID = textureID;
                BindedTexture = null;
            }
            return this;
        }

        /// <summary>
        /// Отправка индекса текстуры
        /// </summary>
        /// <param name="target">тип текстуры</param>
        /// <param name="texture">Отправляемая текстура</param>
        public EngineGL BindTexture(TextureTarget target, Texture texture)
        {
            if (texture != BindedTexture && texture.Id != BindedTexID)
            {
                GL.BindTexture(target, texture.Id);
                BindedTexID = texture.Id;
                BindedTexture = texture;
            }
            return this;
        }

        /// <summary>
        /// Отправка информации в буффер
        /// </summary>
        /// <typeparam name="T">Тип отправляемой информации в буффер </typeparam>
        /// <param name="target">Тип в какой буффер отправляем,должен быть сначала присоединён с помощью <see cref="BindBuffer(BufferTarget, int)"/></param>
        /// <param name="size">Размер отправляемой информации</param>
        /// <param name="data">Сама информация</param>
        /// <param name="hint">Тип как будет использоваться данные. К примеру,
        /// StaticDraw - данные буфера не будут меняться,
        /// можно использовать другие варианты, в будущем</param>
        public EngineGL BufferData<T>(BufferTarget target, int size, T[] data, BufferUsageHint hint) where T : struct
        {
            GL.BufferData(target, size, data, hint);
            return this;
        }

        /// <summary>
        /// Отправка информации в буффер
        /// </summary>
        /// <param name="target">Тип в какой буффер отправляем,должен быть сначала присоединён с помощью <see cref="BindBuffer(BufferTarget, int)"/></param>
        /// <param name="size">Размер отправляемой информации</param>
        /// <param name="data">Сама информация</param>
        /// <param name="hint">Тип как будет использоваться данные. К примеру,
        /// StaticDraw - данные буфера не будут меняться,
        /// можно использовать другие варианты, в будущем</param>
        public EngineGL BufferData(BufferTarget target, int size, IntPtr data, BufferUsageHint hint)
        {
            GL.BufferData(target, size, data, hint);
            return this;
        }

        /// <summary>
        /// Отправка информации в буффер
        /// </summary>
        /// <typeparam name="T">Тип отправляемой информации в буффер </typeparam>
        /// <param name="target">Тип в какой буффер отправляем,должен быть сначала присоединён с помощью <see cref="BindBuffer(BufferTarget, int)"/></param>
        /// <param name="size">Размер отправляемой информации</param>
        /// <param name="data">Сама информация</param>
        /// <param name="hint">Тип как будет использоваться данные. К примеру,
        /// StaticDraw - данные буфера не будут меняться,
        /// можно использовать другие варианты, в будущем</param>
        public EngineGL BufferData<T>(BufferTarget target, IntPtr size, T[] data, BufferUsageHint hint) where T : struct
        {
            GL.BufferData(target, size, data, hint);
            return this;
        }

        /// <summary>
        /// Присоединение буффера кадра
        /// </summary>
        /// <param name="target">целевой тип буффера кадра</param>
        /// <param name="frameBuffer">Сам буффер кадра</param>
        /// <returns></returns>
        public EngineGL BindFramebuffer(FramebufferTarget target, FrameBuffer frameBuffer)
        {
            if (frameBuffer != CurrentFrameBuffer)
            {
                GL.BindFramebuffer(target, frameBuffer.FBO);
                CurrentFrameBuffer = frameBuffer;
            }
            return this;
        }

        /// <summary>
        /// Установка параметров текстуры
        /// </summary>
        /// <param name="target">тип целевой текстуры</param>
        /// <param name="paramName">Название параметра</param>
        /// <param name="param">Сам параметр</param>
        public EngineGL TexParameter(TextureTarget target, TextureParameterName paramName, int param)
        {
            GL.TexParameter(target, paramName, param);
            return this;
        }

        /// <summary>
        /// Установка параметров текстуры
        /// </summary>
        /// <param name="target">тип целевой текстуры</param>
        /// <param name="paramName">Название параметра</param>
        /// <param name="param">Параметры</param>
        public EngineGL TexParameter(TextureTarget target, TextureParameterName paramName, float[] param)
        {
            GL.TexParameter(target, paramName, param);
            return this;
        }

        /// <summary>
        /// Создание 2D текстуры 
        /// </summary>
        /// <param name="textureTarget">Тип целевой текстуры</param>
        /// <param name="level">видимо слой</param>
        /// <param name="pixelInternalFormat">Формат пикселей</param>
        /// <param name="width">ширина </param>
        /// <param name="height">высота</param>
        /// <param name="border">что-то с границами связано</param>
        /// <param name="pixelFormat">еще формат пикселей</param>
        /// <param name="pixelType">Тип пикселей</param>
        /// <param name="pixels">ссылка на загруженную текстуру</param>
        public EngineGL TexImage2D(TextureTarget textureTarget, int level, PixelInternalFormat pixelInternalFormat, int width, int height, int border, PixelFormat pixelFormat, PixelType pixelType, IntPtr pixels)
        {
            GL.TexImage2D(textureTarget, level, pixelInternalFormat, width, height, border, pixelFormat, pixelType, pixels);
            return this;
        }

        /// <summary>
        /// Создаем объект текстуры
        /// </summary>
        /// <param name="tex">на выход подается переменная, которая будет хранить индекс текстуры</param>
        /// <returns></returns>
        public EngineGL GenTexture(out int tex)
        {
            tex = GL.GenTexture();
            return this;
        }

        /// <summary>
        /// Генерируем буффер
        /// </summary>
        /// <param name="vbo">возращается индекс буффера</param>
        public EngineGL GenBuffer(out int vbo)
        {
            vbo = GL.GenBuffer();
            return this;
        }

        /// <summary>
        /// Генерируем буффер кадра
        /// </summary>
        /// <param name="fbo">возращается индекс буффера када</param>
        public EngineGL GenFramebuffer(out int fbo)
        {
            fbo = GL.GenFramebuffer();
            return this;
        }

        /// <summary>
        /// Генерация мипмапа,чтобы при отдалении текстура уменьшалась, а при увеличении увеличивалась, сложно объяснить
        /// </summary>
        /// <param name="target"></param>
        public EngineGL GenerateMipmap(GenerateMipmapTarget target)
        {
            GL.GenerateMipmap(target);
            return this;
        }

        /// <summary>
        /// Установка текстуры буффера кадра 2D
        /// </summary>
        /// <param name="framebufferTarget">на какой буффер накладываем текстуру</param>
        /// <param name="framebufferAttachment">привязка буффера кадра</param>
        /// <param name="textureTarget">целевой тип текстуры</param>
        /// <param name="texture">Текстура</param>
        /// <param name="level">Уровень наложения текстуры</param>
        public EngineGL FramebufferTexture2D(FramebufferTarget framebufferTarget, FramebufferAttachment framebufferAttachment, TextureTarget textureTarget, Texture texture, int level)
        {
            GL.FramebufferTexture2D(framebufferTarget, framebufferAttachment, textureTarget, texture.Id, level);
            return this;
        }

        /// <summary>
        /// Установка текстуры буффера кадра
        /// </summary>
        /// <param name="framebufferTarget">на какой буффер накладываем текстуру</param>
        /// <param name="framebufferAttachment">привязка буффера кадра</param>
        /// <param name="texture">Текстура</param>
        /// <param name="level">Уровень наложения текстуры</param>
        public EngineGL FramebufferTexture(FramebufferTarget framebufferTarget, FramebufferAttachment framebufferAttachment, Texture texture, int level)
        {
            GL.FramebufferTexture(framebufferTarget, framebufferAttachment, texture.Id, level);
            return this;
        }
    }
}
