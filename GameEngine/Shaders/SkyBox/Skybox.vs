﻿#version 450 core
layout (location = 0) in vec3 aPos;

out vec3 TexCoords;

uniform mat4 viewproj;

void main()
{
    TexCoords = aPos;
    vec4 pos = vec4(aPos, 1.0) * viewproj;
    gl_Position = pos.xyww;
}  


