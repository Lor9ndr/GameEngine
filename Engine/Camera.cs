using Engine.Rendering.GameObjects;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;

namespace Engine
{
    public class Camera : GameObject
    {
        public float CameraSpeed
        {
            get => _cameraSpeed;
        }
        public float Sensitivity { get => _sensitivity; }
        private float _cameraSpeed = 10.0f;

        public bool CanMove = true;

        private readonly float _sensitivity = 0.2f;
        private KeyboardState _keyboard;

        private Vector3 _up = Vector3.UnitY;

        private Vector3 _right = Vector3.UnitX;


        // Rotation around the X axis (radians)
        private float _pitch;

        // Rotation around the Y axis (radians)
        private float _yaw = -MathHelper.PiOver2; // Without this you would be started rotated 90 degrees right

        // The field of view of the camera (radians)
        private float _fov = MathHelper.PiOver2;

        public Camera(Vector3 position, float aspectRatio) : base()
        {
            AspectRatio = aspectRatio;
            Transform.Position = position;
            Transform.Direction = Vector3.UnitZ;
        }


        // This is simply the aspect ratio of the viewport, used for the projection matrix
        public float AspectRatio { private get; set; }

        public Vector3 Front => Transform.Direction;

        public Vector3 Up => _up;

        public Vector3 Right => _right;

        public void Enable(Game window)
        {
            window.UpdateFrame += Window_UpdateFrame;
            window.MouseMove += Window_MouseMove;
            _keyboard = window.KeyboardState;
        }
        public void Disable(Game window)
        {
            window.UpdateFrame -= Window_UpdateFrame;
            window.MouseMove -= Window_MouseMove;
            _keyboard = null;
        }
        private void Window_UpdateFrame(FrameEventArgs obj)
        {
            HandleKeyBoard();
        }
        public void SetCameraSpeed(float speed)
        {
            if (speed > 0 && speed < 1000)
            {
                _cameraSpeed = speed;
            }
        }
        private void HandleKeyBoard()
        {
            if (_keyboard == null)
            {
                return;
            }

            if (_keyboard.IsKeyDown(Keys.W))
            {
                Transform.Position += Front * CameraSpeed * Game.DeltaTime; // Forward
            }
            if (_keyboard.IsKeyDown(Keys.S))
            {
                Transform.Position -= Front * CameraSpeed * Game.DeltaTime; // Backwards
            }
            if (_keyboard.IsKeyDown(Keys.A))
            {
                Transform.Position -= Right * CameraSpeed * Game.DeltaTime; // Left
            }
            if (_keyboard.IsKeyDown(Keys.D))
            {
                Transform.Position += Right * CameraSpeed * Game.DeltaTime; // Right
            }
            if (_keyboard.IsKeyDown(Keys.Space))
            {
                Transform.Position += Up * CameraSpeed * Game.DeltaTime; // Up
            }
            if (_keyboard.IsKeyDown(Keys.LeftShift))
            {
                Transform.Position -= Up * CameraSpeed * Game.DeltaTime; // Down
            }
            if (_keyboard.IsKeyDown(Keys.RightAlt))
            {
                SetCameraSpeed(CameraSpeed + 1);
            }
            if (_keyboard.IsKeyDown(Keys.RightControl))
            {
                SetCameraSpeed(CameraSpeed - 1);
            }
        }
        private void Window_MouseMove(MouseMoveEventArgs mouse)
        {
            if (CanMove)
            {
                Yaw += mouse.DeltaX * _sensitivity;
                Pitch -= mouse.DeltaY * _sensitivity;
            }
        }

        // We convert from degrees to radians as soon as the property is set to improve performance
        public float Pitch
        {
            get => MathHelper.RadiansToDegrees(_pitch);
            set
            {
                // We clamp the pitch value between -89 and 89 to prevent the camera from going upside down, and a bunch
                // of weird "bugs" when you are using euler angles for rotation.
                // If you want to read more about this you can try researching a topic called gimbal lock
                var angle = MathHelper.Clamp(value, -89f, 89f);
                _pitch = MathHelper.DegreesToRadians(angle);
                UpdateVectors();
            }
        }

        // We convert from degrees to radians as soon as the property is set to improve performance
        public float Yaw
        {
            get => MathHelper.RadiansToDegrees(_yaw);
            set
            {
                _yaw = MathHelper.DegreesToRadians(value);
                UpdateVectors();
            }
        }

        // The field of view (FOV) is the vertical angle of the camera view, this has been discussed more in depth in a
        // previous tutorial, but in this tutorial you have also learned how we can use this to simulate a zoom feature.
        // We convert from degrees to radians as soon as the property is set to improve performance
        public float Fov
        {
            get => MathHelper.RadiansToDegrees(_fov);
            set
            {
                var angle = MathHelper.Clamp(value, 1f, 45f);
                _fov = MathHelper.DegreesToRadians(angle);
            }
        }

        // Get the view matrix using the amazing LookAt function described more in depth on the web tutorials
        public virtual Matrix4 GetViewMatrix()
        {
            return Matrix4.LookAt(Transform.Position, Transform.Position + Transform.Direction, _up);

        }

        // Get the projection matrix using the same method we have used up until this point
        public Matrix4 GetProjectionMatrix()
        {
            return Matrix4.CreatePerspectiveFieldOfView(_fov, AspectRatio, 0.001f, 100000f);
        }

        // This function is going to update the direction vertices using some of the math learned in the web tutorials
        private void UpdateVectors()
        {
            // First the front matrix is calculated using some basic trigonometry
            Transform.Direction = new Vector3(MathF.Cos(_pitch) * MathF.Cos(_yaw), MathF.Sin(_pitch), MathF.Cos(_pitch) * MathF.Sin(_yaw));

            // We need to make sure the vectors are all normalized, as otherwise we would get some funky results
            Transform.Direction = Vector3.Normalize(Transform.Direction);

            // Calculate both the right and the up vector using cross product
            // Note that we are calculating the right from the global up, this behaviour might
            // not be what you need for all cameras so keep this in mind if you do not want a FPS camera
            _right = Vector3.Normalize(Vector3.Cross(Front, Vector3.UnitY));
            _up = Vector3.Normalize(Vector3.Cross(_right, Front));
        }
        public override void Update(float dt)
        {
            base.Update(dt);
            Transform.Position = Transform.Model.ExtractTranslation();
            //Console.WriteLine(Transform.Model.ExtractTranslation());
        }
    }
}
