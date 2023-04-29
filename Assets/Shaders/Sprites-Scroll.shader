// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "Sprites/Scroll"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        [HideInInspector] _Color ("Tint", Color) = (1,1,1,1)
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
        [HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip ("Flip", Vector) = (1,1,1,1)
        [PerRendererData] _AlphaTex ("External Alpha", 2D) = "white" {}
        [PerRendererData] _EnableExternalAlpha ("Enable External Alpha", Float) = 0
		_ScrollSpeedX ("Scroll Speed X", Range(-10,10)) = 1
		_ScrollSpeedY ("Scroll Speed Y", Range(-10,10)) = 0
		_TilingX ("Tiling X", Range(0,10)) = 1
		_TilingY ("Tiling Y", Range(0,10)) = 1
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
		ZTest LEqual
		Blend One OneMinusSrcAlpha

        Pass
        {
        CGPROGRAM
            #pragma vertex SpriteVert
            #pragma fragment SpriteFragHurt
            #pragma target 2.0
            #pragma multi_compile_instancing
            #pragma multi_compile_local _ PIXELSNAP_ON
            #pragma multi_compile _ ETC1_EXTERNAL_ALPHA
            #include "UnitySprites.cginc"

		
		half _ScrollSpeedX;
		half _ScrollSpeedY;
		half _TilingX;
		half _TilingY;
		
		fixed4 SpriteFragHurt(v2f IN) : SV_Target
		{
			half2 uv =half2(IN.texcoord.x * _TilingX + _ScrollSpeedX * _Time.y, IN.texcoord.y * _TilingY + _ScrollSpeedY * _Time.y);
			fixed4 c = SampleSpriteTexture(uv) * IN.color;
			c.rgb *= c.a;
			return c;
		}
		ENDCG
        }
    }
}
