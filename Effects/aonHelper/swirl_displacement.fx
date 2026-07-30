texture entity_texture : register(t0);
sampler entity_sampler : register(s0);

struct vertex_input
{
    float3 position : POSITION0;
    float4 color : COLOR0;
    float2 uv : TEXCOORD0;
};
struct vertex_output
{
    float4 position : POSITION0;
    float4 color : COLOR0;
    float2 uv : TEXCOORD0;
};

static const float pi = 3.141592654;

uniform float time;
uniform int depth;
uniform float4 swirl_config;

uniform float4x4 World;

// https://www.shadertoy.com/view/4djSRW
float3 pseudo_noise(float3 position)
{
    float3 pseudo = frac(position * float3(.1031, .1030, .0973));
    pseudo += dot(pseudo, pseudo.yxz + 33.33);
    return frac((pseudo.xxy + pseudo.yxx) * pseudo.zyx);
}

// emulates linear sampling from a texture cus apparently u can't do that in the vertex shader
float3 lerped_noise(float3 position) {
    float3 a = pseudo_noise(float3(round(position.x - 0.5), round(position.y - 0.5), position.z));
    float3 b = pseudo_noise(float3(round(position.x + 0.5), round(position.y - 0.5), position.z));
    float3 c = pseudo_noise(float3(round(position.x - 0.5), round(position.y + 0.5), position.z));
    float3 d = pseudo_noise(float3(round(position.x + 0.5), round(position.y + 0.5), position.z));
    
    float2 lerps = frac(position.xy);
    float3 lerp_ab = lerp(a, b, lerps.x);
    float3 lerp_cd = lerp(c, d, lerps.x);
    float3 final = lerp(lerp_ab, lerp_cd, lerps.y);
    
    return final;
}

vertex_output vertex_shader(vertex_input input)
{
    vertex_output output;
    
    float3 noise_sample_pos = float3(input.position.xy / 8.0, depth);
    float3 noise = lerped_noise(noise_sample_pos);
    
    float radius = lerp(swirl_config.x, swirl_config.y, noise.r);
    float speed = lerp(swirl_config.z, swirl_config.w, noise.g);
    float offset = lerp(0.0, 2.0 * pi, noise.b);
    float angle = speed * time + offset;
    
    float4 swirl_offset = float4(cos(angle), sin(angle), 0.0, 0.0) * radius;
    float4 swirl_position = float4(input.position, 1.0) + swirl_offset;
    float4 final_position = mul(swirl_position, World);

    output.position = final_position;
    output.color = input.color;
    output.uv = input.uv;

    return output;
}

float4 pixel_shader(vertex_output input) : COLOR0
{
    return tex2D(entity_sampler, input.uv) * input.color;
}

technique blossom_block
{
    pass
    {
        VertexShader = compile vs_3_0 vertex_shader();
        PixelShader = compile ps_3_0 pixel_shader();
    }
}
