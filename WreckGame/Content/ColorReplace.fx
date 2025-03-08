#if OPENGL
    #define VS_SHADERMODEL vs_3_0
    #define PS_SHADERMODEL ps_3_0
#else
    #define VS_SHADERMODEL vs_4_0_level_9_1
    #define PS_SHADERMODEL ps_4_0_level_9_1
#endif

// External parameters
float4 TextColor;
float4 BackgroundColor;
bool IsBackground;
sampler2D TextureSampler : register(s0);

// Structure from vertex shader to pixel shader
struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float2 TextureCoordinates : TEXCOORD0;
};

// Pixel shader function
float4 MainPS(VertexShaderOutput input) : COLOR
{
    float4 originalColor = tex2D(TextureSampler, input.TextureCoordinates);
    
    // Check if the pixel is white (or close to white)
    float whiteTolerance = 0.9;
    
    if (originalColor.r >= whiteTolerance && 
        originalColor.g >= whiteTolerance && 
        originalColor.b >= whiteTolerance)
    {
        // Get the appropriate target color
        float4 targetColor = IsBackground ? BackgroundColor : TextColor;
        
        // Use the original alpha for transparency handling
        // but only if the target color isn't transparent
        if (targetColor.a < 0.01)
            return float4(0, 0, 0, 0); // Return fully transparent
        else
            return float4(targetColor.rgb, originalColor.a);
    }
    
    // Keep the original color (black outline or any other color)
    return originalColor;
}

technique ColorReplaceTechnique
{
    pass P0
    {
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
}