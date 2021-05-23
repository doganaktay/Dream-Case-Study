Shader "Archi/UnlitBackground" {
Properties {
    _MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
    _BaseColor ("Multiplied Alpha", Color) = (0, 0, 0, 1)
}

SubShader {
    Tags {"Queue"="Opaque" "IgnoreProjector"="True" "RenderType"="Opaque"}
    LOD 100
    
    ZWrite Off
    Blend SrcAlpha OneMinusSrcAlpha

    Pass {
        Lighting Off
        Color [_BaseColor]
        SetTexture [_MainTex] { 
            combine texture, texture * primary
        }
    }
}
}