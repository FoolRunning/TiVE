#version 150 core

layout (points) in;
layout (triangle_strip, max_vertices=24) out;

uniform mat4 matrix_ModelViewProjection;
uniform int voxelSize;

flat in vec4 colorPass[];

flat out vec4 fragment_color;

void main()
{
    vec3 pos = gl_in[0].gl_Position.xyz;
    float x2 = pos.x + voxelSize;
    float y2 = pos.y + voxelSize;
    float z2 = pos.z + voxelSize;
    vec4 v1 = matrix_ModelViewProjection * vec4(pos.x, y2   , pos.z, 1);
    vec4 v2 = matrix_ModelViewProjection * vec4(x2   , y2   , pos.z, 1);
    vec4 v3 = matrix_ModelViewProjection * vec4(x2   , y2   , z2   , 1);
    vec4 v4 = matrix_ModelViewProjection * vec4(pos.x, y2   , z2   , 1);
    vec4 v5 = matrix_ModelViewProjection * vec4(pos.x, pos.y, pos.z, 1);
    vec4 v6 = matrix_ModelViewProjection * vec4(x2   , pos.y, pos.z, 1);
    vec4 v7 = matrix_ModelViewProjection * vec4(x2   , pos.y, z2   , 1);
    vec4 v8 = matrix_ModelViewProjection * vec4(pos.x, pos.y, z2   , 1);
    
    int sides = int(gl_in[0].gl_Position.w);
    if ((sides & 1) != 0) // top
    {
        fragment_color = vec4(colorPass[0].rgb * 1.2, colorPass[0].a);
        gl_Position = v2; EmitVertex();
        gl_Position = v3; EmitVertex();
        gl_Position = v1; EmitVertex();
        gl_Position = v4; EmitVertex();
        EndPrimitive();
    }
    
    if ((sides & 2) != 0) // left
    {
        fragment_color = vec4(colorPass[0].rgb * 1.1, colorPass[0].a);
        gl_Position = v4; EmitVertex();
        gl_Position = v8; EmitVertex();
        gl_Position = v1; EmitVertex();
        gl_Position = v5; EmitVertex();
        EndPrimitive();
    }
        
    if ((sides & 4) != 0) // right
    {
        fragment_color = vec4(colorPass[0].rgb * 0.9, colorPass[0].a);
        gl_Position = v2; EmitVertex();
        gl_Position = v6; EmitVertex();
        gl_Position = v3; EmitVertex();
        gl_Position = v7; EmitVertex();
        EndPrimitive();
    }

    if ((sides & 8) != 0) // bottom
    {
        fragment_color = vec4(colorPass[0].rgb * 0.8, colorPass[0].a);
        gl_Position = v7; EmitVertex();
        gl_Position = v6; EmitVertex();
        gl_Position = v8; EmitVertex();
        gl_Position = v5; EmitVertex();
        EndPrimitive();
    }

    if ((sides & 16) != 0) // front
    {
        fragment_color = colorPass[0];
        gl_Position = v3; EmitVertex();
        gl_Position = v7; EmitVertex();
        gl_Position = v4; EmitVertex();
        gl_Position = v8; EmitVertex();
        EndPrimitive();
    }

    if ((sides & 32) != 0) // back
    {
        fragment_color = colorPass[0];
        gl_Position = v6; EmitVertex();
        gl_Position = v2; EmitVertex();
        gl_Position = v5; EmitVertex();
        gl_Position = v1; EmitVertex();
        EndPrimitive();
    }
}