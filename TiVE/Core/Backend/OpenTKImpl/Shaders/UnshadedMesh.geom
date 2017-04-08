#version 150 core

layout (points) in;
layout (triangle_strip, max_vertices=12) out;

uniform mat4 matrix_ModelViewProjection;
uniform vec3 cameraLoc;
uniform vec3 modelTranslation;
uniform int voxelSize;
uniform int lightCount;

struct Light 
{    
    vec3 location;
    vec3 color;
    float cachedValue;
};

#define NR_LIGHTS 20
uniform Light lights[NR_LIGHTS];

flat in vec4 colorPass[];
flat in vec3 normalPass[];

flat out vec4 fragment_color;

void main()
{
    vec3 pos = gl_in[0].gl_Position.xyz;
    vec3 voxelPos = modelTranslation + pos;
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
    
    vec3 lightColor = vec3(0);
    vec3 normal = normalPass[0];
    for (int i = 0; i < lightCount; i++)
    {
        float dist = length(lights[i].location - voxelPos);
        float att = max(0.0f, 1.0f - dist * lights[i].cachedValue);
        att *= att;

        if (normal.x != 0 || normal.y != 0 || normal.z != 0)
        {
            // Adjust brightness for surface orientation
            vec3 lightDir = normalize(lights[i].location - voxelPos);
            att *= max(dot(normal, lightDir), 0.0);
        }

        lightColor += lights[i].color * att;
    }

    fragment_color = vec4(lightColor * colorPass[0].rgb, colorPass[0].a);

    // apply gamma correction
    //float gamma = 2.2;
    //vec3 finalColor = lightColor * colorPass[0].rgb;
    //fragment_color = vec4(pow(finalColor, vec3(1.0 / gamma)), colorPass[0].a);

    int sides = int(gl_in[0].gl_Position.w);
    if ((sides & 1) != 0 && cameraLoc.y > voxelPos.y) // top
    {
        gl_Position = v2; EmitVertex();
        gl_Position = v3; EmitVertex();
        gl_Position = v1; EmitVertex();
        gl_Position = v4; EmitVertex();
        EndPrimitive();
    }
    
    if ((sides & 2) != 0 && cameraLoc.x < voxelPos.x) // left
    {
        gl_Position = v4; EmitVertex();
        gl_Position = v8; EmitVertex();
        gl_Position = v1; EmitVertex();
        gl_Position = v5; EmitVertex();
        EndPrimitive();
    }
        
    if ((sides & 4) != 0 && cameraLoc.x > voxelPos.x) // right
    {
        gl_Position = v2; EmitVertex();
        gl_Position = v6; EmitVertex();
        gl_Position = v3; EmitVertex();
        gl_Position = v7; EmitVertex();
        EndPrimitive();
    }

    if ((sides & 8) != 0 && cameraLoc.y < voxelPos.y) // bottom
    {
        gl_Position = v7; EmitVertex();
        gl_Position = v6; EmitVertex();
        gl_Position = v8; EmitVertex();
        gl_Position = v5; EmitVertex();
        EndPrimitive();
    }

    if ((sides & 16) != 0 && cameraLoc.z > voxelPos.z) // front
    {
        gl_Position = v3; EmitVertex();
        gl_Position = v7; EmitVertex();
        gl_Position = v4; EmitVertex();
        gl_Position = v8; EmitVertex();
        EndPrimitive();
    }

    if ((sides & 32) != 0 && cameraLoc.z < voxelPos.z) // back
    {
        gl_Position = v6; EmitVertex();
        gl_Position = v2; EmitVertex();
        gl_Position = v5; EmitVertex();
        gl_Position = v1; EmitVertex();
        EndPrimitive();
    }
}