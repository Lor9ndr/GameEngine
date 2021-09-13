using GameEngine.GameObjects;
using GameEngine.GameObjects.Base;
using GameEngine.GameObjects.Lights;
using GameEngine.Intefaces;
using OpenTK.ImGui;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using ImGuiNET;
using GameEngine.Factories;
using GameEngine.Bases.Components;
using GameEngine.Bases;

namespace GameEngine
{
    public class Game : GameWindow
    {
        #region Constructors

        public Game(GameWindowSettings gameWindowSettings)
            : base(gameWindowSettings, new NativeWindowSettings() { APIVersion = new Version(4, 6) })
        {
            CursorGrabbed = true;
        }
        #endregion

        #region Public Properties

        public static float FPS { get; private set; }
        public static float DeltaTime => _deltaTime;
        public static double Time => _time;
        public static int Width = 1920;
        public static int Height = 1080;
        public static Vector2i ShadowSize = new Vector2i(2048, 2048);
        public KeyboardState Keyboard { get; private set; }
        #endregion

        #region Private Properties
        private double oldTimeSinceStart = 0;
        private static float _framesPerSecond = 0.0f;
        private static float _lastTime = 0.0f;
        private static double _time;
        private static float _deltaTime;
        private Camera _camera;
        private readonly List<IRenderable> _models = new();
        private List<Light> _lights = new();
        private WorldRenderer _worldRenderer;

        private Shader LightBoxShader;

        private Light _sun;
        internal static int Shadows = 1;
        internal static int GammaEnable = 1;
        private bool enabled;
        public static ImGuiController Controller;
        private bool isGuiVisible = false;
        #endregion

        #region Const
        public const string RESOURCE_PATH = "../../../Resources/";
        public const string TEXTURES_PATH = RESOURCE_PATH + "Textures/";
        public const string OBJ_PATH = RESOURCE_PATH + "OBJ/";
        public const string NANOSUIT_PATH = OBJ_PATH + "NanoSuit/nanosuit.obj";
        public const string BRIDGE_PATH = OBJ_PATH + "Bridge/Jaaninoja_Bridge_decimated_SF.obj";
        public const string MAN_PATH = OBJ_PATH + "Man/man.fbx";
        public const string TERRAIN_PATH = OBJ_PATH + "Terrain/terrain1.fbx";
        public const string SKYBOX_TEXTURES_PATH = TEXTURES_PATH + "SkyBox/";

        public const string SHADERS_PATH = "../../../Shaders/";
        public const string SIMPLE_SHADER = SHADERS_PATH + "SimpleShader";
        public const string DEFERRED_RENDER_PATH = SHADERS_PATH + "DeferredShading/";
        public const string ANOTHER_SHADERS_PATH = DEFERRED_RENDER_PATH + "Another/";
        public const string SKYBOX_SHADER_PATH = SHADERS_PATH + "SkyBox/SkyBox";
        public const string SHADOW_SHADERS_PATH = SHADERS_PATH + "ShadowShaders/";
        public const string DIRECT_SHADOW_SHADERS_PATH = SHADOW_SHADERS_PATH + "DirectShadows/";
        public const string POINT_SHADOW_SHADERS_PATH = SHADOW_SHADERS_PATH + "PointShadows/";

        #endregion

        #region Overrides
        protected override void OnLoad()
        {
            GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);

            GL.Enable(EnableCap.DepthTest);

            _camera = new Camera(new Vector3(0, 0, 3), Size.X / (float)Size.Y);
            //SimpleShader = new Shader(SIMPLE_SHADER + ".vs", SIMPLE_SHADER + ".fr");
            LightBoxShader = new Shader(DEFERRED_RENDER_PATH + "LightBoxV.glsl", DEFERRED_RENDER_PATH + "LightBoxfr.glsl");
            Random rd = new();

            #region Models
            _models.Add(ModelFactory.GetManModel(new Vector3(0)));
            for (int i = 0; i < 15; i++)
            {
                var pos = new Vector3(rd.Next(-20, 15), rd.Next(0, 30), rd.Next(-15, 16));
                _models.Add(ModelFactory.GetNanoSuitModel(pos));

            }
            _models.Add(ModelFactory.GetTerrainModel(new Vector3(0)));

            #endregion

            #region Lights
            _sun = LightFactory.GetDirectLight(new Vector3(0.5555345f, 10.0f, 0.412412f), new Vector3(0.0f,0.0f,0.0f));
            //Light sl = LightFactory.GetSpotLight(_camera.Position, _camera.Front);
            //_lights.Add(sl);
            _lights.Add(_sun);

            for (int i = 0; i < 10; i++)
            {
                var position = new Vector3(rd.Next(-5, 6), rd.Next(0, 10), rd.Next(-5, 5));
                var direction = new Vector3(rd.Next(-100,100), rd.Next(-100,100), rd.Next(-100, 100));
                _lights.Add(LightFactory.GetSpotLight(position, direction));
            }

           for (int i = 0; i < 5; i++)
           {
               var position = new Vector3(rd.Next(-5, 6), rd.Next(0, 10), rd.Next(-5, 5));
               _lights.Add(LightFactory.GetPointLight(position));
           }
            #endregion

            _worldRenderer = new WorldRenderer(_models, _lights, this);
            _camera.Enable(this);

            Controller = new ImGuiController(this);
            ImGui.StyleColorsDark();
            this.Size = new Vector2i(Width, Height);
            GL.Enable(EnableCap.FramebufferSrgb);
            base.OnLoad();
            //ToggleFullscreen();
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            _worldRenderer.Render(_camera,RenderFlags.None, RenderFlags.MeshAndTextures);
            
#if DEBUG
            LightBoxShader.Use();
            _worldRenderer.SetupCamera(_camera, LightBoxShader);
            foreach (var item in _worldRenderer.Lights)
            {
                item.SetupModel(LightBoxShader);
                if (item.Mesh != null)
                {
                    item.Mesh.Textures[0].Use(TextureUnit.Texture0);
                    LightBoxShader.SetVector3("lightColor", item.LightData.Color);
                    item.DrawMesh(LightBoxShader);
                }
            }
#endif
            onDrawGUI();
            if (isGuiVisible)
            {
                Controller.Render();
            }

            SwapBuffers();
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            CalculateFPS();
            var spotlight = _lights.OfType<SpotLight>().FirstOrDefault();
            Title =
                $"FPS: {FPS}," +
            $" Objects: {_models.Count}," +
            $" Lights: {_lights.Count}, " +
            $" CAMERAPOS: {_camera.Position}" +
            $" Shadows: {Shadows}" +
            $" Gamma : {GammaEnable}";
            _worldRenderer.Update();
            if ((bool)(KeyboardState.IsKeyDown(Keys.O)))
            {
                _lights.OfType<SpotLight>().FirstOrDefault().Transform.Position = _camera.Position;
                _lights.OfType<SpotLight>().FirstOrDefault().Transform.Direction = _camera.Front;
            }

            _worldRenderer.LightShader.Use();
            _worldRenderer.LightShader.SetInt("shadows", Shadows);
            _worldRenderer.LightShader.SetInt("gammaEnable", GammaEnable);

            Controller.Update(this, _deltaTime);
            _time += args.Time;
            _deltaTime = (float)_time - (float)oldTimeSinceStart;
            HandleKeyBoard();
            base.OnUpdateFrame(args);
            oldTimeSinceStart = _time;
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            // Update the opengl viewport
            GL.Viewport(0, 0, e.Width, e.Height);
            Width = Size.X;
            Height = Size.Y;

            // Tell ImGui of the new size
            Controller.WindowResized(e.Width, e.Height);
            base.OnResize(e);
        }
        protected override void OnTextInput(TextInputEventArgs e)
        {
            base.OnTextInput(e);
            Controller.PressChar((char)e.Unicode);
        }
        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);
            Controller.MouseScroll(e.Offset);
        }
        #endregion

        #region Private Methods

        private static void CalculateFPS()
        {
            float currentTime = (float)(_time);

            _framesPerSecond++;
            if ((currentTime - _lastTime) > 1.0f)
            {
                _lastTime = currentTime;

                FPS = _framesPerSecond;

                _framesPerSecond = 0;
            }
        }

        private void HandleKeyBoard()
        {
            Keyboard = KeyboardState;
            if (Keyboard.IsKeyDown(Keys.Escape))
            {
                this.Close();
            }
            if (Keyboard.IsKeyDown(Keys.M))
            {
                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Point);
            }
            if (Keyboard.IsKeyDown(Keys.Comma))
            {
                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
            }
            if (Keyboard.IsKeyDown(Keys.Period))
            {
                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
            }
            if (Keyboard.IsKeyDown(Keys.Up))
            {
                foreach (var item in _lights.OfType<SpotLight>())
                {
                    item.LightData.Intensity += 1.0f;
                }
            }
            if (Keyboard.IsKeyDown(Keys.Down))
            {
                foreach (var item in _lights.OfType<SpotLight>())
                {
                    item.LightData.Intensity -= 1.0f;
                }

            }
            if (Keyboard.IsKeyDown(Keys.X))
            {
                ToggleFullscreen();
            }
            Shadows = Keyboard.IsKeyDown(Keys.B) ? 0 : 1;
            GammaEnable = Keyboard.IsKeyDown(Keys.G) ? 0 : 1;
           
            if (Keyboard.IsKeyDown(Keys.Left))
            {
                foreach (var item in _lights.OfType<PointLight>())
                {
                    item.Transform.Position += new Vector3(1, 0, 0);
                }

            }
            if (Keyboard.IsKeyDown(Keys.Right))
            {
                foreach (var item in _lights.OfType<PointLight>())
                {
                    item.Transform.Position -= new Vector3(1, 0, 0);
                }
            }

            if (Keyboard.IsKeyDown(Keys.LeftControl))
            {
                CursorGrabbed = false;
                _camera.CanMove = CursorGrabbed;
                CursorVisible = true;
            }
            if (Keyboard.IsKeyDown(Keys.LeftAlt))
            {
                CursorGrabbed = true;
                _camera.CanMove = CursorGrabbed;
                CursorVisible = false;
                MousePosition = new Vector2(Game.Width /2, Game.Height/2);
            }
            isGuiVisible = CursorVisible;
            
           

        }

        private void ToggleFullscreen()
        {
            if (IsFullscreen)
            {
                WindowBorder = WindowBorder.Resizable;
                WindowState = WindowState.Normal;
                _camera.AspectRatio = Size.X / (float)Size.Y;
            }
            else
            {
                WindowBorder = WindowBorder.Hidden;
                WindowState = WindowState.Fullscreen;
                _camera.AspectRatio = Size.X / (float)Size.Y;
            }
        }
        void onDrawGUI()
        {
            if (ImGui.BeginMainMenuBar())
            {
                if (ImGui.BeginMenu("File"))
                {
                    if (ImGui.MenuItem("Open", "CTRL+O"))
                    {
                    }

                    ImGui.Separator();
                    if (ImGui.MenuItem("Save", "CTRL+S"))
                    {
                    }

                    if (ImGui.MenuItem("Save As", "CTRL+Shift+S"))
                    {
                    }

                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu("Settings"))
                {
                    ImGui.Checkbox("Enabled", ref enabled);

                    ImGui.EndMenu();
                }

                ImGui.EndMainMenuBar();
            }
            if (ImGui.Begin("GameObjects"))
            {
                if (ImGui.TreeNode("Objects"))
                {
                    foreach (var obj in _worldRenderer.GameObjects)
                    {
                        var item = obj as AMovable;
                        ImGui.PushID(obj.GetType().ToString() + _worldRenderer.GameObjects.IndexOf(obj));
                        if (ImGui.TreeNode(item.GetType().ToString()))
                        {
                            ImGui.Text("Position");

                            Vector3 tmp = item.Transform.Position;
                            System.Numerics.Vector3 v = new System.Numerics.Vector3(tmp.X, tmp.Y, tmp.Z);
                            ImGui.DragFloat3("Position", ref v, 0.1f);
                            item.Transform.Position = new Vector3(v.X, v.Y, v.Z);
                            ImGui.TreePop();
                        }

                        ImGui.PopID();
                    }
                    if (ImGui.TreeNode("Lights"))
                    {
                        foreach (var obj in _worldRenderer.Lights)
                        {
                            var item = obj; 
                            ImGui.PushID(obj.GetType().ToString() + _worldRenderer.Lights.IndexOf(obj));
                            if (ImGui.TreeNode(item.GetType().ToString()))
                            {
                                ImGui.Text("Position");
                                Vector3 tmp = item.Transform.Position;
                                System.Numerics.Vector3 v = new System.Numerics.Vector3(tmp.X, tmp.Y, tmp.Z);
                                ImGui.DragFloat3("Position", ref v, 0.1f);
                                item.Transform.Position = new Vector3(v.X, v.Y, v.Z);

                                ImGui.Text("Intensity");
                                float intens = item.LightData.Intensity;
                                ImGui.DragFloat("Intensity", ref intens);
                                item.LightData.Intensity = intens;

                                var near = item.NearPlane;
                                var far = item.FarPlane;

                                ImGui.DragFloat("Near", ref near, 1.0f, 1.0f, 100.0f);
                                ImGui.DragFloat("Far", ref far, 1.0f, 1.0f, 9999.0f);
                                item.NearPlane = near;
                                item.FarPlane = far;
                            }

                            ImGui.PopID();
                        }

                        ImGui.TreePop();
                    }
                }
            }
            ImGui.End();

            #endregion
        }
    }
}