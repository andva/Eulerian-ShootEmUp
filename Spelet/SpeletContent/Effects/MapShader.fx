float4x4 Projection;
float4x4 View;

float3 lightDir1 = float3(5,-10,5);
float3 lightDir2 = float3(-5,10,5);

texture Texture;

sampler C_Sampler = sampler_state
{
	Texture = <Texture>;   
	MinFilter     = Anisotropic;
	MagFilter     = Anisotropic;
	MipFilter     = Linear;
	MaxAnisotropy = 16;
	MipMapLodBias = -2.0f;
};

texture N_Texture;
sampler N_Sampler = sampler_state
{
	Texture = <N_Texture>;   
	MinFilter     = Anisotropic;
	MagFilter     = Anisotropic;
	MipFilter     = Linear;
	MaxAnisotropy = 16;
	MipMapLodBias = -2.0f;
};

struct VS_INPUT
{
	float4 Position : POSITION0;
	float2 TexCoord : TEXCOORD0;
	float3 Normal : NORMAL0;
	float3 Tangents : TANGENT0;
};

struct VS_OUTPUT
{
	float4 Position : POSITION0;
	float2 TexCoord : TEXCOORD0;
	float3 Data1 : TEXCOORD1;
	float3 Data2 : TEXCOORD2;
};

VS_OUTPUT VSBasic (VS_INPUT input)
{
	VS_OUTPUT output;
	float4 pos = mul(input.Position, View);
	pos = mul(pos, Projection);
	float3 nml = normalize(input.Normal);
	output.Position = pos;
	output.TexCoord = input.TexCoord;
	output.Data1 = nml;
	output.Data2 = 0;
	return output;
}

VS_OUTPUT VSNormals (VS_INPUT input)
{
	VS_OUTPUT output;
	float4 pos = mul(input.Position, View);
	pos = mul(pos, Projection);

	float3x3 tgtSpace;
	tgtSpace[0] = input.Tangents;
	tgtSpace[1] = cross(input.Tangents, input.Normal);
	tgtSpace[2] = input.Normal;

	float3 tgtLightDir1 = mul(tgtSpace, lightDir1);
	float3 tgtLightDir2 = mul(tgtSpace, lightDir2);

	output.Position = pos;
	output.TexCoord = input.TexCoord;
	output.Data1 = normalize(tgtLightDir1);
	output.Data2 = normalize(tgtLightDir2);

	return output;
}

float4 PSBasic (VS_OUTPUT input) : COLOR0
{
	float4 outColor = tex2D(C_Sampler, input.TexCoord);
	return outColor;
}

float4 PSNormals (VS_OUTPUT input) : COLOR0
{
	float4 outColor = tex2D(C_Sampler, input.TexCoord);
	float3 normal = 2 * tex2D(N_Sampler, input.TexCoord) - 1.0 ;
	float diffuse = saturate(dot(normal, input.Data1)) + 
					saturate(dot(normal, input.Data2));
	return outColor * diffuse* 1.5;
}

float4 PSDiffuse(VS_OUTPUT input) : COLOR0
{
	float4 outColor = tex2D(C_Sampler, input.TexCoord);

	float diffuse = saturate(dot(input.Data1, normalize(lightDir1))) +
					saturate(dot(input.Data2, normalize(lightDir2)));

	return outColor * diffuse;
}

technique Basic
{
	pass Pass1
	{
		VertexShader = compile vs_1_1 VSBasic();
		PixelShader = compile ps_2_0 PSBasic();
		CullMode = None;  
	}
}

technique Diffuse
{
	pass Pass1
	{
		VertexShader = compile vs_1_1 VSBasic();
		PixelShader = compile ps_2_0 PSDiffuse();
		CullMode = None;
	}
}

technique Bump
{
	pass Pass1
	{
		VertexShader = compile vs_2_0 VSNormals();
		PixelShader = compile ps_2_0 PSNormals();
		CullMode = None;

	}
}
