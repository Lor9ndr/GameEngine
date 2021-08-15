﻿using GameEngine.GameObjects.Base;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine.GameObjects.Lights
{
    public class Light : AMovable
    {
        public Vector3 LightColor;
        public Vector3 Ambient;
        public Vector3 Diffuse;

        protected Mesh _mesh;
        public Light(Mesh mesh, Vector3 position, Vector3 ambient, Vector3 diffuse,Vector3 lightColor,
             Vector3 direction= default, Vector3 rotation = default, Vector3 scale = default, float velocity = default, Matrix4 model = default) 
            : base(position, direction, rotation, scale, velocity, model)
        {
            _mesh = mesh;
            LightColor = lightColor;
            Ambient = ambient;
            Diffuse = diffuse;

        }
        public virtual void SetupModel(Shader shader)
        {
            var t2 = Matrix4.CreateTranslation(Position);
            var r1 = Matrix4.CreateRotationX(Rotation.X);
            var r2 = Matrix4.CreateRotationY(Rotation.Y);
            var r3 = Matrix4.CreateRotationZ(Rotation.Z);
            var s = Matrix4.CreateScale(Scale);
            _model = r1 * r2 * r3 * s * t2;
            shader.SetMatrix4("model", _model);
        }
        public virtual void Render(Shader shader)
        {
            shader.Use();
            SetupModel(shader);
            _mesh.Draw();
        }
        public void SetAmbient(Vector3 ambient)
        {
            Ambient += ambient;
        }
    }
}
