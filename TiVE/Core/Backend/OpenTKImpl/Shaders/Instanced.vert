#version 150 core 

// incoming vertex information
in vec4 in_Position;
in vec3 in_Normal;

// incoming vertex information for each instance
in vec3 in_InstancePos;
in vec4 in_InstanceColor;

flat out vec4 colorPass;
flat out vec3 normalPass;
 
void main(void)
{
    colorPass = in_InstanceColor;
    normalPass = in_Normal;

    // transform the incoming vertex position
    gl_Position = vec4(in_Position.xyz + in_InstancePos.xyz, in_Position.w);
}
