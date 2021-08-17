﻿using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.IO;

namespace GameEngine.GameObjects
{
    public class Shader
    {
        public readonly int Handle;

        public readonly Dictionary<string, int> UniformLocations;
        private Dictionary<string, int> _samplers = new Dictionary<string, int>();
        private Dictionary<string, int> _arraySamplers = new Dictionary<string, int>();
        private Dictionary<string, int> _uniformCache = new Dictionary<string, int>();
        private string _path;

        // This is how you create a simple shader.
        // Shaders are written in GLSL, which is a language very similar to C in its semantics.
        // The GLSL source is compiled *at runtime*, so it can optimize itself for the graphics card it's currently being used on.
        // A commented example of GLSL can be found in shader.vert
        public Shader(string vertPath, string fragPath, string geomPath)
        {
            // There are several different types of shaders, but the only two you need for basic rendering are the vertex and fragment shaders.
            // The vertex shader is responsible for moving around vertices, and uploading that data to the fragment shader.
            //   The vertex shader won't be too important here, but they'll be more important later.
            // The fragment shader is responsible for then converting the vertices to "fragments", which represent all the data OpenGL needs to draw a pixel.
            //   The fragment shader is what we'll be using the most here.
            _path = vertPath;
            // Load vertex shader and compile
            var shaderSource = File.ReadAllText(vertPath);

            // GL.CreateShader will create an empty shader (obviously). The ShaderType enum denotes which type of shader will be created.
            var vertexShader = GL.CreateShader(ShaderType.VertexShader);


            // Now, bind the GLSL source code
            GL.ShaderSource(vertexShader, shaderSource);

            // And then compile
            CompileShader(vertexShader);

            // We do the same for the fragment shader
            shaderSource = File.ReadAllText(fragPath);
            var fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, shaderSource);
            CompileShader(fragmentShader);

            shaderSource = File.ReadAllText(geomPath);
            int geometryShader  = GL.CreateShader(ShaderType.GeometryShader);
            GL.ShaderSource(geometryShader, shaderSource);
            CompileShader(geometryShader);


            // These 3 shaders must then be merged into a shader program, which can then be used by OpenGL.
            // To do this, create a program...
            Handle = GL.CreateProgram();

            // Attach  shaders...
            GL.AttachShader(Handle, vertexShader);
            GL.AttachShader(Handle, fragmentShader);

            GL.AttachShader(Handle, geometryShader);
            // And then link them together.
            LinkProgram(Handle);

            // When the shader program is linked, it no longer needs the individual shaders attacked to it; the compiled code is copied into the shader program.
            // Detach them, and then delete them.
            GL.DetachShader(Handle, vertexShader);
            GL.DetachShader(Handle, fragmentShader);
            GL.DetachShader(Handle, geometryShader);
            GL.DeleteShader(fragmentShader);
            GL.DeleteShader(vertexShader);
            GL.DeleteShader(geometryShader);

            // The shader is now ready to go, but first, we're going to cache all the shader uniform locations.
            // Querying this from the shader is very slow, so we do it once on initialization and reuse those values
            // later.

            // First, we have to get the number of active uniforms in the shader.
            GL.GetProgram(Handle, GetProgramParameterName.ActiveUniforms, out var numberOfUniforms);

            // Next, allocate the dictionary to hold the locations.
            UniformLocations = new Dictionary<string, int>();

            // Loop over all the uniforms,
            for (var i = 0; i < numberOfUniforms ; i++)
            {
                // get the name of this uniform,
                var key = GL.GetActiveUniform(Handle, i, out _, out _);

                // get the location,
                var location = GL.GetUniformLocation(Handle, key);

                // and then add it to the dictionary.
                UniformLocations.Add(key, location);
            }
        }
        public Shader(string vertPath, string fragPath)
        {
            // There are several different types of shaders, but the only two you need for basic rendering are the vertex and fragment shaders.
            // The vertex shader is responsible for moving around vertices, and uploading that data to the fragment shader.
            //   The vertex shader won't be too important here, but they'll be more important later.
            // The fragment shader is responsible for then converting the vertices to "fragments", which represent all the data OpenGL needs to draw a pixel.
            //   The fragment shader is what we'll be using the most here.
            _path = vertPath;
            // Load vertex shader and compile
            var shaderSource = File.ReadAllText(vertPath);

            // GL.CreateShader will create an empty shader (obviously). The ShaderType enum denotes which type of shader will be created.
            var vertexShader = GL.CreateShader(ShaderType.VertexShader);


            // Now, bind the GLSL source code
            GL.ShaderSource(vertexShader, shaderSource);
            shaderSource = File.ReadAllText(fragPath);
            // And then compile
            CompileShader(vertexShader);

            // We do the same for the fragment shader
            var fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, shaderSource);
            CompileShader(fragmentShader);
            Handle = GL.CreateProgram();


            // These two shaders must then be merged into a shader program, which can then be used by OpenGL.
            // To do this, create a program...

            // Attach both shaders...
            GL.AttachShader(Handle, vertexShader);
            GL.AttachShader(Handle, fragmentShader);

            // And then link them together.
            LinkProgram(Handle);

            // When the shader program is linked, it no longer needs the individual shaders attacked to it; the compiled code is copied into the shader program.
            // Detach them, and then delete them.
            GL.DetachShader(Handle, vertexShader);
            GL.DetachShader(Handle, fragmentShader);
            GL.DeleteShader(fragmentShader);
            GL.DeleteShader(vertexShader);

            // The shader is now ready to go, but first, we're going to cache all the shader uniform locations.
            // Querying this from the shader is very slow, so we do it once on initialization and reuse those values
            // later.

            // First, we have to get the number of active uniforms in the shader.
            GL.GetProgram(Handle, GetProgramParameterName.ActiveUniforms, out var numberOfUniforms);

            // Next, allocate the dictionary to hold the locations.
            UniformLocations = new Dictionary<string, int>();

            // Loop over all the uniforms,
            for (var i = 0; i < numberOfUniforms; i++)
            {
                // get the name of this uniform,
                var key = GL.GetActiveUniform(Handle, i, out _, out _);

                // get the location,
                var location = GL.GetUniformLocation(Handle, key);

                // and then add it to the dictionary.
                UniformLocations.Add(key, location);
            }
            {
                int cSampler = 0;

                string findCode = shaderSource;

                int i = findCode.IndexOf("uniform sampler2D");
                while (i != -1)
                {
                    findCode = findCode.Substring(i + 18);
                    int nx = findCode.IndexOf(';');

                    string var = findCode.Substring(0, nx);
                    if (var.EndsWith("]")) //Array sampler
                    {
                        nx = findCode.IndexOf('[');

                        string name = var.Substring(0, nx);

                        var = var.Substring(nx, var.Length - nx);
                        var = var.Substring(1, var.Length - 1).Substring(0, var.Length - 2);
                        int num = Convert.ToInt32(var);

                        _arraySamplers.Add(name, num);
                        cSampler += num;
                    }
                    else
                    {
                        _samplers.Add(var, cSampler);
                        cSampler++;
                    }

                    i = findCode.IndexOf("uniform sampler2D");
                }
            }

        }

        private static void CompileShader(int shader)
        {
            // Try to compile the shader
            GL.CompileShader(shader);

            // Check for compilation errors
            GL.GetShader(shader, ShaderParameter.CompileStatus, out var code);
            if (code != (int)All.True)
            {
                // We can use `GL.GetShaderInfoLog(shader)` to get information about the error.
                var infoLog = GL.GetShaderInfoLog(shader);
                throw new Exception($"Error occurred whilst compiling Shader({shader}).\n\n{infoLog}");
            }
        }

        private void LinkProgram(int program)
        {
            // We link the program
            GL.LinkProgram(program);

            // Check for linking errors
            GL.GetProgram(program, GetProgramParameterName.LinkStatus, out var code);
            if (code != (int)All.True)
            {
                // We can use `GL.GetProgramInfoLog(program)` to get information about the error.
                throw new Exception($"Error occurred whilst linking Program({program}, path- {_path})");
            }
        }

        // A wrapper function that enables the shader program.
        public void Use()
        {
            GL.UseProgram(Handle);
        }

        // The shader sources provided with this project use hardcoded layout(location)-s. If you want to do it dynamically,
        // you can omit the layout(location=X) lines in the vertex shader, and use this in VertexAttribPointer instead of the hardcoded values.
        public int GetAttribLocation(string attribName)
        {
            return GL.GetAttribLocation(Handle, attribName);
        }

        // Uniform setters
        // Uniforms are variables that can be set by user code, instead of reading them from the VBO.
        // You use VBOs for vertex-related data, and uniforms for almost everything else.

        // Setting a uniform is almost always the exact same, so I'll explain it here once, instead of in every method:
        //     1. Bind the program you want to set the uniform on
        //     2. Get a handle to the location of the uniform with GL.GetUniformLocation.
        //     3. Use the appropriate GL.Uniform* function to set the uniform.

        /// <summary>
        /// Set a uniform int on this shader.
        /// </summary>
        /// <param name="name">The name of the uniform</param>
        /// <param name="data">The data to set</param>
        public void SetInt(string name, int data)
        {
            GL.UseProgram(Handle);
            if (CheckUniformContains(name))
            {
                GL.Uniform1(UniformLocations[name], data);
            }
            /*else
            {
                Logger.Write($"Name: {name} Not Found in shader {_path}");
            }*/
        }

        /// <summary>
        /// Set a uniform float on this shader.
        /// </summary>
        /// <param name="name">The name of the uniform</param>
        /// <param name="data">The data to set</param>
        public void SetFloat(string name, float data)
        {
            GL.UseProgram(Handle);
            if (CheckUniformContains(name))
            {
                GL.Uniform1(UniformLocations[name], data);
            }
            /*else
            {
                Logger.Write($"Name: {name} Not Found in shader {_path}");
            }*/
        }

        /// <summary>
        /// Set a uniform Matrix4 on this shader
        /// </summary>
        /// <param name="name">The name of the uniform</param>
        /// <param name="data">The data to set</param>
        /// <remarks>
        ///   <para>
        ///   The matrix is transposed before being sent to the shader.
        ///   </para>
        /// </remarks>
        public void SetMatrix4(string name, Matrix4 data)
        {
            GL.UseProgram(Handle);
            if (CheckUniformContains(name))
            {
                GL.UniformMatrix4(UniformLocations[name], false, ref data);
            }
            else
            {
                GL.UniformMatrix4(GetUniformLocation(name), false, ref data);
            }
            /* else
             {
                 Logger.Write($"Name: {name} Not Found in shader {_path}");
             }*/
        }

        public bool CheckUniformContains(string uniform) => UniformLocations.ContainsKey(uniform);

        /// <summary>
        /// Set a uniform Vector3 on this shader.
        /// </summary>
        /// <param name="name">The name of the uniform</param>
        /// <param name="data">The data to set</param>
        public void SetVector3(string name, Vector3 data)
        {
            GL.UseProgram(Handle);
            if (CheckUniformContains(name))
            {
                GL.Uniform3(UniformLocations[name], data);
            }
            
            /*else
            {
                Logger.Write($"Name: {name} Not Found in shader {_path}");
            }*/

        }
        public void SetTexture(string name, int data)
        {
            if (_samplers.ContainsKey(name))
            {
                int sampler = _samplers[name];
                GL.ActiveTexture(TextureUnit.Texture0 + sampler);
                GL.BindTexture(TextureTarget.Texture2D, data);
                GL.Uniform1(GetUniformLocation(name), sampler);
            }

        }

        public void SetVector2(string name, Vector2 data)
        {
            GL.UseProgram(Handle);
            if (CheckUniformContains(name))
            {
                GL.Uniform2(UniformLocations[name], data);
            }
           /* else
            {
                Logger.Write($"Name: {name} Not Found in shader {_path}");
            }*/

        }
        public void SetVector3(string name, float x, float y, float z) => SetVector3(name, new Vector3(x, y, z));
        private int GetUniformLocation(string name)
        {
            if (_uniformCache.ContainsKey(name))
            {
                return _uniformCache[name];
            }
            else
            {
                int pos = GL.GetUniformLocation(Handle, name);
                _uniformCache.Add(name, pos);

                return pos;
            }
        }
    }
}