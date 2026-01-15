texture text : register(t0);
sampler text_sampler : register(s0);

texture grade_from : register(t1);
sampler grade_from_sampler : register(s1);
texture grade_to : register(t2);
sampler grade_to_sampler : register(s2);

uniform float percent;

float4 pixel_shader_fade(float4 position : SV_POSITION, float4 color : COLOR, float2 uv : TEXCOORD) : COLOR
{
    float4 text_color = tex2D(text_sampler, uv) * color;
    float size = 16.0;
    float size_squared = size * size;

    float offset_r = text_color.r * (1.0 / size_squared) * (size - 1.0) + (1.0 / size_squared) * 0.5;
    float offset_g = text_color.g + (1.0 / size) * 0.5;
    float b_slice_0 = min(floor(text_color.b * size), size - 1.0);
    float b_slice_1 = min(b_slice_0 + 1.0, size - 1.0);

    float2 index_0 = float2(offset_r + b_slice_0 / size, offset_g);
    float2 index_1 = float2(offset_r + b_slice_1 / size, offset_g);
    float3 from_0 = tex2D(grade_from_sampler, index_0).rgb;
    float3 from_1 = tex2D(grade_from_sampler, index_1).rgb;
    float3 to_0 = tex2D(grade_to_sampler, index_0).rgb;
    float3 to_1 = tex2D(grade_to_sampler, index_1).rgb;

    float b_offset = step(fmod(text_color.b * size, 1.0), 0.5);
    float3 from = lerp(from_0, from_1, b_offset);
    float3 to = lerp(to_0, to_1, b_offset);

    return float4(lerp(from, to, percent) * text_color.a, text_color.a);
}

float4 pixel_shader_single(float4 position : SV_POSITION, float4 color : COLOR, float2 uv : TEXCOORD) : COLOR
{
    float4 text_color = tex2D(text_sampler, uv) * color;
    float size = 16.0;
    float size_squared = size * size;

    float offset_r = text_color.r * (1.0 / size_squared) * (size - 1.0) + (1.0 / size_squared) * 0.5;
    float offset_g = text_color.g + (1.0 / size) * 0.5;
    float b_slice_0 = min(floor(text_color.b * size), size - 1.0);
    float b_slice_1 = min(b_slice_0 + 1.0, size - 1.0);

    float3 sample_0 = tex2D(grade_from_sampler, float2(offset_r + b_slice_0 / size, offset_g)).rgb;
    float3 sample_1 = tex2D(grade_from_sampler, float2(offset_r + b_slice_1 / size, offset_g)).rgb;

    return float4(lerp(sample_0, sample_1, step(0.5, fmod(text_color.b * size, 1.0))) * text_color.a, text_color.a);
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