using BulletSharp;
using Engine.Components;
using Engine.GameObjects.Lights;
using Engine.Physics;
using Engine.Rendering.DefaultMeshes;
using Engine.Rendering.Enums;
using Engine.Rendering.Factories;
using Engine.Rendering.GameObjects;
using ImGuiNET;
using OpenTK.Graphics.OpenGL4;
using OpenTK.ImGui;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Engine
{
    /// <summary>
    /// Основной класс игры 
    /// TODO: добавить разные ивенты
    /// </summary>
    public class Game : GameWindow
    {
        #region Constructors

        /// <summary>
        /// Основной конструктор
        /// </summary>
        /// <param name="gameWindowSettings"></param>
        public Game(GameWindowSettings gameWindowSettings)
            : base(gameWindowSettings, new NativeWindowSettings() { APIVersion = new Version(4, 6), Profile = ContextProfile.Core })
        {
            CursorGrabbed = true;
        }
        #endregion

        #region Public Properties

        /// <summary>
        /// Менеджер физики
        /// </summary>
        public static PhysicsManager PhysicsManager { get => _physicsManager; set => _physicsManager = value; }

        /// <summary>
        /// Поток обновлений
        /// </summary>
        public static Thread UpdateThread { get => _updateThread; set => _updateThread = value; }

        /// <summary>
        /// Поток для обработки клавиатуры
        /// </summary>
        public static Thread KeyBoardThread { get => _keyBoardThread; set => _keyBoardThread = value; }

        /// <summary>
        /// Токенсурс для закрытия потоков
        /// </summary>
        public CancellationTokenSource CancellationTokenSource { get => _cancellationTokenSource; set => _cancellationTokenSource = value; }

        /// <summary>
        /// Ширина окна
        /// </summary>
        public static int Width { get => _width; set => _width = value; }

        /// <summary>
        /// Высота окна
        /// </summary>
        public static int Height { get => _height; set => _height = value; }

        /// <summary>
        /// Интеграция OpenGL
        /// </summary>
        public static EngineGL EngineGL { get => _engineGL; set => _engineGL = value; }

        /// <summary>
        /// Разрешение текстур, пока не работает
        /// </summary>
        public static Vector2i TextureSize { get => _textureSize; set => _textureSize = value; }

        /// <summary>
        /// Контроллер отрисовки интерфейса
        /// </summary>
        public static ImGuiController Controller { get => _controller; set => _controller = value; }

        /// <summary>
        /// Клавиатура
        /// </summary>
        public Keyboard Keyboard { get; private set; }

        /// <summary>
        /// Делегат на изменения разрешения теней
        /// </summary>
        /// <param name="shadows">разрешение теней</param>
        public delegate void OnShadowsChange(Vector2i shadows);

        /// <summary>
        /// Событие на изменение разрешения теней
        /// </summary>
        public static event OnShadowsChange ChangeShadowsSize;

        /// <summary>
        /// Разрешение теней
        /// </summary>
        public static Vector2i ShadowSize
        {
            get => _shadowSize;
            set
            {
                if (value != _shadowSize)
                {

                    _shadowSize = value;
                    ChangeShadowsSize(value);
                }
            }
        }

        /// <summary>
        /// Токен для закрытия потоков
        /// </summary>
        public CancellationToken CancellationToken;

        /// <summary>
        /// Кадры в секунду
        /// </summary>
        public static float FPS { get; private set; }

        /// <summary>
        /// Разница во времени между обновлением
        /// </summary>
        public static float DeltaTime => _deltaTime;

        /// <summary>
        /// Время приложения
        /// </summary>
        public static double Time => _time;

        public static List<float> Stats { get; private set; } = new List<float>();

        /// <summary>
        /// Класс рендера мира
        /// </summary>
        public WorldRenderer WorldRenderer;

        /// <summary>
        /// Просто тестовая штуковина для шейдера геометрии
        /// </summary>
        public bool Explode;

        /// <summary>
        /// Настройка, отвечающая за свет
        /// </summary>
        public static bool EnableLight = true;

        /// <summary>
        /// Настройка, отвечающая за туман, который скрывает видимость (хз как точней описать)
        /// </summary>
        public static bool EnableDistanceFOG = true;

        /// <summary>
        /// Настройка, отвечающая за включение/выключение отрисовку теней
        /// </summary>
        public static bool Shadows = true;

        /// <summary>
        /// Настройка, отвечающая за гамму
        /// </summary>
        public bool GammaEnable = true;

        #endregion

        #region Private Properties

        private double oldTimeSinceStart;
        private static float _framesPerSecond;
        private static float _lastTime;
        private static double _time;
        private static float _deltaTime;
        private Camera _camera;
        private readonly List<GameObject> _models = new();
        private readonly List<Light> _lights = new();
        private Shader _lightBoxShader;
        private bool _enabled;
        private bool _isGuiVisible;
        private bool _multiSample = true;
        private bool _useNormalMapping;
        private Player _player;
        private static Vector2i _shadowSize = new Vector2i(1024, 1024);

        private int _selectedObj;
        private bool _opened = false;
        private static PhysicsManager _physicsManager;
        private static Thread _updateThread;
        private static Thread _keyBoardThread;
        private CancellationTokenSource _cancellationTokenSource;
        private static ImGuiController _controller;
        private static int _width = 1920;
        private static int _height = 1080;
        private static Vector2i _textureSize = new Vector2i(1024, 1024);
        private static EngineGL _engineGL = new EngineGL();
        #endregion

        #region Const
        /// <summary>
        /// Фиксированное обновление в столько то мс или меньше,хз пока, не сильно нужно пока что
        /// </summary>
        public const float FixedUpdate = 0.0005f;


        public const string RESOURCE_PATH = "../../../../Engine/Resources/";
        public const string TEXTURES_PATH = RESOURCE_PATH + "Textures/";
        public const string OBJ_PATH = RESOURCE_PATH + "OBJ/";
        public const string NANOSUIT_PATH = OBJ_PATH + "NanoSuit/nanosuit.obj";
        public const string BRIDGE_PATH = OBJ_PATH + "Bridge/Jaaninoja_Bridge_decimated_SF.obj";
        public const string MAN_PATH = OBJ_PATH + "Man/man.fbx";
        public const string TERRAIN_PATH = OBJ_PATH + "Terrain/terrain1.fbx";
        public const string SKYBOX_TEXTURES_PATH = TEXTURES_PATH + "SkyBox/";

        public const string SHADERS_PATH = "../../../../Engine/Shaders/";
        public const string SIMPLE_SHADER = SHADERS_PATH + "SimpleShader";
        public const string DEFERRED_RENDER_PATH = SHADERS_PATH + "DeferredShading/";
        public const string ANOTHER_SHADERS_PATH = DEFERRED_RENDER_PATH + "Another/";
        public const string SKYBOX_SHADER_PATH = SHADERS_PATH + "SkyBox/SkyBox";
        public const string SHADOW_SHADERS_PATH = SHADERS_PATH + "ShadowShaders/";
        public const string DIRECT_SHADOW_SHADERS_PATH = SHADOW_SHADERS_PATH + "DirectShadows/";
        public const string POINT_SHADOW_SHADERS_PATH = SHADOW_SHADERS_PATH + "PointShadows/";

        #endregion

        #region Overrides
        /// <summary>
        /// Срабатывает при загрузке окна
        /// </summary>
        protected override void OnLoad()
        {
            EngineGL.ClearColor(1f, 1f, 1f, 1.0f)
                .DebugMessageCallback(Logger.DebugProcCallback, IntPtr.Zero)
                .Enable(EnableCap.DebugOutput)
                .Enable(EnableCap.DebugOutputSynchronous)
                .Enable(EnableCap.DepthTest)
                .Enable(EnableCap.FramebufferSrgb)
                .Enable(EnableCap.DepthTest)
                .Disable(EnableCap.CullFace);
            PhysicsManager = new PhysicsManager();
            _camera = new Camera(new Vector3(0, 0, 0), Size.X / (float)Size.Y);
            _lightBoxShader = new Shader(SHADERS_PATH + "LightBox.vert", SHADERS_PATH + "LightBox.frag");
            Random rd = new();
            // Model Loading

            #region Models
            _player = new Player(_camera, this);
            _models.Add(_player);

            var pos = new Vector3(0);
            Transform tr = new Transform(pos, new Vector3(0), new Vector3(0), new Vector3(3f));

            GameObject obj = new GameObject(ModelFactory.GetDancingVampire(), tr);
            _models.Add(obj);
            tr = new Transform(new Vector3(10, 0, 10), new Vector3(0), new Vector3(0), new Vector3(1f));

            
            var m1 = new GameObject(ModelFactory.GetNanoSuitModel(), tr);
            tr = new Transform(new Vector3(10, 0, -10), new Vector3(0), new Vector3(0), new Vector3(0.5f));
            var m2 = new GameObject(ModelFactory.GetNanoSuitModel(), tr);
            m2.AddChildren(m1);
            _models.Add(m2);
            _models.Add(m1);

            tr = new Transform(new Vector3(-10, -10, 10), new Vector3(0), new Vector3(0), new Vector3(0.05f));
            var terr = new GameObject(ModelFactory.GetTerrainModel(), tr);
           /* foreach (var mesh in terr.Model.Meshes)
            {
                ICollection<BulletSharp.Math.Vector3> positions = new List<BulletSharp.Math.Vector3>();
                foreach (var position in mesh.ObjectSetupper.Vertices)
                {
                    positions.Add(BulletExtensions.GetVector(position.Position * tr.Scale));
                }
                //ConvexHullShape convexShape = new ConvexHullShape(positions);
                ICollection<int> indices = new List<int>();
                foreach (var item in mesh.ObjectSetupper.GetIndices())
                {
                    indices.Add((int)item);
                }
                // TODO: Create a normal Model Collision based on vertices & triangles
                // Very expensive collision calculation 
                // incorrect places of colliders
                TriangleIndexVertexArray triangleIndexVertexArray = new TriangleIndexVertexArray(indices, positions);
                ConvexTriangleMeshCollider shape = new ConvexTriangleMeshCollider(triangleIndexVertexArray);
                shape.BaseCollider = new GameObject(new GameObjects.Model(mesh,$"mesh{meshCounter}"));
                var rb = PhysicsManager.LocalCreateRigidBody(0, BulletExtensions.GetMatrix(Matrix4.CreateTranslation(terr.Transform.Position)), shape);
                PhysicsManager.World.AddRigidBody(rb);
                meshCounter++;
                terr.RigidBody = rb;
            }*/
           

            _models.Add(terr);

            _models.Add(new GameObject(ModelFactory.GetNanoSuitModel(), tr));
            tr = new Transform(new Vector3(10, 0, 20), new Vector3(0), new Vector3(0), new Vector3(5f));
            _models.Add(new GameObject(ModelFactory.GetPBRTV(), tr));
            var planeTr = new Transform(new Vector3(0, -30, 0), new Vector3(0), new Vector3(0), new Vector3(1000, 0, 1000));
            var terrain = new GameObject(DefaultMesh.GetDefaultCube, planeTr);

            CollisionShape groundShape = new BoxCollider(1000, 1, 1000);
            PhysicsManager.collisionShapes.Add(groundShape);

            var ground = PhysicsManager.LocalCreateRigidBody(0, BulletExtensions.GetMatrix(Matrix4.CreateTranslation(terrain.Transform.Position)), groundShape);

            ground.UserObject = "Ground";
            terrain.RigidBody = ground;
            _models.Add(terrain);
            PhysicsManager.World.AddRigidBody(ground);

            for (int i = 0; i < 10; i++)
            {
                var position = new Vector3(rd.Next(-25, 25), rd.Next(50, 100), rd.Next(-25, 25));
                tr = new Transform(position);
                SphereCollider shape = new SphereCollider(tr.Scale.X);
                PhysicsManager.collisionShapes.Add(shape);
                BulletSharp.Math.Vector3 localInertia = shape.CalculateLocalInertia(1.0f);
                var rbInfo = new RigidBodyConstructionInfo(1.0f, null, shape, localInertia);
                rbInfo.MotionState = new DefaultMotionState(BulletExtensions.GetMatrix(Matrix4.CreateTranslation(tr.Position)));
                EngineRigidBody body = new EngineRigidBody(rbInfo);
                body.CollisionShape = shape;
                PhysicsManager.World.AddRigidBody(body);
                GameObject go = new GameObject(DefaultMesh.GetSphere, tr, body);
                _models.Add(go);
                rbInfo.Dispose();
            }

            #endregion

            #region Lights
            _lights.Add(LightFactory.GetDirectLight(new Vector3(0.5555345f, 25, 0.412412f), new Vector3(0.0f, 0.0f, 0.0f)));

            for (int i = 0; i < 5; i++)
            {
                var position = new Vector3(rd.Next(-25, 25), rd.Next(5, 10), rd.Next(-25, 25));
                var direction = new Vector3(0, -1, 0);
                _lights.Add(LightFactory.GetSpotLight(position, direction));
            }

            for (int i = 0; i < 5; i++)
            {
                var position = new Vector3(rd.Next(-25, 25), rd.Next(10, 25), rd.Next(-25, 25));
                _lights.Add(LightFactory.GetRandomColorPointLight(position));
            }
            #endregion

            WorldRenderer = new WorldRenderer(_models, _lights, this);
            _camera.Enable(this);

            Controller = new ImGuiController(this);
            this.Size = new Vector2i(Width, Height);


            base.OnLoad();

            //ToggleFullscreen();
            CancellationTokenSource = new CancellationTokenSource();
            CancellationToken = CancellationTokenSource.Token;
            Keyboard = new Keyboard(this);
            HandleKeyBoard();

            MouseDown += Game_MouseDown;
            ThreadsStart();
        }

        /// <summary>
        /// Срабатывает на каждый каждый рендр кадра
        /// </summary>
        /// <param name="args"></param>
        protected override void OnRenderFrame(FrameEventArgs args)
        {
            EngineGL
                .Enable(EnableCap.DepthTest)
                .Enable(EnableCap.DepthClamp)
                .DepthMask(true)
                .Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            WorldRenderer.Render(_camera, RenderFlags.LightData | RenderFlags.ProcessShadow, RenderFlags.MeshTexturesAnimation);

#if DEBUG
            WorldRenderer.RenderLights(_camera, _lightBoxShader, RenderFlags.MeshAndTextures);
            foreach (var item in WorldRenderer.GameObjects)
            {
                if (item.RigidBody is null)
                {
                    continue;
                }
                item.RigidBody.Render(_lightBoxShader, RenderFlags.MeshAndTextures);
            }

#endif
            if (_isGuiVisible)
            {
                onDrawGUI();
                Controller.Render();
            }
            SwapBuffers();
        }
        /// <summary>
        /// Срабатывает на каждое обновление кадра
        /// </summary>
        /// <param name="args"></param>
        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            CalculateFPS();
            var spotlight = _lights.OfType<SpotLight>().FirstOrDefault();
            Title =
                $"FPS: {FPS}," +
            $" Objects: {_models.Count}," +
            $" Lights: {_lights.Count}";
            PhysicsManager.Update((float)args.Time);
            WorldRenderer.Update(DeltaTime);
            // Menu Settings
            EngineGL.UseShader(WorldRenderer.LightShader)
                .SetShaderData("shadows", Shadows)
                .SetShaderData("gammaEnable", GammaEnable)
                .SetShaderData("time", (float)Time)
                .SetShaderData("EnableLight", EnableLight)
                .SetShaderData("EnableDistanceFog", EnableDistanceFOG)
                .SetShaderData("explodeObject", Explode)
                .SetShaderData("useNormalMapping", _useNormalMapping);
            
            //HandleKeyBoard();

            Controller.Update(this, _deltaTime);
            _time += args.Time;
            _deltaTime = (float)_time - (float)oldTimeSinceStart;
            base.OnUpdateFrame(args);
            oldTimeSinceStart = _time;
        }

        /// <summary>
        /// Срабатывает при изменении размера
        /// </summary>
        /// <param name="e"></param>
        protected override void OnResize(ResizeEventArgs e)
        {
            // Update the opengl viewport
            EngineGL.Viewport(0, 0, Width, Height);
            Width = Size.X;
            Height = Size.Y;

            // Tell ImGui of the new size
            Controller.WindowResized(e.Width, e.Height);
            base.OnResize(e);
        }
        /// <summary>
        /// Срабатывает на каждом движении колеса
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);
            Controller.MouseScroll(e.Offset);
        }
        /// <summary>
        /// Срабатывает при движении мышки
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseMove(MouseMoveEventArgs e)
        {
            base.OnMouseMove(e);
        }
        /// <summary>
        /// Срабатывает при закрытии окна
        /// </summary>
        protected override void OnUnload()
        {
            CancellationTokenSource.Cancel();
            PhysicsManager.ExitPhysics();
            WorldRenderer.Dispose();
            base.OnUnload();
            Console.WriteLine($"AverageFPS: {Stats?.Average()}");
        }
        #endregion

        #region Private Methods

        private void Game_MouseDown(MouseButtonEventArgs obj)
        {
            if (obj.Button is MouseButton.Button1 && !_isGuiVisible)
            {
                var position = new Vector3(_camera.Front + _camera.Transform.Position);
                var tr = new Transform(position);
                SphereCollider shape = new SphereCollider(tr.Scale.X);
                var body = PhysicsManager.LocalCreateRigidBody(1, BulletExtensions.GetMatrix(Matrix4.CreateTranslation(position)), shape);

                PhysicsManager.World.AddRigidBody(body);
                GameObject go = new GameObject(DefaultMesh.GetSphere, tr, body);
                body.ApplyCentralImpulse(new BulletSharp.Math.Vector3(_camera.Front.X * 20, _camera.Front.Y * 20, _camera.Front.Z * 20));
                _models.Add(go);
            }
        }

        private void ThreadsStart()
        {
            UpdateThread = new Thread(async () =>
            {
                while (!CancellationToken.IsCancellationRequested)
                {
                    await WorldRenderer.FixedUpdate();
                    Thread.Sleep(1);
                }
            });
            KeyBoardThread = new Thread(async () =>
            {
                while (!CancellationToken.IsCancellationRequested)
                {
                    await Keyboard.UpdateAsync();
                    Thread.Sleep(1);
                }
            });
            KeyBoardThread.Start();
            UpdateThread.Start();
        }
        private static void CalculateFPS()
        {
            float currentTime = (float)(_time);

            _framesPerSecond++;
            if ((currentTime - _lastTime) > 1.0f)
            {
                _lastTime = currentTime;

                FPS = _framesPerSecond;
                Stats.Add(FPS);

                _framesPerSecond = 0;
            }
        }

        private void HandleKeyBoard()
        {
            Keyboard.BindKey(Keys.O,new Action(() => {
                WorldRenderer.Lights.First(s => s.GetType() == typeof(SpotLight)).Transform.Position = _camera.Transform.Position;
                WorldRenderer.Lights.First(s => s.GetType() == typeof(SpotLight)).Transform.Direction = _camera.Front;
            }), PressType.Pressing);
            Keyboard.BindKey(Keys.Escape, Close, PressType.Down);
            Keyboard.BindKey(Keys.M, new Action(() => { EngineGL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Point);}), PressType.Down);
            Keyboard.BindKey(Keys.Comma, new Action(() => { EngineGL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line); }), PressType.Down);
            Keyboard.BindKey(Keys.Period, new Action(() => { EngineGL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill); }), PressType.Down);
            Keyboard.BindKey(Keys.Up, new Action(() =>
            {
                foreach (var item in _lights.OfType<SpotLight>())
                {
                    item.LightData.Intensity += 1.0f;
                }
            }), PressType.Down);

            Keyboard.BindKey(Keys.Down, new Action(() =>
            {
                foreach (var item in _lights.OfType<SpotLight>())
                {
                    item.LightData.Intensity -= 1.0f;
                }
            }), PressType.Down);
            Keyboard.BindKey(Keys.X,ToggleFullscreen, PressType.Down);

            Keyboard.BindKey(Keys.Left, new Action(() =>
            {
                foreach (var item in _lights.OfType<PointLight>())
                {
                    item.Transform.Position += new Vector3(1, 0, 0);
                }
            }), PressType.Pressing);

            Keyboard.BindKey(Keys.Right, new Action(() =>
            {
                foreach (var item in _lights.OfType<PointLight>())
                {
                    item.Transform.Position -= new Vector3(1, 0, 0);
                }
            }),PressType.Pressing);
            Keyboard.BindKey(Keys.LeftControl,new Action(() =>
            {
                CursorGrabbed = false;
                CursorVisible = true;
                _isGuiVisible = !CursorGrabbed;
                _camera.CanMove = CursorGrabbed;
            }),PressType.Down);

            Keyboard.BindKey(Keys.LeftAlt, new Action(() =>
            {
                CursorGrabbed = true;
                _camera.CanMove = CursorGrabbed;
                _isGuiVisible = !CursorGrabbed;
            }), PressType.Down);

        }


        private void ToggleFullscreen()
        {
            if (IsFullscreen)
            {
                WindowBorder = WindowBorder.Fixed;
                WindowState = WindowState.Normal;
            }
            else
            {
                WindowBorder = WindowBorder.Hidden;
                WindowState = WindowState.Fullscreen;
            }
            Thread.Sleep(360);
            _camera.AspectRatio = Size.X / (float)Size.Y;
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
                    ImGui.Checkbox("Enabled", ref _enabled);

                    ImGui.EndMenu();
                }
                ImGui.Checkbox("LIGHT", ref EnableLight);
                ImGui.Checkbox("Shadows", ref Shadows);

                ImGui.Checkbox("FOG", ref EnableDistanceFOG);
                ImGui.Checkbox("GammaCorrection", ref GammaEnable);
                ImGui.Checkbox("MultiSample", ref _multiSample);
                ImGui.Checkbox("Explode", ref Explode);
                ImGui.Checkbox("UseNormalMapping", ref _useNormalMapping);



                ImGui.EndMainMenuBar();
            }
            List<GameObject> gos = new List<GameObject>();
            gos.AddRange(WorldRenderer.GameObjects);
            gos.AddRange(WorldRenderer.Lights);
            if (ImGui.ListBox("Objects and Lights", ref _selectedObj, gos.Select(s => s.GetType().ToString()).ToArray(), gos.Count, 30))
            {
            }
            if (ImGui.Begin("Properties", ref _opened, ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.AlwaysUseWindowPadding))
            {
                var obj = gos[_selectedObj];

                ImGui.Text("Transform");

                Vector3 pos = obj.Transform.Position;
                var p = new System.Numerics.Vector3(pos.X, pos.Y, pos.Z);
                ImGui.DragFloat3("Position", ref p, 0.1f);
                obj.Transform.Position = new Vector3(p.X, p.Y, p.Z);

                Vector3 rotation = obj.Transform.Rotation;
                var r = new System.Numerics.Vector3(rotation.X, rotation.Y, rotation.Z);
                ImGui.DragFloat3("Rotation", ref r, 0.1f);
                obj.Transform.Rotation = new Vector3(r.X, r.Y, r.Z);

                Vector3 scale = obj.Transform.Scale;
                var s = new System.Numerics.Vector3(scale.X, scale.Y, scale.Z);
                ImGui.DragFloat3("Scale", ref s, 0.1f);
                obj.Transform.Scale = new Vector3(s.X, s.Y, s.Z);
                if (obj is Light)
                {
                    var intensity = obj.LightData.Intensity;
                    ImGui.DragFloat("Intensity", ref intensity, 0.1f);
                    obj.LightData.Intensity = intensity;
                }

            }
            ImGui.End();
        }
        #endregion
    }
}