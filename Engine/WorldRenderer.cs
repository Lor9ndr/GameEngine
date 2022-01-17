using Engine.Animations;
using Engine.Components;
using Engine.GameObjects;
using Engine.GameObjects.Lights;
using Engine.GLObjects.FrameBuffers;
using Engine.Rendering.Enums;
using Engine.Rendering.GameObjects;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Engine
{
    /// <summary>
    /// Класс отрисовки мира
    /// </summary>
    public class WorldRenderer : IDisposable
    {
        /// <summary>
        /// Наше небо и пол, которые почти недостижимы) 
        /// </summary>
        public SkyBox SkyBox;
        /// <summary>
        /// Список основных объектов для рендера
        /// </summary>
        public List<GameObject> GameObjects;

        /// <summary>
        /// Список всех источников свет
        /// </summary>
        public List<Light> Lights;

        /// <summary>
        /// Шейдер, для теней <see cref="PointLight"/>
        /// </summary>
        public Shader DepthCubeShader
            = new Shader(
                $"{Game.SHADOW_SHADERS_PATH}DepthCubeMap.vert",
                $"{Game.SHADOW_SHADERS_PATH}DepthCubeMap.frag",
                $"{Game.SHADOW_SHADERS_PATH}DepthCubeMap.geom");

        /// <summary>
        /// Шейдер, для теней <see cref="SpotLight"/> и <seealso cref="DirectLight"/>
        /// </summary>
        public Shader DepthDirectShader
            = new Shader($"{Game.SHADOW_SHADERS_PATH}Depth.vert",
                         $"{Game.SHADOW_SHADERS_PATH}Depth.frag");

        /// <summary>
        /// Основной шейдер для основной отрисовки моделей и света
        /// </summary>
        public Shader LightShader
            = new Shader(
                $"{Game.SHADERS_PATH}SimpleShader.vert",
                $"{Game.SHADERS_PATH}SimpleShader.frag",
                $"{Game.SHADERS_PATH}SimpleShader.geom");


        /// <summary>
        /// Основной буффер кадра, куда будут поступать тени, и другие объекты,этот буффер мы выводим на экран
        /// </summary>
        public FrameBuffer DefaultFBO;

        /// <summary>
        /// Список анимаций, сейчас скорее всего он не нужен
        /// </summary>
        public Dictionary<string, Animation> Animations = new Dictionary<string, Animation>();

        /// <summary>
        /// Основной конструктор отрисовщика мира
        /// </summary>
        /// <param name="gameObjects">Cобственно все модели, которые мы будем рендерить</param>
        /// <param name="aLights">Все источники света</param>
        /// <param name="window">Текущее окно</param>
        public WorldRenderer(List<GameObject> gameObjects, List<Light> aLights, GameWindow window)
        {
            GameObjects = gameObjects;
            Lights = aLights;
            SkyBox = new SkyBox();
            DefaultFBO = new FrameBuffer(new Vector2i(Game.Width, Game.Height), ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            window.Resize += Window_Resize;
            //Game.ChangeShadowsSize += Game_ChangeShadowsSize;
        }

        private void Game_ChangeShadowsSize(Vector2i shadows)
        {
            LightShader = new Shader(Game.SHADERS_PATH + "SimpleShader.vert", Game.SHADERS_PATH + "SimpleShader.frag");/*, Game.SHADERS_PATH + "SimpleShader.geom"*/
        }

        private void Window_Resize(OpenTK.Windowing.Common.ResizeEventArgs obj)
        {
            DefaultFBO.Size = obj.Size;
            DefaultFBO.SetViewPort();
        }
        /// <summary>
        /// Основной метод, который уже получает тени и выдает их в основной буффер
        /// </summary>
        /// <param name="camera">Основная камера</param>
        /// <param name="lightFlags">Флаги рендера света</param>
        /// <param name="objFlags">Флаги рендера объектов</param>
        public void Render(Camera camera, RenderFlags lightFlags, RenderFlags objFlags)
        {
            var plcounter = Lights.Count(s => s.GetType() == typeof(PointLight));
            Game.EngineGL.UseShader(LightShader)
                .SetShaderData("nrSpotLights", Lights.Count(s => s.GetType() == typeof(SpotLight)))
                .SetShaderData("nrPointLights", plcounter);

            if (Game.Shadows)
            {
                Game.EngineGL.Clear(ClearBufferMask.DepthBufferBit)
                    .ColorMask(false, false, false, false)
                    .Enable(EnableCap.CullFace)
                    .CullFace(CullFaceMode.Front);
                // Render a shadow framebuffers
                SetupCamera(camera, DepthDirectShader);
                SetupCamera(camera, DepthCubeShader);
                for (int i = 0; i < Lights.Count; i++)
                {
                    var light = Lights[i];
                    var type = light.GetType();
                    if (type == typeof(SpotLight) || type == typeof(DirectLight))
                    {
                        RenderDirectAndSpotLightShadow(light);
                    }
                    else
                    {
                        RenderPointLightShadows((PointLight)light);
                    }
                }
            }
            // Render scene as normal
            DefaultFBO.Activate();
            Game.EngineGL.Enable(EnableCap.DepthTest)
                .Enable(EnableCap.CullFace)
                .CullFace(CullFaceMode.Back)
                .ColorMask(true, true, true, true);
            RenderAll(camera, LightShader, lightFlags, objFlags);
            ShadowData.SetTextureIdDefault();
        }

        /// <summary>
        /// Рендер только света
        /// </summary>
        /// <param name="camera">Активная камера</param>
        /// <param name="shader">на какой шейдер пишем</param>
        /// <param name="flags">Флаги рендера света</param>
        /// <param name="setupCamera">нужно ли в этот шейдер отправлять модель камеры, то есть view * projection</param>
        public void RenderLights(Camera camera, Shader shader, RenderFlags flags, bool setupCamera = true)
        {
            if (setupCamera)
            {
                SetupCamera(camera, shader);
            }
            foreach (var item in Lights)
            {
                item.Render(shader, flags);
            }
        }

        /// <summary>
        /// Метод рендера всех объектов
        /// </summary>
        /// <param name="shader">На какой шейдер пишем</param>
        /// <param name="objFlags">Флаги рендера моделей
        /// Например, при рендере теней,нам нет смысла рисовать текстуры,
        /// поэтому ставим тольк <see cref="RenderFlags.MeshAndAnimations"/>
        /// В таком случае мы будем экономить на отрисовке текстур</param>
        public void RenderObjects(Shader shader, RenderFlags objFlags)
        {
            if (objFlags.HasFlag(RenderFlags.Textures))
            {
                Game.EngineGL.SetShaderData("material.shininess", 32.0f);
            }
            foreach (var item in GameObjects)
            {
                item.Render(shader, objFlags);
            }
        }

        /// <summary>
        /// Просто отрисовка всех объектов
        /// </summary>
        /// <param name="camera">Активная камера</param>
        /// <param name="shader">На какой шейдер пишем</param>
        /// <param name="lightFlags">Флаги рендера света</param>
        /// <param name="objFlags">Флаги рендера объектов</param>
        public void RenderAll(Camera camera, Shader shader, RenderFlags lightFlags, RenderFlags objFlags)
        {
            SkyBox.Render(camera, RenderFlags.MeshAndTextures);
            SetupCamera(camera, shader);
            RenderObjects(shader, objFlags);
            RenderLights(camera, shader, lightFlags, false);
        }

        /// <summary>
        /// Рисуем тени для <see cref="SpotLight"/> и <seealso cref="DirectLight"/>, тк там шейдер не отличается от <see cref="PointLight"/>
        /// то можно в одном вызове 
        /// </summary>
        /// <param name="light">От какого источника света тень</param>
        private void RenderDirectAndSpotLightShadow(Light light)
        {
            light.ShadowData.Shadow.Activate();
            DepthDirectShader.Use();
            DepthDirectShader.SetMatrix4("lightSpaceMatrix", light.LightSpaceMatrix);
            DepthDirectShader.SetMatrix4("model", light.Transform.Model);
            RenderObjects(DepthDirectShader, RenderFlags.MeshAndAnimations);
            light.ShadowData.Shadow.Unbind();
        }

        /// <summary>
        /// Рисуем тени для <see cref="PointLight"/>
        /// </summary>
        /// <param name="light">От какого источника света тень</param>
        public void RenderPointLightShadows(PointLight light)
        {
            light.ShadowData.Shadow.Activate();

            Game.EngineGL.UseShader(DepthCubeShader);
            // light space matrices
            for (int i = 0; i < 6; i++)
            {
                Game.EngineGL.SetShaderData($"shadowMatrices[{i}]", light.LightSpaceMatrices[i]);
            }
            Game.EngineGL.SetShaderData("lightPos", light.Transform.Position)
                .SetShaderData("far_plane", light.FarPlane)
                .SetShaderData("model", light.Transform.Model);

            RenderObjects(DepthCubeShader, RenderFlags.MeshAndAnimations);
            light.ShadowData.Shadow.Unbind();
        }
        public async Task FixedUpdate()
        {
            for (int i = 0; i < GameObjects.Count; i++)
            {
                await GameObjects[i].FixedUpdate();
            }
            for (int i = 0; i < Lights.Count; i++)
            {
                await Lights[i].FixedUpdate();
            }
        }
        public void Update(float dt)
        {
            foreach (var item in GameObjects)
            {
                item.Update(dt);
            }
            foreach (var item in Lights)
            {
                item.Update(dt);
            }
        }

        public static void SetupCamera(Camera camera, Shader shader)
        {
            var view = camera.GetViewMatrix();
            var projection = camera.GetProjectionMatrix();
            Game.EngineGL.UseShader(shader)
                .SetShaderData("VP", view * projection)
                .SetShaderData("projection", projection)
                .SetShaderData("view", view)
                .SetShaderData("viewPos", camera.Transform.Position);
        }

        public void Dispose()
        {
            SkyBox.Dispose();
            for (int i = 0; i < GameObjects.Count; i++)
            {
                GameObjects[i].Dispose();
            }
            DepthDirectShader.Dispose();
            DepthCubeShader.Dispose();
            LightShader.Dispose();
        }
    }
}
