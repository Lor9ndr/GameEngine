

#version 450 core
out vec4 FragColor;

in vec2 TexCoords;

uniform sampler2D gPositionMap;
uniform sampler2D gColorMap;
uniform sampler2D gNormalMap;

struct Light {
    vec3 position;
    vec3 color;
    
    float linear;
    float quadratic;
    float radius;
};
const int NR_LIGHTS = 6;
uniform Light lights[NR_LIGHTS];
uniform vec3 viewPos;
uniform vec2 gScreenSize;

vec2 CalcTexCoord()
{
    return gl_FragCoord.xy / gScreenSize;
}

void main()
{             
    // retrieve data from gbuffer
    vec2 TexCoords = CalcTexCoord();
    vec3 FragPos = texture(gPositionMap, TexCoords).rgb;
    vec3 Normal = texture(gNormalMap, TexCoords).rgb;
    vec3 Diffuse = texture(gColorMap, TexCoords).rgb;
    float Specular = texture(gColorMap, TexCoords).a;
    
    // then calculate lighting as usual
    vec3 lighting  = Diffuse * 0.1; // hard-coded ambient component
    vec3 viewDir  = normalize(viewPos - FragPos);
    for(int i = 0; i < NR_LIGHTS; ++i)
    {
        // calculate distance between light source and current fragment
        float distance = length(lights[i].position - FragPos);
        if(distance < lights[i].radius)
        {
            // diffuse
            vec3 lightDir = normalize(lights[i].position - FragPos);
            vec3 diffuse = max(dot(Normal, lightDir), 0.0) * Diffuse * lights[i].color;
            // specular
            vec3 halfwayDir = normalize(lightDir + viewDir);  
            float spec = pow(max(dot(Normal, halfwayDir), 0.0), 16.0);
            vec3 specular = lights[i].color * spec * Specular;
            // attenuation
            float attenuation = 1.0 / (1.0 + lights[i].linear * distance + lights[i].quadratic * distance * distance);
            diffuse *= attenuation;
            specular *= attenuation;
            lighting += diffuse + specular;
        }
    }    
    FragColor = vec4(lighting, 1.0);
}

