#version 330 core
out vec4 FragColor;       

in vec2 TexCoord;

uniform sampler2D ColorBuffer;
uniform sampler2D LightBuffer;

void main()
{
    vec4 color = texture2D(ColorBuffer, TexCoord);

    if(color.a == 0.0)
        discard;

    FragColor = color * texture2D(LightBuffer, TexCoord);
}