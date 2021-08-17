﻿using GameEngine.GameObjects;
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
        public SkyBox SkyBox;
        public List<IRenderable> GameObjects;
        public List<Light> Lights;

        public WorldRenderer(List<IRenderable> gameObjects)
        {
            GameObjects = gameObjects;
            SkyBox = new SkyBox();
        }
        public WorldRenderer(List<IRenderable> gameObjects, List<Light> aLights)
        {
            GameObjects = gameObjects;
            Lights = aLights;
            SkyBox = new SkyBox();
        }
        public void Render(Camera camera, Shader shader)
        {
            SkyBox.Render(camera);
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
                        (item as PointLight).DrawMesh(shader);
                    }
                }
            }
        }

        public void RenderObjects(Camera camera, Shader shader)
        {
            shader.SetFloat("material.shininess", 32.0f);
            foreach (var item in GameObjects)
            {
                item.Render(shader);
            }
        }
        private void SetupCamera(Camera camera, Shader shader)
        {
            shader.SetMatrix4("projection", camera.GetProjectionMatrix());
            shader.SetMatrix4("view", camera.GetViewMatrix());
            shader.SetVector3("viewPos", camera.Position);
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