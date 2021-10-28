#version 330 core
out vec4 LightMap;

uniform vec2 ScreenSize;

uniform float LightRadius;
uniform vec3 LightColor;
uniform float LightIntensity;
uniform vec3 LightCenter;

uniform sampler2D PositionBuffer;
uniform sampler2D NormalBuffer;

void main()
{
    vec2 texCoord = gl_FragCoord.xy / ScreenSize;

    vec3 pixelPos = texture2D(PositionBuffer, texCoord).xyz;
    vec3 pixelNormal = normalize(texture2D(NormalBuffer, texCoord).xyz);

    float alpha = length(pixelNormal);
    if (alpha < 0.1)
    {
        LightMap = vec4(0, 0, 0, 0);
        return;
    }

    vec3 toLight = LightCenter - pixelPos;

    float attenuation = clamp(1.0 - length(toLight) / LightRadius, 0.0, 1.0);

    toLight = normalize(toLight);

    float nDotL = max(dot(pixelNormal, toLight), 0.0);

    vec3 diffuseLight = LightColor * nDotL;

    LightMap = LightIntensity * attenuation * vec4(diffuseLight, alpha);
}