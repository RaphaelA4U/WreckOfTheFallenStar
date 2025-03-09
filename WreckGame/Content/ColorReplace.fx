#if OPENGL
    #define VS_SHADERMODEL vs_3_0
    #define PS_SHADERMODEL ps_3_0
#else
    #define VS_SHADERMODEL vs_4_0_level_9_1
    #define PS_SHADERMODEL ps_4_0_level_9_1
#endif

float4 TextColor;
float4 BackgroundColor;
bool IsBackground;
sampler2D TextureSampler : register(s0);

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float2 TextureCoordinates : TEXCOORD0;
};

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float4 originalColor = tex2D(TextureSampler, input.TextureCoordinates);

    if (IsBackground)
    {
        if (BackgroundColor.a < 0.01)
            return float4(BackgroundColor.rgb, 1.0);

        return float4(BackgroundColor.rgb, originalColor.a * BackgroundColor.a);
    }
    else
    {
        float whiteTolerance = 0.9;
        
        float3 normalizedColor = (originalColor.a > 0) ? originalColor.rgb / originalColor.a : originalColor.rgb;

        if (normalizedColor.r >= whiteTolerance &&
            normalizedColor.g >= whiteTolerance &&
            normalizedColor.b >= whiteTolerance)
        {
            if (TextColor.a < 0.01)
                return float4(0, 0, 0, 0);
            return float4(TextColor.rgb, originalColor.a * TextColor.a);
        }
        return originalColor;
    }
}

technique ColorReplaceTechnique
{
    pass P0
    {
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
}
