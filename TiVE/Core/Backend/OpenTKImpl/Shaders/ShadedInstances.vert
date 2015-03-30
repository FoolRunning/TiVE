#version 150 core 
 
// premultiplied model to projection transformation
uniform mat4 matrix_ModelViewProjection;
 
// incoming vertex information
in vec3 in_Position;
in vec4 in_Color;

// incoming vertex information for each instance
in vec3 in_InstancePos;
in vec4 in_InstanceColor;

flat out vec4 fragment_color;
 
void main(void)
{
    fragment_color = in_Color * in_InstanceColor;

    // transform the incoming vertex position
    gl_Position = matrix_ModelViewProjection * vec4(in_Position + in_InstancePos, 1);
}
