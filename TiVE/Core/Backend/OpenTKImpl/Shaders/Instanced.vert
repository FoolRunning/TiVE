#version 150 core 

uniform vec3 modelTranslation;
uniform int lightCount;

struct Light 
{    
    vec3 location;
    vec3 color;
    float cachedValue;
};

#define NR_LIGHTS 50
uniform Light lights[NR_LIGHTS];

// incoming vertex information
in vec4 in_Position;
in vec3 in_Normal;

// incoming vertex information for each instance
in vec3 in_InstancePos;
in vec4 in_InstanceColor;

flat out vec4 voxColor;
 
vec4 CalcColor(vec3 voxelPos, vec4 baseColor)
{
    vec3 lightColor = vec3(0);
    //if (in_Normal != 0)
    //{
    //    for (int i = 0; i < lightCount; i++)
    //    {
    //        float dist = length(lights[i].location - voxelPos);
    //        float att = max(0.0f, 1.0f - dist * lights[i].cachedValue);
    //        att *= att;
    //
    //        vec3 lightDir = (lights[i].location - voxelPos) / dist;
    //        att *= max(dot(in_Normal, lightDir), 0.0);
    //
    //        lightColor += lights[i].color * att;
    //    }
    //}
    //else
    {
        for (int i = 0; i < lightCount; i++)
        {
            float dist = length(lights[i].location - voxelPos);
            float att = max(0.0f, 1.0f - dist * lights[i].cachedValue);
            att *= att;
    
            lightColor += lights[i].color * att;
        }
    }

    return vec4(lightColor * baseColor.rgb, baseColor.a);
}

void main(void)
{
    voxColor = CalcColor(modelTranslation + in_Position.xyz + in_InstancePos.xyz, in_InstanceColor);

    // transform the incoming vertex position
    gl_Position = vec4(in_Position.xyz + in_InstancePos.xyz, in_Position.w);
}
