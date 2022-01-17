#version 330 core
in vec2 TexCoord;                                                                  
in vec3 Nrm;                                                                    
in vec3 WorldPos;    

layout (location = 0) out vec3 WorldPosOut;   
layout (location = 1) out vec3 DiffuseOut;     
layout (location = 2) out vec3 NormalOut; 
layout (location = 3) out vec4 LightOut; 

uniform sampler2D diffuse;              

uniform vec3 AmbientDirection;
uniform vec3 AmbientColor;
uniform float AmbientPower;  

void main()
{
    DiffuseOut      = texture2D(diffuse, TexCoord).xyz;
    WorldPosOut     = WorldPos;
    NormalOut       = normalize(Nrm);

    float dt = 1.0 - (max(dot(NormalOut, -AmbientDirection),0.0) * 0.5);

    LightOut = vec4(AmbientColor * AmbientPower * dt, 1);
}
