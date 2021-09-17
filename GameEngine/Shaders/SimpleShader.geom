#version 460 core
layout (triangles) in;
layout (triangle_strip, max_vertices = 3) out;

in VS_OUT {
    vec3 FragPos;
    vec3 Normal;
    vec2 TexCoords;
} gs_in[];
out GS_OUT
{
    vec3 FragPos;
    vec3 Normal;
    vec2 TexCoords;
} gs_out;


uniform mat4 VP;
uniform float time;

vec4 explode(vec4 position, vec3 normal) 
{
    float magnitude = 2.0;
    vec3 direction = normal * ((sin(time) + 1.0) / 2.0) * magnitude; 
    return position + vec4(direction, 0.0);
}

vec3 GetNormal()
{   
    vec3 a = vec3(gl_in[0].gl_Position) - vec3(gl_in[1].gl_Position);
   vec3 b = vec3(gl_in[2].gl_Position) - vec3(gl_in[1].gl_Position);
   return normalize(cross(a, b)); 
   }

void main() 
{    
    for (int i = 0; i < gs_in.length(); i++)
    {
        gl_Position = VP *  gl_in[i].gl_Position;
        gs_out.FragPos = gs_in[i].FragPos;
        gs_out.TexCoords = gs_in[i].TexCoords;
        gs_out.Normal = gs_in[i].Normal;
        EmitVertex();
    }
    EndPrimitive();
}  