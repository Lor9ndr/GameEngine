using Engine.Rendering.Enums;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using PixelFormat = OpenTK.Graphics.OpenGL4.PixelFormat;

namespace Engine.GLObjects.Textures
{
    /// <summary>
    /// Класс текстур
    /// </summary>
    public class Texture : IDisposable
    {
        /// <summary>
        /// Идентификатор текстуры генерируемый OpenGL'ом
        /// </summary>
        public readonly int Id;
        
        /// <summary>
        /// Тип текстуры
        /// </summary>
        public readonly TextureType Type;
        
        /// <summary>
        /// Путь текстуры
        /// </summary>
        public readonly string Path;

        /// <summary>
        /// Основной конструктор тектсуры
        /// </summary>
        /// <param name="id">Идентификатор текстуры</param>
        /// <param name="type">Тип текстуры</param>
        /// <param name="path">Путь текстуры</param>
        public Texture(int id, TextureType type, string path)
            : this(id, type) => Path = path;

        /// <summary>
        /// Базовый конструктор текстуры, создаваемый только с помощью идентификтора текстуры
        /// </summary>
        /// <param name="id">Идентификтор текстуры</param>
        public Texture(int id)
            => Id = id;

        /// <summary>
        /// Базовый конструктор текстуры, создаваемый только с помощью идентификтора и типом текстуры
        /// </summary>
        /// <param name="id">Идентификтор текстуры</param>
        /// <param name="type">Тип текстуры</param>
        public Texture(int id, TextureType type)
            : this(id) => Type = type;

        /// <summary>
        /// Конструктор текстуры, где сразу определяется идентификатор текстуры
        /// </summary>
        public Texture() => Game.EngineGL.GenTexture(out Id);

        /// <summary>
        /// Загузка и создание текстуры из файла
        /// </summary>
        /// <param name="path">Путь к текстуре</param>
        /// <param name="type">Тип текстуры</param>
        /// <param name="directory">Директория текстуры</param>
        /// <returns></returns>
        public static Texture LoadFromFile(string path, TextureType type, string directory)
        {
            if (!string.IsNullOrEmpty(directory))
            {
                path = directory + '/' + path;
            }
            // Generate handle
            Game.EngineGL.GenTexture(out int handle).BindTexture(TextureTarget.Texture2D, handle);
            // Load the image
            try
            {
                using (Bitmap image = new(path))
                {
                    var data = image.LockBits(
                        new Rectangle(0, 0, image.Width, image.Height),
                        ImageLockMode.ReadOnly,
                        System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                    Game.EngineGL.TexImage2D(TextureTarget.Texture2D,
                        0,
                        PixelInternalFormat.SrgbAlpha,
                        image.Width,
                        image.Height,
                        0,
                        PixelFormat.Bgra,
                        PixelType.UnsignedByte,
                        data.Scan0);
                }


                Game.EngineGL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear)
                            .TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear)
                            .TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat)
                            .TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat)
                            .GenerateMipmap(GenerateMipmapTarget.Texture2D);

                return new Texture(handle, type, path);
            }
            catch (ArgumentException)
            {
                return GetDefaultTexture;
            }

        }

        /// <summary>
        /// Привязка текстуры к следующему рендерируемому объекту
        /// </summary>
        /// <param name="target">Целевой тип текстуры</param>
        public virtual void Bind(TextureTarget target)
            => Game.EngineGL.BindTexture(target, Id);

        /// <summary>
        /// Установка параметров текстуры
        /// </summary>
        /// <param name="size">Размер</param>
        /// <param name="format">Формат пикселей</param>
        /// <param name="type">тип пикселей</param>
        public virtual void SetTexParameters(Vector2i size, PixelInternalFormat format, PixelType type)
            => SetTexParameters(size, format, (PixelFormat)format, type);

        /// <summary>
        /// Установка параметров текстуры
        /// </summary>
        /// <param name="size">Размер</param>
        /// <param name="format">Формат пикселей</param>
        /// <param name="type">тип пикселей</param>
        /// <param name="anotherFormat">Формат пикселей</param>
        public virtual void SetTexParameters(Vector2i size, PixelInternalFormat format, PixelFormat anotherFormat, PixelType type)
        {
            Bind(TextureTarget.Texture2D);
            float[] borderColor = { 1.0f, 1.0f, 1.0f, 1.0f };
            Game.EngineGL.TexImage2D(TextureTarget.Texture2D, 0, format, size.X, size.Y, 0, anotherFormat, type, (IntPtr)null)
                        .TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear)
                        .TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear)
                        .TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToBorder)
                        .TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToBorder)
                        .GenerateMipmap(GenerateMipmapTarget.Texture2D)
                        .TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBorderColor, borderColor);
        }

        /// <summary>
        /// Приатачить текстуру к юниту и забиндить её 
        /// </summary>
        /// <param name="unit">На какой юнит коннектить текстуру</param>
        public void Use(TextureUnit unit)
            => Game.EngineGL.ActiveTexture(unit).BindTexture(TextureTarget.Texture2D, Id);

        /// <summary>
        /// Приатачить текстуру к юниту и забиндить её 
        /// </summary>
        /// <param name="unit">На какой юнит коннектить текстуру</param>
        /// <param name="taget">Целевой тип текстуры</param>
        public void Use(TextureUnit unit, TextureTarget taget)
            => Game.EngineGL.ActiveTexture(unit).BindTexture(taget, Id);

        /// <summary>
        /// Отвязка текстуры
        /// </summary>
        /// <param name="target">От какой цели отвзяываем</param>
        public void Unbind(TextureTarget target)
            => Game.EngineGL.BindTexture(target, 0);

        /// <summary>
        /// Обычная diffuse текстура
        /// </summary>
        public static Texture GetDefaultTexture = LoadFromFile(Game.TEXTURES_PATH + "/DefaultBase.jpg", TextureType.Diffuse, string.Empty);
        
        /// <summary>
        /// Пустая текстура нормалей
        /// </summary>
        public static Texture GetDefaultNormalTexture = LoadFromFile(Game.TEXTURES_PATH + "/EmptyNormal.png", TextureType.Normal, string.Empty);
        
        /// <summary>
        /// Список текстуры цвета и нормалей
        /// </summary>
        public static List<Texture> GetDefaultTextures = new List<Texture>() { GetDefaultTexture, GetDefaultNormalTexture };

        /// <summary>
        /// Освобождение памяти
        /// </summary>
        public void Dispose()
        {
            GL.DeleteTexture(Id);
        }
    }
   
}
