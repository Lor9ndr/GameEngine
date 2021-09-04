using GameEngine.DefaultMeshes;
using GameEngine.Extensions;
using GameEngine.GameObjects;
using GameEngine.GameObjects.Base;
using GameEngine.GameObjects.Lights;
using GameEngine.Intefaces;
using GameEngine.RenderPrepearings.FrameBuffers;
using OpenTK.ImGui;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImGuiNET;
using GameEngine.Factories;
using GameEngine.Bases;
using GameEngine.Bases.Components;

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
        public static Vector2i ShadowSize = new Vector2i(1024, 1024);
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
        private ImGuiController controller;
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

            #endregion

            #region Lights
            _sun = LightFactory.GetDirectLight(new Vector3(0.5555345f, 10.0f, 0.412412f), new Vector3(0.0f,0.0f,0.0f));
            Light sl = LightFactory.GetSpotLight(_camera.Position, _camera.Front);
            _lights.Add(sl);
            _lights.Add(_sun);

            for (int i = 0; i < 3; i++)
            {
                var position = new Vector3(rd.Next(-50, 60), rd.Next(5, 50), rd.Next(-50, 50));
                _lights.Add(LightFactory.GetPointLight(position));
            }
            var ter = ModelFactory.GetTerrainModel(new Vector3(0));
            #endregion

            _models.Add(ter);
            _worldRenderer = new WorldRenderer(_models, _lights, this);
            _camera.Enable(this);

            /*controller = new ImGuiController(this);
            ImGui.StyleColorsClassic();*/
            this.Size = new Vector2i(Width, Height);
            GL.Enable(EnableCap.FramebufferSrgb);
            base.OnLoad();
            //ToggleFullscreen();
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            /*ImGui.NewFrame();*/
            Logger.ClearError();
            _worldRenderer.Render(_camera, false);
            Logger.CheckLastError();
#if DEBUG
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
            /*onDrawGUI();
            controller.Render();
            ImGui.EndFrame();*/

            SwapBuffers();
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            CalculateFPS();
            Title =
                $"FPS: {FPS}," +
            $" Objects: {_models.Count}," +
            $" Lights: {_lights.Count}, " +
            $" CAMERAPOS: {_camera.Position}" +
            $" Shadows: {Shadows}" +
            $" Gamma : {GammaEnable}";
            _worldRenderer.Update();
            foreach (var item in _lights.OfType<DirectLight>())
            {
                if ((bool)(KeyboardState.IsKeyDown(Keys.O)))
                {
                    item.Transform.Position = _camera.Position;
                }
            }
            _worldRenderer.LightShader.Use();
            _worldRenderer.LightShader.SetInt("shadows", Shadows);
            _worldRenderer.LightShader.SetInt("gammaEnable", GammaEnable);

            //controller.Update(this, _deltaTime);
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
            Game.Width = Size.X;
            Game.Height = Size.Y;

            // Tell ImGui of the new size
            //controller.WindowResized(e.Width, e.Height);
            base.OnResize(e);
        }
        protected override void OnUnload()
        {
            base.OnUnload();
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
                foreach (var item in _lights.OfType<PointLight>())
                {
                    item.GetComponent<Transform>().Position += new Vector3(0, 0.1f, 0);
                }

            }
            if (Keyboard.IsKeyDown(Keys.X))
            {
                ToggleFullscreen();
            }
            Shadows = Keyboard.IsKeyDown(Keys.B) ? 0 : 1;
            GammaEnable = Keyboard.IsKeyDown(Keys.G) ? 0 : 1;
            if (Keyboard.IsKeyDown(Keys.Down))
            {
                foreach (var item in _lights.OfType<PointLight>())
                {
                    item.Transform.Position -= new Vector3(0, 0.1f, 0);
                }


            }
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
            }
            
           

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

            if (ImGui.Begin("Objects"))
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

                    ImGui.TreePop();
                }
                if (ImGui.Begin("Lights"))
                {
                    if (ImGui.TreeNode("Lights"))
                    {
                        foreach (var obj in _worldRenderer.Lights)
                        {
                            var item = obj as Light;
                            ImGui.PushID(obj.GetType().ToString() + _worldRenderer.Lights.IndexOf(obj));
                            if (ImGui.TreeNode(item.GetType().ToString()))
                            {
                                ImGui.Text("Position");
                                Vector3 tmp = item.Transform.Position;
                                System.Numerics.Vector3 v = new System.Numerics.Vector3(tmp.X, tmp.Y, tmp.Z);
                                ImGui.DragFloat3("Position", ref v, 0.1f);
                                item.Transform.Position = new Vector3(v.X, v.Y, v.Z);

                               /* Vector3 amb = item.Ambient;
                                System.Numerics.Vector3 am = new System.Numerics.Vector3(amb.X, amb.Y, amb.Z);
                                ImGui.DragFloat3("Ambient", ref am, 0.1f);
                                item.Ambient = new Vector3(am.X, am.Y, am.Z);

                                Vector3 specular = item.Specular;
                                System.Numerics.Vector3 spec = new System.Numerics.Vector3(specular.X, specular.Y, specular.Z);
                                ImGui.DragFloat3("Ambient", ref spec, 0.1f);

                                item.Specular = new Vector3(spec.X, spec.Y, spec.Z);
                                ImGui.TreePop();*/
                            }

                            ImGui.PopID();
                        }

                        ImGui.TreePop();
                    }

                    ImGui.End();
                }
            }

            #endregion
        }
    }
}