using GameEngine.GameObjects;
using GameEngine.GameObjects.Base;
using GameEngine.GameObjects.Lights;
using GameEngine.Intefaces;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine
{
    public class WorldRenderer
    {
        public SkyBox sb;
        public List<IRenderable> GameObjects;
        public List<Light> Lights;

        public WorldRenderer(List<IRenderable> gameObjects)
        {
            GameObjects = gameObjects;
            sb = new SkyBox();
        }
        //TODO: Рефактор кода нужен
        public WorldRenderer(List<IRenderable> gameObjects, List<Light> aLights)
        {
            GameObjects = gameObjects;
            Lights = aLights;
            sb = new SkyBox();
        }
        public void Render(Camera camera, Shader shader)
        {
            sb.Render(camera);
            SetupCamera(camera, shader);
            RenderObjects(camera, shader);
            RenderLights(camera, shader);
            for (int i = 0; i < 6; i++)
            {
                GL.DisableVertexAttribArray(i);
            }
        }

        public void RenderLights(Camera camera, Shader shader, bool drawMesh = false)
        {
            SetupCamera(camera, shader);

            if (Lights.OfType<SpotLight>().Count() > 0)
            {
                Lights.OfType<SpotLight>().FirstOrDefault().Position = camera.Position;
                Lights.OfType<SpotLight>().FirstOrDefault().Direction = camera.Front;
            }
            foreach (var item in Lights)
            {
                item.Render(shader);
                if (drawMesh)
                {
                    if (item.GetType() == typeof(PointLight))
                    {
                        (item as PointLight).DrawMesh();
                    }
                }
            }
        }

        public void RenderObjects(Camera camera, Shader shader)
        {
            shader.SetInt("material.texture_diffuse0", 0);
            shader.SetInt("material.texture_specular0", 1);
            shader.SetFloat("material.shininess", 16.0f);
            foreach (var item in GameObjects)
            {
                item.Render(shader);
            }
        }
        private void SetupCamera(Camera camera, Shader shader)
        {
            shader.SetMatrix4("projection", camera.GetProjectionMatrix());
            shader.SetMatrix4("view", camera.GetViewMatrix());
        }
        public void Update()
        {
            foreach (var item in GameObjects)
            {
                item.Update();
            }
            foreach (var item in Lights)
            {
                item.Update();
            }
        }
    }
}
