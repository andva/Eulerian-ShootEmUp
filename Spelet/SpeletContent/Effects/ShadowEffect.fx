#define MaxBones 59
float4x4 Bones[MaxBones];
float4x4 View;
float4x4 Projection;

float3 lightDir1 = float3(1,1,1);
float3 lightDir2 = float3(-1,-1,-1);
float specPow = 30;
float Kd = 1; 
float Ks = 1;
float Kr = 1;

bool useSpecMap = false;

texture Texture;
sampler C_Sampler = sampler_state 
{
	Texture = <Texture>; 
	MinFilter = Linear; 
	MagFilter = Linear;  
	MipFilter = Linear; 
};
 
texture N_Texture;
sampler N_Sampler = sampler_state 
{
	Texture = <N_Texture>; 
	MinFilter = Linear; 
	MagFilter = Linear;  
	MipFilter = Linear; 
 };

texture Env_Texture;
sampler Env_Sampler = sampler_state 
{
	Texture = <Env_Texture>; 
	MinFilter = Linear; 
	MagFilter = Linear;  
	MipFilter = Linear; 
}; 
 
texture S_Texture;
sampler S_Sampler = sampler_state 
{
	Texture = <S_Texture>; 
	MinFilter = Linear; 
	MagFilter = Linear;  
	MipFilter = Linear; 
}; 

texture G_Texture;
sampler G_Sampler = sampler_state 
{
 Texture = <G_Texture>; MinFilter = Linear; MagFilter = Linear;  MipFilter = Linear; 
 }; 


struct VS_INPUT 
{
	float4 Position : POSITION0;
	float2 TexCoord : TEXCOORD0;
	float3 Normal : NORMAL0; 
	float3 Tangent : TANGENT0;
	float4 BoneIndices : BLENDINDICES0;
	float4 BoneWeights : BLENDWEIGHT0;
};

struct VS_OUTPUT 
{
	float4 Position : POSITION0;
	float2 TexCoord : TEXCOORD0;
	float3 Data1 : TEXCOORD1;
	float3 Data2 : TEXCOORD2;
	float3 Data3 : TEXCOORD3;
};

VS_OUTPUT VSBasic(VS_INPUT input) {
	VS_OUTPUT output;
	float4x4 skinTransform = 0;
	skinTransform += Bones[input.BoneIndices.x] * input.BoneWeights.x;
	skinTransform += Bones[input.BoneIndices.y] * input.BoneWeights.y;
	skinTransform += Bones[input.BoneIndices.z] * input.BoneWeights.z;
	skinTransform += Bones[input.BoneIndices.w] * input.BoneWeights.w;
	
	float4 pos = mul(input.Position, skinTransform);
	float3 eyeLoc = mul(View._m30_m31_m32, transpose(View));
	float3 eyeDir = eyeLoc - pos;	
	pos = mul(pos,View);
	pos = mul(pos,Projection);
	float3 nml = mul(input.Normal, skinTransform);
	nml = normalize(nml);
	output.Position = pos;
	output.TexCoord = input.TexCoord;
	output.Data1 = nml;
	output.Data2 = eyeDir;
	output.Data3 = 0;
	return output;
}

VS_OUTPUT VSNormals(VS_INPUT input) {
	VS_OUTPUT output;
	float4x4 skinTransform = 0;
	skinTransform += Bones[input.BoneIndices.x] * input.BoneWeights.x;
	skinTransform += Bones[input.BoneIndices.y] * input.BoneWeights.y;
	skinTransform += Bones[input.BoneIndices.z] * input.BoneWeights.z;
	skinTransform += Bones[input.BoneIndices.w] * input.BoneWeights.w;
	
	float4 pos = mul(input.Position, skinTransform);
	float3 eyeLoc = mul(View._m30_m31_m32, transpose(View));
	float3 eyeDir = eyeLoc - pos;	
	pos = mul(pos,View);
	pos = mul(pos,Projection);
	
	float3x3 tgtSpace;
	tgtSpace[0] = mul(input.Tangent, skinTransform);
	tgtSpace[1] = mul(cross(input.Tangent,input.Normal),skinTransform);
	tgtSpace[2] = mul(input.Normal, skinTransform);
	
	float3 tgtLightDir1 = mul(tgtSpace, lightDir1);
	float3 tgtLightDir2 = mul(tgtSpace, lightDir2);
	
	output.Position = pos;
	output.TexCoord = input.TexCoord;
	output.Data1 = normalize(tgtLightDir1);
	output.Data2 = normalize(tgtLightDir2);
	output.Data3 = normalize(mul(tgtSpace,eyeDir));
	
	return output;
}

float4 PSShadow(VS_OUTPUT input) : COLOR0 {
  return float4(0,0,0,0.5);
}

float4 PSBasic(VS_OUTPUT input) : COLOR0 {
	float4 outColor = tex2D(C_Sampler, input.TexCoord);
  return outColor;
}

float4 PSDiffuse(VS_OUTPUT input) : COLOR0 {
  float4 outColor = tex2D(C_Sampler, input.TexCoord);
  float diffuse = saturate(dot(input.Data1, normalize(lightDir1))) +
                  saturate(dot(input.Data1, normalize(lightDir2)));
  return outColor * (diffuse * Kd);
}

float4 PSNormals(VS_OUTPUT input) : COLOR0 {
	float4 outColor = tex2D(C_Sampler, input.TexCoord);
  float3 normal = 2*tex2D(N_Sampler, input.TexCoord) - 1.0;
  
  float diffuse = saturate(dot(normal, input.Data1)) +
                  saturate(dot(normal, input.Data2));
  
  return outColor * (diffuse * Kd);
}

float4 PSSpecular(VS_OUTPUT input) : COLOR0 {
  float4 outColor = tex2D(C_Sampler, input.TexCoord);
  float diffuse = saturate(dot(input.Data1, normalize(lightDir1))) +
                  saturate(dot(input.Data1, normalize(lightDir2)));  
  float3 halfVector1 = normalize(lightDir1 + input.Data2);
  float3 halfVector2 = normalize(lightDir2 + input.Data2);
  float4 specular = pow(dot(input.Data1,halfVector1),specPow) +
                    pow(dot(input.Data1,halfVector2),specPow);
  return outColor * (diffuse * Kd) + (specular * Ks);
}

float4 PSSpecularNormals(VS_OUTPUT input) : COLOR0 {
  float4 outColor = tex2D(C_Sampler, input.TexCoord);
  float3 normal = 2*tex2D(N_Sampler, input.TexCoord) - 1.0;  
  float diffuse = saturate(dot(normal, input.Data1)) +
                  saturate(dot(normal, input.Data2));
  float3 halfVector1 = normalize(input.Data1 + input.Data3);
  float3 halfVector2 = normalize(input.Data2 + input.Data3);
  float4 specular = pow(dot(normal,halfVector1),specPow) 
                    + pow(dot(normal,halfVector2),specPow);  
  
  if (useSpecMap) specular *= tex2D(S_Sampler, input.TexCoord);                  
  
  return outColor * (diffuse * Kd) + (specular * Ks);
}

float4 PSRef(VS_OUTPUT input):COLOR0 {
  float4 outColor = PSDiffuse(input);
  float3 ref = reflect(normalize(input.Data2), input.Data1);
  float4 envColor = texCUBE(Env_Sampler, ref);
  return outColor + (envColor*Kr);
}

float4 PSRefNormals(VS_OUTPUT input) : COLOR0 {
  float3 normal = 2*tex2D(N_Sampler, input.TexCoord) - 1.0;  
  float4 outColor = PSNormals(input);
  float3 ref = reflect(input.Data3, normal);
  float4 envColor = texCUBE(Env_Sampler, ref);
  return outColor + (envColor * Kr);
}

float4 PSRefSpecularNormals(VS_OUTPUT input) : COLOR0 {
  float3 normal = 2*tex2D(N_Sampler, input.TexCoord) - 1.0; 
  float4 outColor = PSSpecularNormals(input);
  float3 ref = reflect(input.Data3, normal);
  float4 envColor = texCUBE(Env_Sampler, ref);
  return outColor + (envColor * Kr);
}

float4 PSGlow(VS_OUTPUT input) : COLOR0 {
	float4 outColor = tex2D(G_Sampler, input.TexCoord);
	outColor *= tex2D(C_Sampler, input.TexCoord);
	return outColor;
}

technique SkinnedModelTechnique
{
	pass SkinnedModelPass
	{
		VertexShader = compile vs_1_1 VSBasic();
		PixelShader = compile ps_2_0 PSBasic();
	}
}


technique Shadow
{
	pass Pass1
	{
		VertexShader = compile vs_1_1 VSBasic();
		PixelShader = compile ps_2_0 PSShadow();
	}
}

technique Diffuse 
{
  pass Pass1
  {
    VertexShader = compile vs_1_1 VSBasic();
    PixelShader = compile ps_2_0 PSDiffuse();
  }
}

technique Bump 
{
  pass Pass1
  {
    VertexShader = compile vs_1_1 VSNormals();
    PixelShader = compile ps_2_0 PSNormals();
  }
}

technique Shiny 
{
  pass Pass1
  {
    VertexShader = compile vs_1_1 VSBasic();
    PixelShader = compile ps_2_0 PSSpecular();
  }
}

technique ShinyBump 
{
  pass Pass1
  {
    VertexShader = compile vs_1_1 VSNormals();
    PixelShader = compile ps_2_0 PSSpecularNormals();
  }
}

technique Ref 
{
  pass Pass1
  {
    VertexShader = compile vs_1_1 VSBasic();
    PixelShader = compile ps_2_0 PSRef();
  }
}

technique RefBump
{
  pass Pass1
  {
    VertexShader = compile vs_1_1 VSNormals();
    PixelShader = compile ps_2_0 PSRefNormals();
  }
}

technique RefShinyBump
{
  pass Pass1
  {
    VertexShader = compile vs_1_1 VSNormals();
    PixelShader = compile ps_2_0 PSRefSpecularNormals();
  }
}

technique Glow
{
  pass Pass1
  {
    VertexShader = compile vs_1_1 VSBasic();
    PixelShader = compile ps_2_0 PSGlow();
  }
}

