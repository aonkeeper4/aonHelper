// i have verified that this has the exact same colorgrading behaviour as the vanilla shader when both `from_quantization` and `to_quantization` are 0.
// don't yell at me

texture tex : register(t0);
sampler tex_sampler : register(s0);

// i would prefer not to do this but otherwise there are weird ass magenta artifacts
// guess the vanilla shader does it for a reason :cry:
texture grade_from : register(t1);
sampler grade_from_sampler : register(s1)
{
    AddressU = Clamp;
    AddressV = Clamp;
};
texture grade_to : register(t2);
sampler grade_to_sampler : register(s2)
{
    AddressU = Clamp;
    AddressV = Clamp;
};

uniform float from_quantization = 0.0;
uniform float to_quantization = 0.0;
uniform float percent = 0.0;

static const float2 dimensions = float2(256.0, 16.0);

// used when fading between 2 colorgrades
float4 pixel_shader_fade(float4 position : SV_POSITION, float4 color : COLOR, float2 uv : TEXCOORD) : COLOR
{
    float4 quantization_percent = lerp(from_quantization, to_quantization, percent);

    float4 tex_color = tex2D(tex_sampler, uv) * color;

    float offset_r = tex_color.r / dimensions.x * (dimensions.y - 1.0);
    float offset_g = tex_color.g; // / dimensions.y * (dimensions.y - 1.0); <- this should really be here to make the logic consistent across the r and g components
    float b_slice_0 = min(floor(tex_color.b * dimensions.y), dimensions.y - 1.0);
    float b_slice_1 = min(b_slice_0 + 1.0, dimensions.y - 1.0);

    float2 index_0 = float2(offset_r + b_slice_0 / dimensions.y, offset_g);
    float2 index_1 = float2(offset_r + b_slice_1 / dimensions.y, offset_g);
    float2 filtered_index_0 = lerp(index_0 + 0.5 / dimensions, (round(index_0 * dimensions) + 0.5) / dimensions, quantization_percent);
    float2 filtered_index_1 = lerp(index_1 + 0.5 / dimensions, (round(index_1 * dimensions) + 0.5) / dimensions, quantization_percent);
    float3 from_0 = tex2D(grade_from_sampler, filtered_index_0).rgb;
    float3 from_1 = tex2D(grade_from_sampler, filtered_index_1).rgb;
    float3 to_0 = tex2D(grade_to_sampler, filtered_index_0).rgb;
    float3 to_1 = tex2D(grade_to_sampler, filtered_index_1).rgb;

    float offset_b = frac(tex_color.b * dimensions.y);
    float filtered_offset_b = lerp(offset_b, step(0.5, offset_b), quantization_percent);
    float3 from = lerp(from_0, from_1, filtered_offset_b);
    float3 to = lerp(to_0, to_1, filtered_offset_b);

    return float4(lerp(from, to, percent) * tex_color.a, tex_color.a);
}

// used when only a single colorgrade is being used
float4 pixel_shader_single(float4 position : SV_POSITION, float4 color : COLOR, float2 uv : TEXCOORD) : COLOR
{
    float4 tex_color = tex2D(tex_sampler, uv) * color;

    float offset_r = tex_color.r / dimensions.x * (dimensions.y - 1.0);
    float offset_g = tex_color.g; // / dimensions.y * (dimensions.y - 1.0); <- this should really be here to make the logic consistent across the r and g components
    float b_slice_0 = min(floor(tex_color.b * dimensions.y), dimensions.y - 1.0);
    float b_slice_1 = min(b_slice_0 + 1.0, dimensions.y - 1.0);

    float2 index_0 = float2(offset_r + b_slice_0 / dimensions.y, offset_g);
    float2 index_1 = float2(offset_r + b_slice_1 / dimensions.y, offset_g);
    float2 filtered_index_0 = lerp(index_0 + 0.5 / dimensions, (round(index_0 * dimensions) + 0.5) / dimensions, from_quantization);
    float2 filtered_index_1 = lerp(index_1 + 0.5 / dimensions, (round(index_1 * dimensions) + 0.5) / dimensions, from_quantization);
    float3 sample_0 = tex2D(grade_from_sampler, filtered_index_0).rgb;
    float3 sample_1 = tex2D(grade_from_sampler, filtered_index_1).rgb;

    float offset_b = frac(tex_color.b * dimensions.y);
    float filtered_offset_b = lerp(offset_b, step(0.5, offset_b), from_quantization);
    return float4(lerp(sample_0, sample_1, filtered_offset_b) * tex_color.a, tex_color.a);
}

technique ColorGrade
{
    pass
    {
        PixelShader = compile ps_3_0 pixel_shader_fade();
    }
}
technique ColorGradeSingle
{
    pass
    {
        PixelShader = compile ps_3_0 pixel_shader_single();
    }
}