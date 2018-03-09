#version 150 core

uniform vec3 modelTranslation;
uniform vec3 ambientLight;
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
in vec4 inPosition;
in vec4 inColor;
in vec3 inNormal;
in vec4 inVoxelData;

flat out vec4 voxColor;

vec4 CalcColor(vec3 voxelPos, vec4 baseColor)
{
    vec3 lightColor = ambientLight;
    if (inNormal != 0)
    {
        for (int i = 0; i < lightCount; i++)
        {
            float dist = length(lights[i].location - voxelPos);
            float att = max(0.0f, 1.0f - dist * lights[i].cachedValue);
            att *= att;

            vec3 lightDir = (lights[i].location - voxelPos) / dist;
            att *= max(dot(inNormal, lightDir), 0.0);

            lightColor += lights[i].color * att;
        }
    }
    else
    {
        for (int i = 0; i < lightCount; i++)
        {
            float dist = length(lights[i].location - voxelPos);
            float att = max(0.0f, 1.0f - dist * lights[i].cachedValue);
            att *= att;
    
            lightColor += lights[i].color * att;
        }
    }

    float ambientOcclusionFactor = int(inVoxelData.x) / 255.0f;
    lightColor = lightColor * ambientOcclusionFactor;
    return vec4(lightColor * baseColor.rgb, baseColor.a);
}

void main(void)
{
    voxColor = CalcColor(modelTranslation + inPosition.xyz, inColor);
    gl_Position = inPosition;
}

