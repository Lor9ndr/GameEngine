using GameEngine.GameObjects;
using GameEngine.GameObjects.Base;
using GameEngine.GameObjects.Lights;
using GameEngine.Intefaces;
using GameEngine.RenderPrepearings.FrameBuffers.Base;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
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
        public Shader DepthCubeShader = new Shader(Game.SHADOW_SHADERS_PATH + "DepthCubeMap.vert", Game.SHADOW_SHADERS_PATH + "DepthCubeMap.frag", Game.SHADOW_SHADERS_PATH + "DepthCubeMap.geom");
        public Shader DepthDirectShader = new Shader(Game.SHADOW_SHADERS_PATH + "Depth.vert", Game.SHADOW_SHADERS_PATH + "Depth.frag");
        public Shader LightShader = new Shader(Game.SHADOW_SHADERS_PATH + "SimpleShader.vert", Game.SHADOW_SHADERS_PATH + "SimpleShader.frag");
        public FrameBuffer DefaultFBO;

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
            DefaultFBO = new FrameBuffer(new Vector2i(Game.Width, Game.Height), ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
            DefaultFBO.Bind();
        }
        public void Render(Camera camera, bool drawlightMesh = false)
        {

            SetupCamera(camera, DepthCubeShader);
            Logger.ClearError();

            // Render a shadow framebuffers
            foreach (var item in Lights.OfType<PointLight>())
            {

                RenderPointLightShadows(item, camera);
                
            }


            Logger.CheckLastError();
            DefaultFBO.Activate();
            RenderShader(camera, LightShader);
            for (int i = 0; i < 6; i++)
            {
                GL.DisableVertexAttribArray(i);
            }
        }

        public void RenderLights(Camera camera, Shader shader, bool drawMesh = false, bool setupCamera = true)
        {
            if (setupCamera)
            {
                SetupCamera(camera, shader);
            }

            int textureIdx = 31;
            foreach (var item in Lights)
            {
                item.Render(shader, textureIdx--, drawMesh);
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
        public void SetupCamera(Camera camera, Shader shader)
        {
            shader.Use();
            shader.SetMatrix4("projection", camera.GetProjectionMatrix());
            shader.SetMatrix4("view", camera.GetViewMatrix());
            shader.SetVector3("viewPos", camera.Position);
        }
        public void RenderShader(Camera camera, Shader shader)
        {
            SkyBox.Render(camera);
            shader.Use();
            SetupCamera(camera,shader);
            RenderLights(camera, shader);
            RenderObjects(camera, shader);

        }
        public void RenderPointLightShadows(PointLight light, Camera camera)
        {
            light.ShadowData.Shadow.Activate();

            DepthCubeShader.Use();
            // light space matrices
            for (int i = 0; i < 6; i++)
            {
                DepthCubeShader.SetMatrix4($"shadowMatrices[{i}]", light.LightSpaceMatrices[i]);
            }
            DepthCubeShader.SetVector3("lightPos", light.Transform.Position);
            DepthCubeShader.SetFloat("far_plane", PointLight.FarPlane);

            RenderObjects(camera, DepthCubeShader);
            light.ShadowData.Shadow.Unbind();
        }
    }
}
