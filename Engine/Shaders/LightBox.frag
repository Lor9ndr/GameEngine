#version 450 core
layout(location = 0) out vec4 FragColor;

uniform vec3 lightColor;
uniform sampler2D texture_diffuse0;
in vec2 TexCoords;

void main()
{           
	vec4 diff = texture(texture_diffuse0, TexCoords);
	if (diff.a < 0.5)
	{
		discard;
	}
	FragColor = vec4(lightColor,0.0)*diff;
}