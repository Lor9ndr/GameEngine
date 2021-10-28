#version 330 core
layout (location = 0) in vec3 aPos;
layout (location = 1) in vec3 aNormal;
layout (location = 2) in vec2 aTexCoords;

uniform mat4 VP;    
uniform mat4 model;      

out vec2 TexCoord;                                                                 
out vec3 Nrm;                                                                   
out vec3 WorldPos;

void main()
{       
	mat3 worldRotationInverse = transpose(mat3(model));

	TexCoord      = aTexCoords;                  
    Nrm        = (worldRotationInverse * aNormal);   
    WorldPos      = (model * vec4(aPos, 1.0)).xyz;

    gl_Position  = VP* model * vec4(aPos, 1.0);
}