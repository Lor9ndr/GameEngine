using Engine.Components;
using Engine.Extensions;
using Engine.GameObjects;
using Engine.GameObjects.Lights;
using Engine.GLObjects.Textures;
using Engine.Rendering.DefaultMeshes;
using Engine.Rendering.Enums;
using OpenTK.Mathematics;
using System;

namespace Engine.Rendering.Factories
{
    /// <summary>
    /// Фабрика создания света
    /// </summary>
    public static class LightFactory
    {
        private static readonly Random _rd = new Random();

        #region Default Data
        private static readonly Texture _lightTexture = Texture.LoadFromFile(Game.TEXTURES_PATH + "/blub.png", TextureType.Diffuse, string.Empty);
        private static readonly Texture _spotLightTexture = Texture.LoadFromFile(Game.TEXTURES_PATH + "/SpotLightIcon.png", TextureType.Diffuse, string.Empty);
        private static readonly Model _cube = DefaultMesh.GetTexturedCube(_lightTexture);
        private static readonly Model _spotLightCube = DefaultMesh.GetTexturedCube(_spotLightTexture);

        private static LightData DirectData
            => new LightData(ambient: new Vector3(0.6f),
                            diffuse: new Vector3(1f),
                            color: new Vector3(1f),
                            specular: new Vector3(0.7f),
                            intensity: 1.0f);

        private static LightData PointData
            => new LightData(ambient: new Vector3(0.25f),
                            diffuse: new Vector3(1f),
                            color: new Vector3(1),
                            specular: new Vector3(0.3f),
                            intensity: 200.0f);

        private static LightData PointRandomColorData
            => new LightData(ambient: new Vector3(0.25f),
                            diffuse: new Vector3(1f),
                            color: new Vector3(_rd.NextFloat(0f, 1f), _rd.NextFloat(0f, 1f), _rd.NextFloat(0f, 1f)),
                            specular: new Vector3(0.3f),
                            intensity: 500.0f);

        private static LightData SpotData
            => new LightData(diffuse: new Vector3(1),
                            ambient: new Vector3(1),
                            color: new Vector3(1f),
                            specular: new Vector3(1.0f),
                            intensity: 5.0f);
        #endregion

        #region Getters

        /// <summary>
        /// Получение точечный источник света
        /// </summary>
        /// <param name="position">позиция света</param>
        /// <returns>Возвращает точечный источник света</returns>
        public static Light GetPointLight(Vector3 position)
            => new PointLight(_cube, PointData, new Transform(position));

        /// <summary>
        /// Получение точечногоисточника света с рандомным цветом
        /// </summary>
        /// <param name="position">позиция света</param>
        /// <returns>Возвращает точечный источник света с рандомным цветом</returns>
        public static Light GetRandomColorPointLight(Vector3 position)
            => new PointLight(_cube, PointRandomColorData, new Transform(position));

        /// <summary>
        /// Получение прямого источника света
        /// </summary>
        /// <param name="position">позиция света</param>
        /// <param name="direction">Направление света</param>
        /// <returns>Возвращает прямой источник света</returns>
        public static Light GetDirectLight(Vector3 position, Vector3 direction)
            => new DirectLight(_cube, DirectData, new Transform(position, direction));

        /// <summary>
        /// Получение местного источника света
        /// </summary>
        /// <param name="position">позиция света</param>
        /// <param name="direction">Направление света</param>
        /// <returns>Возвращает местного источник света</returns>
        public static Light GetSpotLight(Vector3 position, Vector3 direction)
            => new SpotLight(SpotData, new Transform(position, direction), model: _spotLightCube);

        #endregion
    }
}
