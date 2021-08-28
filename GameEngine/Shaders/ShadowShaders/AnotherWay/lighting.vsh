layout(location = 0) in vec3 aPos;
layout(location = 1) in vec3 aNormal;
layout(location = 2) in vec2 aTexCoords;
layout(location = 3) in vec3 aTangent;
layout(location = 4) in vec3 aBitangent;

varying vec2 texCoord0;
varying vec3 worldPos0;
varying vec4 shadowMapCoords0;
varying mat3 tbnMatrix;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;
uniform mat4 lightSpaceMatrix;

void main()
{
    gl_Position = projection * view * model * vec4(aPos, 1.0)
    texCoord0 = texCoord; 
    shadowMapCoords0 = lightSpaceMatrix * vec4(aPos, 1.0);
    worldPos0 = (model * vec4(aPos, 1.0)).xyz;
    
    vec3 n = normalize((model * vec4(aNormal, 0.0)).xyz);
    vec3 t = normalize((model * vec4(aTangent, 0.0)).xyz);
    t = normalize(t - dot(t, n) * n);
    
    vec3 biTangent = cross(t, n);
    tbnMatrix = mat3(t, aBitangent, n);
}
