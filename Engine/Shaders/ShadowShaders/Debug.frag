#version 460 core
out vec4 FragColor;

in vec2 TexCoords;

uniform sampler2D fboAttachment;
uniform float near_plane;
uniform float far_plane;


void main()
{             
    FragColor = texture(fboAttachment, TexCoords);
}