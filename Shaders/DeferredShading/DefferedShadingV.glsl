#version 450 core

layout (location = 0) in vec3 Position; 

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

void main()
{          
    gl_Position = vec4(Position, 1.0) * model * view * projection;
}
