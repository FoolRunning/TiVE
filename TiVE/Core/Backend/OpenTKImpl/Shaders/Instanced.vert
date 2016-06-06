#version 150 core 

// incoming vertex information
in vec4 in_Position;

// incoming vertex information for each instance
in vec3 in_InstancePos;
in vec4 in_InstanceColor;

flat out vec4 colorPass;
 
void main(void)
{
    colorPass = in_InstanceColor;

    // transform the incoming vertex position
    gl_Position = vec4(in_Position.xyz + in_InstancePos.xyz, in_Position.w);
}
