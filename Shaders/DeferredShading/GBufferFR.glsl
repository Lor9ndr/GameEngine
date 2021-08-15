#version 450 core
layout (location = 0) out vec3 WorldPosOut;   
layout (location = 1) out vec3 DiffuseOut;     
layout (location = 2) out vec3 NormalOut;     
layout (location = 3) out vec3 TexCoordOut;    

in vec2 TexCoords;
in vec3 FragPos;
in vec3 Normal;

uniform sampler2D gColorMap;



void main()
{    
	WorldPosOut     = FragPos;					
	DiffuseOut      = texture(gColorMap, TexCoords).xyz;	
	NormalOut       = normalize(Normal);					
	TexCoordOut     = vec3(TexCoords, 0.0);	
}

