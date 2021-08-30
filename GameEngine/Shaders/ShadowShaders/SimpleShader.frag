#version 460 core

struct Material {
	sampler2D texture_diffuse0;
	sampler2D texture_specular0;
	float     shininess;
};


struct DirLight {
	vec3 position;
	vec3 direction;
	vec3 lightColor;

	vec3 ambient;
	vec3 diffuse;
	vec3 specular;
};

struct PointLight {
	vec3 position;

	float constant;
	float linear;
	float quadratic;
	
	vec3 lightColor;
	vec3 ambient;
	vec3 diffuse;
	vec3 specular;
	samplerCube shadow;
};

struct SpotLight{
	vec3  position;
	vec3  direction;
	float cutOff;
	float outerCutOff;
	vec3 lightColor;

	vec3 ambient;
	vec3 diffuse;
	vec3 specular;

	float constant;
	float linear;
	float quadratic;
};

#define NR_POINT_LIGHTS 2
uniform SpotLight spotLight;
uniform DirLight dirLight;
uniform PointLight pointLights[NR_POINT_LIGHTS];
uniform Material material;
uniform sampler2D shadowMap;
uniform highp vec3 viewPos;
uniform float far_plane;

uniform bool shadows;

out vec4 FragColor;

in VS_OUT {
	vec3 FragPos;
	vec3 Normal;
	vec2 TexCoords;
	vec4 FragPosLightSpace;
} fs_in;

float near = 0.01; 
float far  = 100.0; 

float LinearizeDepth(float depth);
vec3 CalcDirLight(DirLight light, vec3 normal, vec3 viewDir, vec3 color);
vec3 CalcPointLight(PointLight light, vec3 normal, vec3 fragPos, vec3 viewDir, vec3 color, int idx);
vec3 CalcSpotLight(SpotLight light, vec3 normal, vec3 fragPos, vec3 viewDir, vec3 color);
float ShadowPointCalculation(vec3 fragPos, PointLight light,int idx);
float ShadowDirectCalculation(vec4 fragPosLightSpace, DirLight light);
vec3 gridSamplingDisk[20] = vec3[]
(
   vec3(1, 1,  1), vec3( 1, -1,  1), vec3(-1, -1,  1), vec3(-1, 1,  1), 
   vec3(1, 1, -1), vec3( 1, -1, -1), vec3(-1, -1, -1), vec3(-1, 1, -1),
   vec3(1, 1,  0), vec3( 1, -1,  0), vec3(-1, -1,  0), vec3(-1, 1,  0),
   vec3(1, 0,  1), vec3(-1,  0,  1), vec3( 1,  0, -1), vec3(-1, 0, -1),
   vec3(0, 1,  1), vec3( 0, -1,  1), vec3( 0, -1, -1), vec3( 0, 1, -1)
);


void main()
{    
	// properties
	vec3 norm = normalize(fs_in.Normal);
	vec3 viewDir = normalize(viewPos - fs_in.FragPos);

	vec4 textureColour = texture(material.texture_diffuse0, fs_in.TexCoords);
	vec3 color =  textureColour.rgb;
	// phase 1: directional lighting
	//vec3 result = CalcDirLight(dirLight, norm, viewDir, color);
	vec3 result = vec3(0);
	// phase 2: point lights
	for(int i = 0; i < NR_POINT_LIGHTS; i++)
		result += CalcPointLight(pointLights[i], norm, fs_in.FragPos, viewDir, color, i);    
	// phase 3: spot light
	//result += CalcSpotLight(spotLight, norm, fs_in.FragPos, viewDir,color); 
	
	FragColor = vec4(result, 1.0);
}

// calculates the color when using a directional light.
vec3 CalcDirLight(DirLight light, vec3 normal, vec3 viewDir, vec3 color,int i )
{
	float specColor = texture(material.texture_specular0, fs_in.TexCoords).r;
	vec3 lightColor = light.lightColor;
	// ambient
	vec3 ambient = light.ambient * color;
	// diffuse
	vec3 lightDir = normalize(light.position - fs_in.FragPos);
	float diff = max(dot(lightDir, normal), 0.0);
	vec3 diffuse = diff * lightColor;
	// specular
	vec3 reflectDir = reflect(-lightDir, normal);
	float spec = 0.0;
	vec3 halfwayDir = normalize(lightDir + viewDir);  
	spec = pow(max(dot(normal, halfwayDir), 0.0), 64.0);
	vec3 specular = spec * lightColor;    
	// calculate shadow
	float shadow =  shadows ? ShadowDirectCalculation(fs_in.FragPosLightSpace, light) : 0.0;                      
	vec3 lighting = (texture(material.texture_diffuse0, fs_in.TexCoords).rgb * 
	(diffuse * (1.0f - shadow) + ambient) + specColor * specular  * (1.0f - shadow))* light.lightColor; 
	return lighting;
}

// calculates the color when using a point light.
vec3 CalcPointLight(PointLight light, vec3 normal, vec3 fragPos, vec3 viewDir,vec3 color, int idx)
{
	float specColor = texture(material.texture_specular0, fs_in.TexCoords).r;
	vec3 lightColor = light.lightColor;
	// ambient
	vec3 ambient =  light.ambient * color;
	// diffuse
	vec3 lightDir = normalize(light.position - fs_in.FragPos);
	float diff = max(dot(lightDir, normal), 0.0);
	vec3 diffuse = diff * lightColor;
	// specular
	vec3 reflectDir = reflect(-lightDir, normal);
	float spec = 0.0;
	vec3 halfwayDir = normalize(lightDir + viewDir);  
	spec = pow(max(dot(normal, halfwayDir), 0.0), 32.0);
	vec3 specular = spec * lightColor;    
	// calculate shadow
	float shadow = shadows ? ShadowPointCalculation(fs_in.FragPos, light,idx) : 0.0;                      
	return (color *(diffuse * (1.0f - shadow) + ambient) +  specColor * specular  * (1.0f - shadow) + ambient) * lightColor; 
}

// calculates the color when using a spot light.
vec3 CalcSpotLight(SpotLight light, vec3 normal, vec3 fragPos, vec3 viewDir, vec3 color)
{
	// controls how big the area that is lit up is
	float outerCone = 0.90f;
	float innerCone = 0.95f;

	// ambient lighting
	float ambient = 0.20f;

	// diffuse lighting
	vec3 lightDirection = light.direction;// normalize(light.direction - fragPos);
	float diffuse = max(dot(normal, lightDirection), 0.0f);

	// specular lighting
	float specular = 0.0f;
	if (diffuse != 0.0f)
	{
		float specularLight = 0.50f;
		vec3 viewDirection = normalize(viewPos - fragPos);
		vec3 halfwayVec = normalize(viewDirection + lightDirection);
		float specAmount = pow(max(dot(normal, halfwayVec), 0.0f), 16);
		specular = specAmount * specularLight;
	};

	// calculates the intensity of the crntPos based on its angle to the center of the light cone
	float angle = dot(vec3(0.0f, -1.0f, 0.0f), -lightDirection);
	float inten = clamp((angle - outerCone) / (innerCone - outerCone), 0.0f, 1.0f);


	// Shadow value
	float shadow = 0.0f;
	// Sets lightCoords to cull space
	vec3 lightCoords = fs_in.FragPosLightSpace.xyz / fs_in.FragPosLightSpace.w;
	if(lightCoords.z <= 1.0f)
	{
		// Get from [-1, 1] range to [0, 1] range just like the shadow map
		lightCoords = (lightCoords + 1.0f) / 2.0f;
		float currentDepth = lightCoords.z;
		// Prevents shadow acne
		float bias = max(0.00025f * (1.0f - dot(normal, lightDirection)), 0.000005f);

		// Smoothens out the shadows
		int sampleRadius = 2;
		vec2 pixelSize = 1.0 / textureSize(shadowMap, 0);
		for(int y = -sampleRadius; y <= sampleRadius; y++)
		{
			for(int x = -sampleRadius; x <= sampleRadius; x++)
			{
				float closestDepth = texture(shadowMap, lightCoords.xy + vec2(x, y) * pixelSize).r;
				if (currentDepth > closestDepth + bias)
					shadow += 1.0f;     
			}    
		}
		// Get average shadow
		shadow /= pow((sampleRadius * 2 + 1), 2);

	}
	return (texture(material.texture_diffuse0, fs_in.TexCoords).rgb * (diffuse * (1.0f - shadow) * inten + ambient) + texture(material.texture_specular0, fs_in.TexCoords).r * specular * (1.0f - shadow) * inten) * light.lightColor;
}


float ShadowPointCalculation(vec3 fragPos, PointLight light, int idx)
{
	// get vector between fragment position and light position
	vec3 fragToLight = fragPos - light.position;
	// use the fragment to light vector to sample from the depth map    
	// float closestDepth = texture(shadowCubeMap, fragToLight).r;
	// it is currently in linear range between [0,1], let's re-transform it back to original depth value
	// closestDepth *= far_plane;
	// now get current linear depth as the length between the fragment and light position
	float currentDepth = length(fragToLight);
	// test for shadows
	// float bias = 0.05; // we use a much larger bias since depth is now in [near_plane, far_plane] range
	// float shadow = currentDepth -  bias > closestDepth ? 1.0 : 0.0;
	// PCF
	 float shadow = 0.0;
	 float bias = 0.05; 
	 float samples = 4.0;
	 float offset = 0.1;
	 for(float x = -offset; x < offset; x += offset / (samples * 0.5))
	 {
		for(float y = -offset; y < offset; y += offset / (samples * 0.5))
		{
			for(float z = -offset; z < offset; z += offset / (samples * 0.5))
			{
				float closestDepth = texture(light.shadow, fragToLight + vec3(x, y, z)).r; // use lightdir to lookup cubemap
				closestDepth *= far_plane;   // Undo mapping [0;1]
				if(currentDepth - bias > closestDepth)
					shadow += 1.0;
			}
		}
	 }
	 shadow /= (samples * samples * samples);
	 return shadow;
	

}

float ShadowDirectCalculation(vec4 fragPosLightSpace, DirLight light)
{
	// perform perspective divide
	vec3 projCoords = fragPosLightSpace.xyz / fragPosLightSpace.w;
	// transform to [0,1] range
	projCoords = projCoords * 0.5 + 0.5;
	// get closest depth value from light's perspective (using [0,1] range fragPosLight as coords)
	float closestDepth = texture(shadowMap, projCoords.xy).r; 
	// get depth of current fragment from light's perspective
	float currentDepth = projCoords.z;
	// calculate bias (based on depth map resolution and slope)
	vec3 normal = normalize(fs_in.Normal);
	vec3 lightDir = normalize(light.direction - fs_in.FragPos);
	float bias = max(0.05 * (1.0 - dot(normal, lightDir)), 0.005);
	// check whether current frag pos is in shadow
	// float shadow = currentDepth - bias > closestDepth  ? 1.0 : 0.0;
	// PCF
	float shadow = 0.0;
	vec2 texelSize = 1.0 / textureSize(shadowMap, 0);
	for(int x = -1; x <= 1; ++x)
	{
		for(int y = -1; y <= 1; ++y)
		{
			float pcfDepth = texture(shadowMap, projCoords.xy + vec2(x, y) * texelSize).r; 
			shadow += currentDepth - bias > pcfDepth  ? 1.0 : 0.0;        
		}    
	}
	shadow /= 9.0;
	
	// keep the shadow at 0.0 when outside the far_plane region of the light's frustum.
	//if(projCoords.z > 1.0)
		shadow = 0.0;
		
	return shadow;
}