Shader "Archi/LayoutMask"
{
    Properties
    {
        [IntRange] _StencilRef ("Stencil Reference Value", Range(0,255)) = 0
    }
    SubShader
    {
        Tags
        {
            "Queue"="Geometry"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
        }

        ColorMask 0
        ZWrite Off
        Cull off

        Stencil
        {
            Ref [_StencilRef]
            Comp always
            Pass replace
        }

        Pass
        {
            
        }
    }
}
