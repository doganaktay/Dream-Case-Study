using UnityEngine;

[DisallowMultipleComponent, RequireComponent(typeof(Renderer))]
public class PerObjectMaterialProperties : MonoBehaviour
{
    public static readonly int baseColorId = Shader.PropertyToID("_BaseColor");
    public static readonly int secondaryColorId = Shader.PropertyToID("_SecondaryColor");
    public static readonly int gridWidthId = Shader.PropertyToID("_GridWidth");
    public static readonly int gridHeightId = Shader.PropertyToID("_GridHeight");
    public static readonly int mainTexId = Shader.PropertyToID("_MainTex");

    static MaterialPropertyBlock block;

    [HideInInspector] public Renderer myRenderer;

    [Header("Settings")]
    [SerializeField] Texture2D texture;
    [SerializeField] Color BaseColor;
    [SerializeField] Color SecondaryColor;

    private void Awake()
    {
        if (block == null)
            block = new MaterialPropertyBlock();

        myRenderer = GetComponent<Renderer>();
        block.Clear();

        if(myRenderer.material.HasProperty(mainTexId) && texture != null)
            block.SetTexture(mainTexId, texture);

        if (myRenderer.material.HasProperty(baseColorId))
            block.SetColor(baseColorId, BaseColor);

        if (myRenderer.material.HasProperty(secondaryColorId))
            block.SetColor(secondaryColorId, SecondaryColor);
        
        myRenderer.SetPropertyBlock(block);
    }

    #region Setters

    public void SetGridSize(Vector2Int size)
    {
        block.Clear();
        myRenderer.GetPropertyBlock(block);

        block.SetFloat(gridWidthId, size.x);
        block.SetFloat(gridHeightId, size.y);
        myRenderer.SetPropertyBlock(block);
    }

    public void SetBaseColor()
    {
        block.Clear();
        myRenderer.GetPropertyBlock(block);

        block.SetColor(baseColorId, BaseColor);
        myRenderer.SetPropertyBlock(block);
    }

    public void SetBaseColor(Color color)
    {
        block.Clear();
        myRenderer.GetPropertyBlock(block);

        block.SetColor(baseColorId, color);
        myRenderer.SetPropertyBlock(block);
    }

    public void SetBaseColorAlpha(float alpha)
    {
        block.Clear();
        myRenderer.GetPropertyBlock(block);

        Color newColor = new Color(BaseColor.r, BaseColor.g, BaseColor.b, alpha);

        block.SetColor(baseColorId, newColor);
        myRenderer.SetPropertyBlock(block);
    }

    public void SetSecondaryColor()
    {
        block.Clear();
        myRenderer.GetPropertyBlock(block);

        block.SetColor(secondaryColorId, SecondaryColor);
        myRenderer.SetPropertyBlock(block);
    }

    public void SetSecondaryColor(Color color)
    {
        block.Clear();
        myRenderer.GetPropertyBlock(block);

        block.SetColor(secondaryColorId, color);
        myRenderer.SetPropertyBlock(block);
    }

    public void SetTexture()
    {
        block.Clear();
        myRenderer.GetPropertyBlock(block);

        block.SetTexture(mainTexId, texture);
        myRenderer.SetPropertyBlock(block);
    }

    public void SetTexture(Texture2D texture)
    {
        block.Clear();
        myRenderer.GetPropertyBlock(block);

        block.SetTexture(mainTexId, texture);
        myRenderer.SetPropertyBlock(block);
    }

    #endregion
}
