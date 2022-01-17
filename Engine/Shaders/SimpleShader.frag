#version 460 core

struct Material {
	sampler2D Diffuse;
	sampler2D Specular;
	sampler2D Normal;
	sampler2D Metallic;
	sampler2D Roughness;
	sampler2D Ao;
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
#define MAX_POINT_LIGHTS 10
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

in GS_OUT {
	vec3 FragPos;
	vec2 TexCoords;
	vec3 Normal;
	mat3 TBN;
} fs_in;

const float near = 0.1; 
const float far  = 1000.0; 
const float PI = 3.14159265359;

float LinearizeDepth(float depth);
vec3 CalcDirLight(DirLight light, vec3 normal, vec3 viewDir,  vec3 diffColor, vec3 specColor,vec3 F0, float roughness, float metallic);

vec3 CalcPointLight(PointLight light, vec3 normal,  vec3 viewDir, vec3 diffColor, vec3 specColor,vec3 F0, float roughness, float metallic);

vec3 CalcSpotLight(SpotLight light, vec3 normal, vec3 fragPos, vec3 viewDir,vec3 diffColor, vec3 specColor, vec3 F0, float roughness,  float metallic);

float ShadowPointCalculation(PointLight light,vec3 normal, vec3 lightDir);
float ShadowDirectCalculation(DirLight light, vec3 normal, vec3 lightDir);
float SpotLightShadowCalculation(SpotLight light, vec3 normal, vec3 lightDir);
  
float DistributionGGX(vec3 N, vec3 H, float roughness);
float GeometrySchlickGGX(float NdotV, float roughness);
float GeometrySmith(vec3 N, vec3 V, vec3 L, float roughness);
vec3 fresnelSchlick(float cosTheta, vec3 F0);

vec3 sampleOffsetDirections[NUM_SAMPLES] = vec3[]
(
   vec3(1, 1,  1), vec3( 1, -1,  1), vec3(-1, -1,  1), vec3(-1, 1,  1), 
   vec3(1, 1, -1), vec3( 1, -1, -1), vec3(-1, -1, -1), vec3(-1, 1, -1),
   vec3(1, 1,  0), vec3( 1, -1,  0), vec3(-1, -1,  0), vec3(-1, 1,  0),
   vec3(1, 0,  1), vec3(-1,  0,  1), vec3( 1,  0, -1), vec3(-1, 0, -1),
   vec3(0, 1,  1), vec3( 0, -1,  1), vec3( 0, -1, -1), vec3( 0, 1, -1)
);
uniform bool EnableLight;
uniform bool EnableDistanceFOG;
uniform bool useNormalMapping;

void main()
{    
	// properties
	vec3 diffColor = texture(material.Diffuse, fs_in.TexCoords).rgb; // pow 2.x2
	diffColor.x = pow(diffColor.x,2.2);
	diffColor.y = pow(diffColor.y,2.2);
	diffColor.z = pow(diffColor.z,2.2);
	vec3 specColor = texture(material.Specular, fs_in.TexCoords).rgb;
    float metallic  = texture(material.Metallic, fs_in.TexCoords).r;
    float roughness = texture(material.Roughness, fs_in.TexCoords).r;
    float ao        = texture(material.Ao, fs_in.TexCoords).r;

	float gamma = 0.9;
	vec3 normal;
	vec3 viewDir;
	if(useNormalMapping)
	{
		normal =  texture(material.Normal, fs_in.TexCoords).rgb;
		normal = normalize(normal * 2.0 - 1.0);
		normal = normalize(fs_in.TBN * normal);
	}
	else
	{
		 normal = normalize(fs_in.Normal);
	}
	vec3 F0 = vec3(0.04); 
    F0 = mix(F0, diffColor, metallic);

	viewDir = normalize(viewPos - fs_in.FragPos);
	vec3 result = vec3(0);
	if(EnableLight)
	{
		// phase 1: directional lighting
		result = CalcDirLight(dirLight, normal, viewDir, diffColor, specColor,F0, roughness, metallic);
		// phase 2: point lights
		for(int i = 0; i < nrPointLights; i++)
			result += CalcPointLight(pointLights[i], normal,  viewDir, diffColor, specColor, F0, roughness, metallic);
		// phase 3: spot light
		for(int i = 0; i < nrSpotLights; i++)
			result += CalcSpotLight(spotLights[i], normal, fs_in.FragPos, viewDir,diffColor, specColor, F0, roughness, metallic); 
	}
	else
	{
		result = (diffColor) * 0.5;
	}
	if(EnableDistanceFOG)
	{
			float z = gl_FragCoord.z * 2.0 - 1.0; // transform to NDC [0, 1] => [-1, 1]
			float linearDepth = (2.0 * near * far) / (z * (far - near) - (far + near)); // take inverse of the projection matrix (perspective)
			float factor = (near + linearDepth) / (near - far); // convert back to [0, 1]
			result.rgb *= 1.0 - factor;
	}
	if(gammaEnable)
	{
		result = pow(result, vec3(1.0/gamma));
	}
	vec3 ambient = vec3(0.03) * diffColor * ao;
    vec3 color = ambient + result;
	
    color = color / (color + vec3(1.0));
    color = pow(color, vec3(1.0/2.2));  
   
    FragColor = vec4(color, 1.0);
}

// calculates the color when using a directional light.
vec3 CalcDirLight(DirLight light, vec3 normal, vec3 viewDir, vec3 diffColor, vec3 specColor,vec3 F0, float roughness, float metallic)
{
	// ambient
	vec3 ambient = light.ambient * diffColor;
	// diffuse
	vec3 L =  normalize(light.position - fs_in.FragPos);
	vec3 H = normalize(viewDir + L);
	
	float diff = max(dot(normal, L), 0.0);
	vec3 diffuse = light.diffuse * (diff * diffColor);
	float NDF = DistributionGGX(normal, H, roughness);
    float G = GeometrySmith(normal, viewDir, L, roughness);
    vec3 F = fresnelSchlick(clamp(dot(H, viewDir), 0.0, 1.0), F0);
	// specular
	vec3 numerator    = NDF * G * F; 
    float denominator = 4 * max(dot(normal, viewDir), 0.0) * max(dot(normal, L), 0.0) + 0.0001; // + 0.0001 to prevent divide by zero
    vec3 specular = numerator / denominator;
	// kS is equal to Fresnel
    vec3 kS = F;
    // for energy conservation, the diffuse and specular light can't
    // be above 1.0 (unless the surface emits light); to preserve this
    // relationship the diffuse component (kD) should equal 1.0 - kS.
    vec3 kD = vec3(1.0) - kS;
    // multiply kD by the inverse metalness such that only non-metals 
    // have diffuse lighting, or a linear blend if partly metal (pure metals
    // have no diffuse light).
    kD *= 1.0 - metallic;

    // scale light by NdotL
    float NdotL = max(dot(normal, L), 0.0);
	// calculate shadow
	float shadow =  shadows ? ShadowDirectCalculation(light, normal, L) : 0.0;
	return ((1.0 - shadow) * (kD * diffColor / PI + specular)) * light.lightColor * light.intensity * NdotL; 
}

// calculates the color when using a point light.
vec3 CalcPointLight(PointLight light, vec3 normal, vec3 viewDir,vec3 diffColor, vec3 specColor,vec3 F0, float roughness, float metallic)
{
	vec3 ambient = light.ambient * diffColor;

	// diffuse
	vec3 L =  normalize(light.position - fs_in.FragPos);
	vec3 H = normalize(viewDir + L);
	float diff = max(dot(normal, L), 0.0);
	vec3 diffuse = light.diffuse * (diff * diffColor);
	float NDF = DistributionGGX(normal, H, roughness);   
    float G   = GeometrySmith(normal, viewDir, L, roughness);      
    vec3 F    = fresnelSchlick(clamp(dot(H, viewDir), 0.0, 1.0), F0);
	vec3 numerator    = NDF * G * F; 
    float denominator = 4 * max(dot(normal, viewDir), 0.0) * max(dot(normal, L), 0.0) + 0.0001; // + 0.0001 to prevent divide by zero
    vec3 specular = numerator / denominator;

	// calculate attenuation
	float dist = length(light.position - fs_in.FragPos);
	float attenuation = 1.0 / (light.constant + light.linear * dist + light.quadratic * (dist * dist));
	ambient *= attenuation;
	diffuse *= attenuation;
	specular *= attenuation;
    vec3 radiance = light.lightColor * attenuation * light.intensity;
	vec3 kS = F;
    // for energy conservation, the diffuse and specular light can't
    // be above 1.0 (unless the surface emits light); to preserve this
    // relationship the diffuse component (kD) should equal 1.0 - kS.
    vec3 kD = vec3(1.0) - kS;
    // multiply kD by the inverse metalness such that only non-metals 
    // have diffuse lighting, or a linear blend if partly metal (pure metals
    // have no diffuse light).
    kD *= 1.0 - metallic;	  

    // scale light by NdotL
    float NdotL = max(dot(normal, L), 0.0);
	float shadow = shadows ? ShadowPointCalculation(light,normal,L) : 0.0;
	
	return ((1.0 - shadow) * (kD * diffColor / PI + specular)) * radiance * NdotL; 
}


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

		if (currentDepth - bias >= pcfDepth) {
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
    //if(projCoords.z > 1.0)
    //    shadow = 0.0;
        
    return shadow;
}

vec3 CalcSpotLight(SpotLight light, vec3 normal, vec3 fragPos, vec3 viewDir,vec3 diffColor, vec3 specColor,vec3 F0, float roughness, float metallic)
{
    // Ambient
    vec3 ambient = light.ambient * diffColor;
    
    // Diffuse
	vec3 L =  normalize(light.position - fs_in.FragPos);
    float diff = max(dot(normal, L), 0.0);
    vec3 diffuse = light.diffuse * diff * diffColor;
    vec3 H = normalize(viewDir + L);
    // Specular
    vec3 reflectDir = reflect(-L, normal);
    float spec = pow(max(dot(viewDir, reflectDir), 0.0), material.shininess);
    
    // Spotlight (soft edges)
    float theta = dot(L, normalize(-light.direction));
    float epsilon = (light.cutOff - light.outerCutOff);
    float intensity = clamp((theta - light.outerCutOff) / epsilon, 0.0, 1.0);
	diffuse *= intensity;
    // Attenuation
    float distance  = length(light.position - fragPos);
    float attenuation = 1.0 / (light.constant + light.linear * distance + light.quadratic * (distance * 2));
    diffuse  *=  attenuation;

	float NDF = DistributionGGX(normal, H, roughness);   
    float G   = GeometrySmith(normal, viewDir, L, roughness);      
    vec3 F    = fresnelSchlick(clamp(dot(H, viewDir), 0.0, 1.0), F0);
       
    vec3 numerator    = NDF * G * F; 
    float denominator = 4 * max(dot(normal, viewDir), 0.0) * max(dot(normal, L), 0.0) + 0.0001; // + 0.0001 to prevent divide by zero
    vec3 specular = numerator / denominator;
	vec3 radiance = light.lightColor * attenuation * light.intensity * intensity;
    
    vec3 kS = F;
    vec3 kD = vec3(1.0) - kS;
    kD *= 1.0 - metallic;

    // scale light by NdotL
    float NdotL = max(dot(normal, L), 0.0);
	float shadow = shadows ? SpotLightShadowCalculation(light, normal, L) : 0.0;

	return ((1.0 - shadow) * (kD * diffuse / PI + specular)) * radiance * NdotL * 100; 
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
    float bias = max(0.05 * (1.0 - dot(normal, lightDir*2)), 0.05);
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
				shadow += currentDepth - bias >= pcfDepth  ? 1.0 : 0.0;        
			}
		}
		shadow /= 9.0;
	}
   
    
    // keep the shadow at 0.0 when outside the far_plane region of the light's frustum.
    if(projCoords.z >= 1.0)
        shadow = 1.0;
        
    return shadow;
}
float DistributionGGX(vec3 N, vec3 H, float roughness)
{
    float a = roughness*roughness;
    float a2 = a*a;
    float NdotH = max(dot(N, H), 0.0);
    float NdotH2 = NdotH*NdotH;

    float nom   = a2;
    float denom = (NdotH2 * (a2 - 1.0) + 1.0);
    denom = PI * denom * denom;

    return nom / denom;
}
// ----------------------------------------------------------------------------
float GeometrySchlickGGX(float NdotV, float roughness)
{
    float r = (roughness + 1.0);
    float k = (r*r) / 8.0;

    float nom   = NdotV;
    float denom = NdotV * (1.0 - k) + k;

    return nom / denom;
}
// ----------------------------------------------------------------------------
float GeometrySmith(vec3 N, vec3 V, vec3 L, float roughness)
{
    float NdotV = max(dot(N, V), 0.0);
    float NdotL = max(dot(N, L), 0.0);
    float ggx2 = GeometrySchlickGGX(NdotV, roughness);
    float ggx1 = GeometrySchlickGGX(NdotL, roughness);

    return ggx1 * ggx2;
}
// ----------------------------------------------------------------------------
vec3 fresnelSchlick(float cosTheta, vec3 F0)
{
    return F0 + (1.0 - F0) * pow(clamp(1.0 - cosTheta, 0.0, 1.0), 5.0);
}