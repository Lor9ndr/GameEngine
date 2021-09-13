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
	
	mat4 lightSpaceMatrix;
	sampler2D shadow;
	float farPlane;
	float intensity;
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
	float farPlane;
	float intensity;
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

	mat4 lightSpaceMatrix;
	sampler2D shadow;
	float farPlane;
	float intensity;
};


#define NUM_SAMPLES 20
#define NUM_SAMPLESF 20.0
#define MAX_POINT_LIGHTS 5
#define MAX_SPOT_LIGHTS 10

uniform int nrSpotLights;
uniform int nrPointLights;
uniform SpotLight spotLights[MAX_SPOT_LIGHTS];
uniform DirLight dirLight;
uniform PointLight pointLights[MAX_POINT_LIGHTS];
uniform Material material;
uniform vec3 viewPos;
uniform bool gammaEnable;

uniform bool shadows;

out vec4 FragColor;

in VS_OUT {
	highp vec3 FragPos;
	highp vec3 Normal;
	highp vec2 TexCoords;
} fs_in;

float near = 0.1; 
float far  = 1000.0; 

float LinearizeDepth(float depth);
vec3 CalcDirLight(DirLight light, vec3 normal, vec3 viewDir,  vec3 diffColor, vec3 specColor);

vec3 CalcPointLight(PointLight light, vec3 normal,  vec3 viewDir, vec3 diffColor, vec3 specColor);

vec3 CalcSpotLight(SpotLight light, vec3 normal, vec3 fragPos, vec3 viewDir,vec3 diffColor, vec3 specColor);

float ShadowPointCalculation(PointLight light,vec3 normal, vec3 lightDir);
float ShadowDirectCalculation(DirLight light, vec3 normal, vec3 lightDir);
float SpotLightShadowCalculation(SpotLight light, vec3 normal, vec3 lightDir);

vec3 sampleOffsetDirections[NUM_SAMPLES] = vec3[]
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
	float gamma = 0.9;

	vec3 diffColor = texture(material.texture_diffuse0, fs_in.TexCoords).rgb;
	vec3 specColor = texture(material.texture_specular0, fs_in.TexCoords).rgb;

	// phase 1: directional lighting
	vec3 result = CalcDirLight(dirLight, norm, viewDir, diffColor, specColor);
	//// phase 2: point lights
	//vec3 result = vec3(0);
	//result += CalcPointLight(pointLights[0], norm,  viewDir, diffColor, specColor);    
	for(int i = 0; i < nrPointLights; i++)
		result += CalcPointLight(pointLights[i], norm,  viewDir, diffColor, specColor);    
	// phase 3: spot light
	for(int i = 0; i < nrSpotLights; i++)
		result += CalcSpotLight(spotLights[i], norm, fs_in.FragPos, viewDir,diffColor, specColor); 
	
	
	float z = gl_FragCoord.z * 2.0 - 1.0; // transform to NDC [0, 1] => [-1, 1]
	float linearDepth = (2.0 * near * far) / (z * (far - near) - (far + near)); // take inverse of the projection matrix (perspective)
	float factor = (near + linearDepth) / (near - far); // convert back to [0, 1]
	
	result.rgb *= 1.0 -factor;
	if(gammaEnable)
	{
		result = pow(result, vec3(1.0/gamma));
	}
	FragColor = vec4(result, 1.0);
}

// calculates the color when using a directional light.
vec3 CalcDirLight(DirLight light, vec3 normal, vec3 viewDir, vec3 diffColor, vec3 specColor)
{
	// ambient
	vec3 ambient = light.ambient * diffColor;

	// diffuse
	vec3 lightDir =  normalize(light.position - fs_in.FragPos);
	float diff = max(dot(normal, lightDir), 0.0);
	vec3 diffuse = light.diffuse * (diff * diffColor);

	// specular
	vec3 specular = vec3(0.0, 0.0, 0.0);
	if (diff > 0) {
		// if diff <= 0, object is "behind" light

		float dotProd = 0.0;
		// calculate using Blinn-Phong model
		vec3 halfwayDir = normalize(lightDir + viewDir);
		dotProd = dot(normal, halfwayDir);

		float spec = pow(max(dotProd, 0.0), material.shininess * 128);
		specular = light.specular * (spec * specColor);
	}
	
	// calculate shadow
	float shadow =  shadows ? ShadowDirectCalculation(light, normal, lightDir) : 0.0;
	return (ambient + (1.0 - shadow) * (diffuse + specular)) * light.lightColor * light.intensity; 
}

// calculates the color when using a point light.
vec3 CalcPointLight(PointLight light, vec3 normal, vec3 viewDir,vec3 diffColor, vec3 specColor)
{
	vec3 ambient = light.ambient * diffColor;

	// diffuse
	vec3 lightDir = normalize(light.position - fs_in.FragPos);
	float diff = max(dot(normal, lightDir), 0.0);
	vec3 diffuse = light.diffuse * (diff * diffColor);

	// specular
	vec3 specular = vec3(0.0);
	if (diff > 0) {
		// if diff <= 0, object is "behind" light

		float dotProd = 0.0;
		// calculate using Blinn-Phong model
		vec3 halfwayDir = normalize(lightDir + viewDir);
		dotProd = dot(normal, halfwayDir);

		float spec = pow(max(dotProd, 0.0), material.shininess * 128);
		specular = light.specular * (spec * specColor);
	}

	// calculate attenuation
	float dist = length(light.position - fs_in.FragPos);
	float attenuation = 1.0 / (light.constant + light.linear * dist + light.quadratic * (dist * dist));
	ambient *= attenuation;
	diffuse *= attenuation;
	specular *= attenuation;

	float shadow = shadows ? ShadowPointCalculation(light,normal,lightDir) : 0.0;     
	
	return (ambient + (1.0 - shadow)) * (diffuse + specular)* light.lightColor*light.intensity; 
}

// calculates the color when using a spot light.



float ShadowPointCalculation(PointLight light,vec3 normal, vec3 lightDir)
{
	// get vector from the light to the fragment
	vec3 lightToFrag = fs_in.FragPos - light.position;

	// get current depth
	float currentDepth = length(lightToFrag);

	// calculate bias
	float minBias = 0.005;
	float maxBias = 0.05;
	float bias = max(maxBias * (1.0 - dot(normal, lightDir)), minBias);

	// PCF
	float shadow = 0.0;
	float viewDist = length(viewPos);
	float diskRadius = (1.0 + (viewDist / light.farPlane)) / 30.0;
	for (int i = 0; i < NUM_SAMPLES; i++) {
		float pcfDepth = texture(light.shadow, lightToFrag + sampleOffsetDirections[i] * diskRadius).r;
		pcfDepth *= light.farPlane;

		if (currentDepth - bias > pcfDepth) {
			shadow += 1.0;
		}
	}

	shadow /= NUM_SAMPLESF;

	return shadow;
	

}

float ShadowDirectCalculation(DirLight light, vec3 normal, vec3 lightDir)
{
  // perform perspective divide
	vec4 fragPosLightSpace = light.lightSpaceMatrix * vec4(fs_in.FragPos, 1.0);
    vec3 projCoords = fragPosLightSpace.xyz / fragPosLightSpace.w;
    // transform to [0,1] range
    projCoords = projCoords * 0.5 + 0.5;
    // get closest depth value from light's perspective (using [0,1] range fragPosLight as coords)
    float closestDepth = texture(light.shadow, projCoords.xy).r; 
    // get depth of current fragment from light's perspective
    float currentDepth = projCoords.z;
    // calculate bias (based on depth map resolution and slope)
    float bias = max(0.05 * (1.0 - dot(normal, lightDir)), 0.05);
    // check whether current frag pos is in shadow
    // float shadow = currentDepth - bias > closestDepth  ? 1.0 : 0.0;
    // PCF
    float shadow = 0.0;
    vec2 texelSize = 1.0 / textureSize(light.shadow, 0);
    for(int x = -1; x <= 1; ++x)
    {
        for(int y = -1; y <= 1; ++y)
        {
            float pcfDepth = texture(light.shadow, projCoords.xy + vec2(x, y) * texelSize).r; 
            shadow += currentDepth - bias > pcfDepth  ? 1.0 : 0.0;        
        }    
    }
    shadow /= 9.0;
    
    // keep the shadow at 0.0 when outside the far_plane region of the light's frustum.
    if(projCoords.z > 1.0)
        shadow = 0.0;
        
    return shadow;
}

vec3 CalcSpotLight(SpotLight light, vec3 normal, vec3 fragPos, vec3 viewDir,vec3 diffColor, vec3 specColor)
{
    // Ambient
    vec3 ambient = light.ambient * diffColor;
    
    // Diffuse
    vec3 lightDir = normalize(light.position - fragPos);
    float diff = max(dot(normal, lightDir), 0.0);
    vec3 diffuse = light.diffuse * diff * diffColor;
    
    // Specular
    vec3 reflectDir = reflect(-lightDir, normal);
    float spec = pow(max(dot(viewDir, reflectDir), 0.0), material.shininess);
    vec3 specular = light.specular * spec * specColor;
    
    // Spotlight (soft edges)
    float theta = dot(lightDir, normalize(-light.direction));
    float epsilon = (light.cutOff - light.outerCutOff);
    float intensity = clamp((theta - light.outerCutOff) / epsilon, 0.0, 1.0);
    diffuse  *= intensity;
    specular *= intensity;
    
    // Attenuation
    float distance  = length(light.position - fragPos);
    float attenuation = 1.0f / (light.constant + light.linear * distance + light.quadratic * (distance * distance));
    ambient  *= attenuation;
    diffuse  *= attenuation;
    specular *= attenuation;
	float shadow = shadows ? SpotLightShadowCalculation(light, normal, lightDir) : 0.0;
		//float shadow = 0.0;

	return (ambient + (1.0 - shadow)) * (diffuse + specular) * light.lightColor * light.intensity * 100;
}

float SpotLightShadowCalculation(SpotLight light, vec3 normal, vec3 lightDir)
{
	// perform perspective divide
	vec4 fragPosLightSpace = light.lightSpaceMatrix * vec4(fs_in.FragPos, 1.0);
    vec3 projCoords = fragPosLightSpace.xyz / fragPosLightSpace.w;
    // transform to [0,1] range
    projCoords = projCoords * 0.5 + 0.5;
    // get closest depth value from light's perspective (using [0,1] range fragPosLight as coords)
    float closestDepth = texture(light.shadow, projCoords.xy).r; 
    // get depth of current fragment from light's perspective
    float currentDepth = projCoords.z;
    // calculate bias (based on depth map resolution and slope)
    float bias = max(0.05 * (1.0 - dot(normal, lightDir)), 0.05);
    // check whether current frag pos is in shadow
    // float shadow = currentDepth - bias > closestDepth  ? 1.0 : 0.0;
    // PCF
    float shadow = 0.0;
    vec2 texelSize = 1.0 / textureSize(light.shadow, 0);
	if (fragPosLightSpace.w > 1)
	{
		 for(int x = -1; x <= 1; ++x)
		{
			for(int y = -1; y <= 1; ++y)
			{
				float pcfDepth = texture(light.shadow, projCoords.xy + vec2(x, y) * texelSize).r; 
				shadow += currentDepth - bias > pcfDepth  ? 1.0 : 0.0;        
			}    
		}
		shadow /= 9.0;
	}
   
    
    // keep the shadow at 0.0 when outside the far_plane region of the light's frustum.
    if(projCoords.z > 1.0)
        shadow = 0.0;
        
    return shadow;
}
