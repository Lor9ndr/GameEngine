using Engine.GameObjects;
using Engine.GameObjects.Lights;
using Engine.GLObjects.FrameBuffers;
using Engine.GLObjects.Textures;
using Engine.Rendering.Enums;
using Engine.Rendering.GameObjects;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;
using System.Collections.Generic;
using System.Linq;

namespace Engine
{
    public class WorldRenderer
    {
        public SkyBox SkyBox;
        public List<GameObject> GameObjects;
        public List<Light> Lights;
        public Shader DepthCubeShader = new Shader(Game.SHADOW_SHADERS_PATH + "DepthCubeMap.vert", Game.SHADOW_SHADERS_PATH + "DepthCubeMap.frag", Game.SHADOW_SHADERS_PATH + "DepthCubeMap.geom");
        public Shader DepthDirectShader = new Shader(Game.SHADOW_SHADERS_PATH + "Depth.vert", Game.SHADOW_SHADERS_PATH + "Depth.frag");
        public Shader LightShader = new Shader(Game.SHADERS_PATH + "SimpleShader.vert", Game.SHADERS_PATH + "SimpleShader.frag", Game.SHADERS_PATH + "SimpleShader.geom");
        public FrameBuffer DefaultFBO;
        public FrameBuffer MultiSampleBuffer;

        public WorldRenderer(List<GameObject> gameObjects)
        {
            GameObjects = gameObjects;
            SkyBox = new SkyBox();
        }
        public WorldRenderer(List<GameObject> gameObjects, List<Light> aLights, GameWindow window)
        {
            GameObjects = gameObjects;
            Lights = aLights;
            SkyBox = new SkyBox();
            DefaultFBO = new FrameBuffer(new Vector2i(Game.Width, Game.Height), ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            DefaultFBO.Bind();
            window.Resize += Window_Resize;
            Game.ChangeShadowsSize += Game_ChangeShadowsSize;
        }

        private void Game_ChangeShadowsSize(Vector2i shadows)
        {
            LightShader = new Shader(Game.SHADERS_PATH + "SimpleShader.vert", Game.SHADERS_PATH + "SimpleShader.frag", Game.SHADERS_PATH + "SimpleShader.geom");
        }

        private void Window_Resize(OpenTK.Windowing.Common.ResizeEventArgs obj)
        {
            DefaultFBO.Size = obj.Size;
            DefaultFBO.SetViewPort();
        }

        public void Render(Camera camera, RenderFlags lightFlags, RenderFlags objFlags)
        {
            var plcounter = Lights.Count(s => s.GetType() == typeof(PointLight));
            LightShader.SetInt("nrSpotLights", Lights.Count(s => s.GetType() == typeof(SpotLight)));
            LightShader.SetInt("nrPointLights", plcounter);
            GL.Enable(EnableCap.DepthTest);

            if (Game.Shadows)
            {
                GL.Enable(EnableCap.CullFace);
                GL.CullFace(CullFaceMode.Front);
                // Render a shadow framebuffers
                SetupCamera(camera, DepthDirectShader);
                // Render SpotLight and DirectLight shadows
                foreach (var item in Lights.Where(s => s.GetType() == typeof(DirectLight) || s.GetType() == typeof(SpotLight)))
                {
                    RenderDirectAndSpotLightShadow(item, camera);
                }

                SetupCamera(camera, DepthCubeShader);
                // Render PointLight shadows
                foreach (var item in Lights.OfType<PointLight>())
                {
                    RenderPointLightShadows(item, camera);
                }
            }
            DefaultFBO.Activate();

            // Render scene as normal
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);
            RenderShader(camera, LightShader, lightFlags, objFlags);

        }

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

        public void RenderObjects(Camera camera, Shader shader, RenderFlags objFlags)
        {
            if (objFlags.HasFlag(RenderFlags.Textures))
            {
                shader.SetFloat("material.shininess", 32.0f);
            }
            foreach (var item in GameObjects)
            {
                item.Render(shader, objFlags);
            }
        }
      
        public void RenderShader(Camera camera, Shader shader, RenderFlags lightFlags, RenderFlags objFlags)
        {

            SkyBox.Render(camera, RenderFlags.MeshAndTextures);
            shader.Use();
            SetupCamera(camera,shader);

            
            RenderLights(camera, shader, lightFlags);

            RenderObjects(camera, shader, objFlags);



        }
        private void RenderDirectAndSpotLightShadow(Light light, Camera camera)
        {
            light.ShadowData.Shadow.Activate();
            DepthDirectShader.Use();
            DepthDirectShader.SetMatrix4("lightSpaceMatrix", light.LightSpaceMatrix);
            DepthDirectShader.SetMatrix4("model", light.Transform.Model);
            RenderObjects(camera, DepthDirectShader, RenderFlags.Mesh);
            light.ShadowData.Shadow.Unbind();
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
            DepthCubeShader.SetFloat("far_plane", light.FarPlane);
            DepthCubeShader.SetMatrix4("model", light.Transform.Model);

            RenderObjects(camera, DepthCubeShader, RenderFlags.Mesh);
            light.ShadowData.Shadow.Unbind();
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
            shader.SetMatrix4("VP", camera.GetViewMatrix() * camera.GetProjectionMatrix());
            shader.SetMatrix4("projection", camera.GetProjectionMatrix());
            shader.SetMatrix4("view", camera.GetViewMatrix());
            shader.SetVector3("viewPos", camera.Position);
        }
    }
}
