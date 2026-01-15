texture text : register(t0);
sampler text_sampler : register(s0);

// i would prefer not to do this but otherwise there are weird ass magenta artifacts
// also it's what the vanilla shader does so :fire:
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

uniform float percent = 0.0;

uniform float from_filter = 0.0;
uniform float to_filter = 0.0;

static const float2 dimensions = float2(256.0, 16.0);

float4 pixel_shader_fade(float4 position : SV_POSITION, float4 color : COLOR, float2 uv : TEXCOORD) : COLOR
{
    float4 filter_percent = lerp(from_filter, to_filter, percent);

    float4 text_color = tex2D(text_sampler, uv) * color;

    float offset_r = text_color.r / dimensions.x * (dimensions.y - 1.0);
    float offset_g = text_color.g;
    float b_slice_0 = min(floor(text_color.b * dimensions.y), dimensions.y - 1.0);
    float b_slice_1 = min(b_slice_0 + 1.0, dimensions.y - 1.0);

    float2 index_0 = float2(offset_r + b_slice_0 / dimensions.y, offset_g);
    float2 index_1 = float2(offset_r + b_slice_1 / dimensions.y, offset_g);
    float2 filtered_index_0 = lerp(index_0, (round(index_0 * dimensions) + 0.5) / dimensions, filter_percent);
    float2 filtered_index_1 = lerp(index_1, (round(index_1 * dimensions) + 0.5) / dimensions, filter_percent);
    float3 from_0 = tex2D(grade_from_sampler, filtered_index_0).rgb;
    float3 from_1 = tex2D(grade_from_sampler, filtered_index_1).rgb;
    float3 to_0 = tex2D(grade_to_sampler, filtered_index_0).rgb;
    float3 to_1 = tex2D(grade_to_sampler, filtered_index_1).rgb;

    float offset_b = frac(text_color.b * dimensions.y);
    float filtered_offset_b = lerp(offset_b, step(0.5, offset_b), filter_percent);
    float3 from = lerp(from_0, from_1, filtered_offset_b);
    float3 to = lerp(to_0, to_1, filtered_offset_b);

    return float4(lerp(from, to, percent) * text_color.a, text_color.a);
}

float4 pixel_shader_single(float4 position : SV_POSITION, float4 color : COLOR, float2 uv : TEXCOORD) : COLOR
{
    float4 text_color = tex2D(text_sampler, uv) * color;

    float offset_r = text_color.r / dimensions.x * (dimensions.y - 1.0);
    float offset_g = text_color.g;
    float b_slice_0 = min(floor(text_color.b * dimensions.y), dimensions.y - 1.0);
    float b_slice_1 = min(b_slice_0 + 1.0, dimensions.y - 1.0);

    float2 index_0 = float2(offset_r + b_slice_0 / dimensions.y, offset_g);
    float2 index_1 = float2(offset_r + b_slice_1 / dimensions.y, offset_g);
    float2 filtered_index_0 = lerp(index_0, (round(index_0 * dimensions) + 0.5) / dimensions, from_filter);
    float2 filtered_index_1 = lerp(index_1, (round(index_1 * dimensions) + 0.5) / dimensions, from_filter);
    float3 sample_0 = tex2D(grade_from_sampler, filtered_index_0).rgb;
    float3 sample_1 = tex2D(grade_from_sampler, filtered_index_1).rgb;

    float offset_b = frac(text_color.b * dimensions.y);
    float filtered_offset_b = lerp(offset_b, step(0.5, offset_b), from_filter);
    return float4(lerp(sample_0, sample_1, filtered_offset_b) * text_color.a, text_color.a);
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