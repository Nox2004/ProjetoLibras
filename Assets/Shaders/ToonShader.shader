Shader "Custom/ToonShader"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
		_SecondTex ("Second Texture", 2D) = "white" {} //a texture that will be drawn above normal texture (for stickers and stuff) (white with 0 alpha by default)
		_DrawSecondTex ("Draw Second Texture", Range(0, 1)) = 0
        _ToonLut ("Toon LUT", 2D) = "white" {}
		_AlphaClipping ("Alpha Clipping", Range(0, 1)) = 0
        _RimColor ("Rim Color", Color) = (1,1,1,1)
		_RimPower ("Rim Power", Range(0, 10)) = 1
        // _OutColor ("Outline Color", Color) = (0,0,0,1)
        // _OutWidth ("Outline Width", Range(0.0, 0.2)) = 0.01
    }
    SubShader
    {
        Pass
		{
            Tags
			{
				"LightMode" = "ForwardBase"
			}

            CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fwdbase

            #include "UnityCG.cginc"
			#include "Lighting.cginc"
            struct appdata
			{
				float4 vertex : POSITION;
				float3 normal: NORMAL;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				float3 normal : TEXCOORD1;
				float3 viewDir : TEXCOORD2;
			};

			sampler2D _MainTex;
			sampler2D _SecondTex;
			sampler2D _ToonLut;
			
			half3 _RimColor;
			half _RimPower;
			half _AlphaClipping;

			fixed4 _Color;
			fixed4 _SecondTexColor;
			
			float _UVOffsetX;
			float _UVOffsetY;
			float _WhiteEffectStrength;
			int _DrawSecondTex;

			v2f vert (appdata v)
			{
				v2f o;

				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				o.normal = UnityObjectToWorldNormal(v.normal);
				o.viewDir = normalize(UnityWorldSpaceViewDir(mul(unity_ObjectToWorld, v.vertex)));

				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				float3 normal = normalize(i.normal);
				float ndotl = dot(normal, _WorldSpaceLightPos0);
				float ndotv = saturate(dot(normal, i.viewDir));

				float3 lut = tex2D(_ToonLut, float2(ndotl, 0));
				float3 rim = _RimColor * pow(1 - ndotv, _RimPower) * ndotl;

				float3 directDiffuse = lut * _LightColor0;
				float3 indirectDiffuse = unity_AmbientSky;

				fixed4 col = tex2D(_MainTex, i.uv+float2(_UVOffsetX,_UVOffsetY)) * _Color;
				
				//if second texture is not white, draw it above the main texture
				if (_DrawSecondTex)
				{
					fixed4 tex2 = tex2D(_SecondTex, i.uv) * _SecondTexColor;
					col = lerp(col, tex2, tex2.a);
				}
				
				if (_AlphaClipping >= 1) clip(-1); //if alpha clipping is 1 or more (????), always clip
				clip(col.a - _AlphaClipping);

				col.rgb *= directDiffuse + indirectDiffuse;
				col.rgb += rim;
				col.a = 1.0;
				
				col.rgb = lerp(col.rgb, float3(1, 1, 1), _WhiteEffectStrength);

				return col;
			}
			ENDCG
        }
    }
    FallBack "Diffuse"
}
