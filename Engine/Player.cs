using BulletSharp;
using Engine.Components;
using Engine.GameObjects;
using Engine.Physics;
using Engine.Rendering.Factories;
using Engine.Rendering.GameObjects;
using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Engine
{
    public class Player : GameObject
    {
        public Camera Camera { get; set; }
        public float Speed = 20.0f;
        private readonly KeyboardState _keyboardState;
        private readonly KinematicCharacterController controller;

        public Player(Camera camera, GameWindow window, Model model = default, EngineRigidBody rb = null)
            : base(model, rb)
        {
            Camera = camera;
            Model = ModelFactory.GetNanoSuitModel();
            window.UpdateFrame += Window_UpdateFrame;
            _keyboardState = window.KeyboardState;
            CapsuleCollider capsule = new CapsuleCollider(1, 10);
            Game.PhysicsManager.collisionShapes.Add(capsule);
            Transform = new Transform(new Vector3(0, 3, 0), new Vector3(0), new Vector3(0), new Vector3(1));
            RigidBodyConstructionInfo rpPlayer = new RigidBodyConstructionInfo(10, null, capsule);
            PairCachingGhostObject pairCachingGhostObject = new PairCachingGhostObject();
            rpPlayer.MotionState = new DefaultMotionState(BulletExtensions.GetMatrix(Matrix4.CreateTranslation(Transform.Position)));
            rpPlayer.LocalInertia = new BulletSharp.Math.Vector3(0, 1, 0);
            rb = new EngineRigidBody(rpPlayer);
            rb.UserObject = "Player";
            RigidBody = rb;
            Game.PhysicsManager.World.AddRigidBody(rb);
            rpPlayer.Dispose();
            AddChildren(camera);
        }

        private void Window_UpdateFrame(OpenTK.Windowing.Common.FrameEventArgs obj)
        {
            Vector3 pos = Transform.Position;
            RigidBody.ForceActivationState(ActivationState.ActiveTag);
            if (_keyboardState.IsKeyDown(Keys.Space))
            {
                RigidBody.ApplyCentralImpulse(new BulletSharp.Math.Vector3(0, 1, 0));
            }
            if (_keyboardState.IsKeyDown(Keys.W))
            {
                RigidBody.ApplyCentralImpulse(new BulletSharp.Math.Vector3(Camera.Front.X, 0, Camera.Front.Z));
            }
            if (_keyboardState.IsKeyDown(Keys.S))
            {
                RigidBody.ApplyCentralImpulse(-new BulletSharp.Math.Vector3(Camera.Front.X, 0, Camera.Front.Z));
            }
            if (_keyboardState.IsKeyDown(Keys.D))
            {
                RigidBody.ApplyCentralImpulse(new BulletSharp.Math.Vector3(Camera.Right.X, 0, Camera.Right.Z));
            }
            if (_keyboardState.IsKeyDown(Keys.A))
            {
                RigidBody.ApplyCentralImpulse(-new BulletSharp.Math.Vector3(Camera.Right.X, 0, Camera.Right.Z));
            }


            var p = BulletExtensions.GetMatrix(RigidBody.MotionState.WorldTransform).ExtractTranslation();
            //Camera.Transform.Position = new Vector3(p.X, (float)(p.Y + (RigidBody.CollisionShape as CapsuleCollider).HalfHeight * 3), p.Z);
            //Transform.Rotation = new Vector3(Camera.Front.X,0,Camera.Front.Z);
            FreezeRotation();
        }
        private void FreezeRotation()
        {
            /*RigidBody.SpinningFriction = 0;
            RigidBody.RollingFriction = 0;*/
        }
    }
}
