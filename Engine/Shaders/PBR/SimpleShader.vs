#version 450 core
layout (location = 0) in vec3 aPos;
layout (location = 1) in vec3 aNormal;
layout (location = 2) in vec2 aTexCoords;
layout (location = 3) in vec3 aTangent;
layout (location = 4) in vec3 aBitangent;


out VS_OUT {
    vec3 Normal;
    vec2 TexCoords;
    vec3 WorldPos;
} vs_out;

uniform mat4 projection;
uniform mat4 view;
uniform mat4 model;

void main()
{
    vs_out.TexCoords = aTexCoords;
    vs_out.Normal = mat3(model) * aNormal;   
    vs_out.WorldPos = vec3(model * vec4(aPos, 1.0));

    gl_Position =  projection * view * vec4( vs_out.WorldPos, 1.0);

}