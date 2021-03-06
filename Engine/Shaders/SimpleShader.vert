#version 460 core
layout (location = 0) in vec3 aPos;
layout (location = 1) in vec3 aNormal;
layout (location = 2) in vec2 aTexCoords;
layout (location = 3) in vec3 aTangent;
layout (location = 4) in vec3 aBitangent;
layout (location = 5) in ivec4 boneIds;
layout (location = 6) in vec4 weights;


out VS_OUT {
    vec3 FragPos;
    vec2 TexCoords;
    vec3 Normal;
    mat3 TBN;
} vs_out;


uniform mat4 model;
uniform bool reverse_normals;

//Animation
const int MAX_BONES = 100;
const int MAX_BONE_INFLUENCE = 4;
uniform mat4 finalBonesMatrices[MAX_BONES];

uniform mat4 view;
uniform mat4 projection;

uniform vec3 viewPos;

void main()
{

    vec4 totalPosition = vec4(0.0f);
    vec3 localNormal = vec3(0.0f);
    if(boneIds[0] != -1)
    {
        for(int i = 0 ; i < MAX_BONE_INFLUENCE ; i++)
        {
            if(boneIds[i] == -1) 
                continue;
            if(boneIds[i] >= MAX_BONES) 
            {
                totalPosition = vec4(aPos,1.0f);
                break;
            }
            vec4 localPosition = finalBonesMatrices[boneIds[i]] * vec4(aPos,1.0f);
            totalPosition +=  localPosition * weights[i];
            localNormal +=  aNormal * mat3(finalBonesMatrices[boneIds[i]]);
        }
        //vs_out.FragPos = vec3(model * vec4(totalPosition.xyz, 1.0f));
    }
    else
    {
        totalPosition = vec4(aPos,1.0f);
        localNormal = aNormal;
    }
    vec4 modelxTotalPos = model * vec4(totalPosition.xyz, 1.0f);
    vs_out.FragPos = vec3(modelxTotalPos);
    vs_out.TexCoords = aTexCoords;
    vs_out.Normal =  transpose(inverse(mat3(model))) * localNormal;
    vec3 T = normalize(vec3(model * vec4(aTangent, 0.0)));
    vec3 N = normalize(vec3(model * vec4(localNormal, 0.0)));
    // re-orthogonalize T with respect to N
    T = normalize(T - dot(T, N) * N);
    // then retrieve perpendicular vector B with the cross product of T and N
    vec3 B = cross(N, T);

    mat3 TBN = mat3(T, B, N);
    vs_out.TBN = TBN;

    gl_Position = modelxTotalPos;
}