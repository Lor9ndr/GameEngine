#version 450 core
layout (location = 0) in vec3 aPos;

uniform highp mat4 lightSpaceMatrix;
uniform highp mat4 model;

void main()
{
    gl_Position = lightSpaceMatrix * model * vec4(aPos, 1.0);
}