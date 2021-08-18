﻿#version 450 core
layout (location = 0) in highp vec3 aPos;
layout (location = 1) in highp vec3 aNormal;
layout (location = 2) in highp vec2 aTexCoords;
layout (location = 3) in vec3 aTangent;
layout (location = 4) in vec3 aBitangent;


out VS_OUT {
    highp vec3  FragPos;
    highp vec3 Normal;
    highp vec2 TexCoords;
    highp vec4 FragPosLightSpace;
} vs_out;

uniform highp mat4 projection;
uniform highp mat4 view;
uniform highp mat4 model;
uniform highp mat4 lightSpaceMatrix;
uniform bool reverse_normals;

void main()
{
    vs_out.FragPos = vec3(model * vec4(aPos, 1.0));
     if(reverse_normals) // a slight hack to make sure the outer large cube displays lighting from the 'inside' instead of the default 'outside'.
        vs_out.Normal = transpose(inverse(mat3(model))) * (-1.0 * aNormal);
    else
        vs_out.Normal = transpose(inverse(mat3(model))) * aNormal;
    vs_out.TexCoords = aTexCoords;
    vs_out.FragPosLightSpace = lightSpaceMatrix * vec4(vs_out.FragPos, 1.0);
    gl_Position =  projection * view * model * vec4(aPos, 1.0) ;
}