#version 150 core

layout (points) in;
layout (points, max_vertices=1) out;

uniform mat4 matrix_ModelViewProjection;
uniform int voxelSize;

flat in vec4 colorPass[];

flat out vec4 fragment_color;

void main()
{
    vec3 pos = gl_in[0].gl_Position.xyz;
    float voxelHalf = voxelSize / 2.0;
    
    fragment_color = colorPass[0];
    gl_Position = matrix_ModelViewProjection * vec4(pos.x + voxelHalf, pos.y + voxelHalf, pos.z + voxelHalf, 1); 
    EmitVertex();
    EndPrimitive();
}