using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LayoutDesigner : MonoBehaviour
{
    public List<ExposedGOComponentPair> exposedComponents;
    [SerializeField] HorizontalAlignment horizontalAlignment;
    [SerializeField] VerticalAlignment verticalAlignment;
    [SerializeField] LayoutSpacing spacing;
    [SerializeField] LayoutScaling scaling;

    [SerializeField, Range(0,1)] float heightRatio;
    public float HeightRatio { get { return heightRatio; } set { heightRatio = value; } }
    [SerializeField, Range(0,1)] float widthRatio;
    public float WidthRatio { get { return widthRatio; } set { widthRatio = value; } }
    [SerializeField, Range(-1f, 1f)] float childVerticalDisplacement;

    [SerializeField] bool hasBackground;
    [SerializeField] Color backgroundColor;
    public Color BackgroundColor { get { return backgroundColor; } set { backgroundColor = value; } }
    [SerializeField] Material backgroundMaterial;
    GameObject background;

    public int ChildCount => transform.childCount;

    [SerializeField] bool overrideVerticalAlignment = false;

    [SerializeField] float[] childAlignPercent;
    public void InitChildAlignmentArray(int size) => childAlignPercent = size > 0 ? new float[size] : null;
    public void SetChildAlignmentArray(float[] alignments) => childAlignPercent = alignments;

    [SerializeField] bool[] childScaling;
    public void InitChildScalingArray(int size) => childScaling = size > 0 ? new bool[size] : null;
    public void SetChildScalingArray(bool[] scales) => childScaling = scales;

    //private void OnValidate()
    //{
    //    if (Application.isPlaying)
    //    {
    //        if (hasBackground)
    //            SetBackgroundSize();

    //        DistributeChildren();
    //    }
    //}

    void Awake()
    {
        if (hasBackground)
        {
            CreateBackground();
            SetBackgroundSize();
        }

        DistributeChildren();
    }

    void DistributeChildren()
    {
        int firstChildIndex = hasBackground ? 1 : 0;

        switch (spacing)
        {
            case LayoutSpacing.Equal:
                var screenWidth = GameManager.ScreenSize.x * widthRatio;
                var screenHeight = GameManager.ScreenSize.y * heightRatio;
                var worldWidth = GameManager.ScreenWorldSize.x * widthRatio;
                var worldHeight = GameManager.ScreenWorldSize.y * heightRatio;
                var perCell = screenWidth / (ChildCount - firstChildIndex);
                float start = GetHorizontalStartPos();
                float rectX = GameManager.ScreenWorldSize.x * widthRatio / (ChildCount - firstChildIndex);
                float rectY = GameManager.ScreenWorldSize.y * heightRatio;

                for (int i = firstChildIndex, j = 0; i < ChildCount; i++, j++)
                {
                    var xPos = start + j * perCell + perCell / 2f;
                    var yPos = GetVerticalScreenPos() + (screenHeight / 2f * childVerticalDisplacement);
                    var worldPos = GameManager.MainCam.ScreenToWorldPoint(new Vector2(xPos, yPos));

                    if(!overrideVerticalAlignment)
                        transform.GetChild(i).position = new Vector3(worldPos.x, worldPos.y, transform.position.z);
                    else
                    {
                        var worldY = transform.position.y + (worldHeight / 2 * childVerticalDisplacement);
                        transform.GetChild(i).position = new Vector3(worldPos.x, worldY, transform.position.z);
                    }

                    if (scaling == LayoutScaling.All || (i < childScaling.Length && childScaling[i]))
                        transform.GetChild(i).localScale = new Vector3(rectX, rectY, 1f);
                }

                RectTransform[] rects = GetComponentsInChildren<RectTransform>();
                var length = rects.Length;

                for (int i = 0; i < length; i++)
                    rects[i].sizeDelta = new Vector2(rectX, 0);

                break;

            case LayoutSpacing.Custom:
                screenWidth = GameManager.ScreenSize.x * widthRatio;
                screenHeight = GameManager.ScreenSize.y * heightRatio;
                worldWidth = GameManager.ScreenWorldSize.x * widthRatio;
                worldHeight = GameManager.ScreenWorldSize.y * heightRatio;
                start = GetHorizontalStartPos();
                for(int i = firstChildIndex, j = 0; i < ChildCount; i++, j++)
                {
                    var xPos = start + (childAlignPercent[j] * screenWidth / 2f);
                    var yPos = GetVerticalScreenPos() + (screenHeight / 2f * childVerticalDisplacement);
                    var worldPos = GameManager.MainCam.ScreenToWorldPoint(new Vector2(xPos, yPos));

                    if(!overrideVerticalAlignment)
                        transform.GetChild(i).position = new Vector3(worldPos.x, worldPos.y, transform.position.z);
                    else
                    {
                        var worldY = transform.position.y + (worldHeight / 2 * childVerticalDisplacement);
                        transform.GetChild(i).position = new Vector3(worldPos.x, worldY, transform.position.z);
                    }

                    start += childAlignPercent[j] * screenWidth;

                    if (scaling == LayoutScaling.All || (i < childScaling.Length && childScaling[i]))
                    {
                        rectX = GameManager.ScreenWorldSize.x * widthRatio * childAlignPercent[j];
                        rectY = GameManager.ScreenWorldSize.y * heightRatio;
                        transform.GetChild(i).localScale = new Vector3(rectX, rectY, 1f);

                        rects = transform.GetChild(i).GetComponentsInChildren<RectTransform>();
                        length = rects.Length;

                        for (i = 0; i < length; i++)
                            rects[i].sizeDelta = new Vector2(rectX, 0);
                    }

                }
                break;
        }
    }

    void CreateBackground()
    {
        Material mat;

        if(backgroundMaterial == null)
        {
            var shader = Shader.Find("Unlit/Color");
            mat = new Material(shader);
        }
        else
            mat = backgroundMaterial;
        

        background = new GameObject();
        background.name = "Background";
        background.transform.SetParent(transform);
        background.transform.SetAsFirstSibling();
        var renderer = background.AddComponent<MeshRenderer>();
        var filter = background.AddComponent<MeshFilter>();
        var props = background.AddComponent<PerObjectMaterialProperties>();
        props.myRenderer = renderer;
        filter.mesh = LevelUtility.PrimitiveQuad;
        renderer.material = mat;
        props.SetBaseColor(backgroundColor);
    }

    private void SetBackgroundSize()
    {
        var heightScale = GameManager.ScreenWorldSize.y * heightRatio;
        var widthScale = GameManager.ScreenWorldSize.x * widthRatio;

        var worldPoint = GameManager.MainCam.ScreenToWorldPoint(GetAlignedScreenPos());

        if(!overrideVerticalAlignment)
            background.transform.position = new Vector3(worldPoint.x, worldPoint.y, transform.position.z + 1f);
        else
            background.transform.position = new Vector3(worldPoint.x, transform.position.y, transform.position.z + 1f);

        background.transform.localScale = new Vector3(widthScale, heightScale, 1);
    }

    Vector2 GetAlignedScreenPos() => new Vector2(GetHorizontalScreenPos(), GetVerticalScreenPos());

    float GetHorizontalStartPos()
    {
        var width = GameManager.ScreenSize.x * widthRatio;

        switch (horizontalAlignment)
        {
            case HorizontalAlignment.Left:
                return 0;
            case HorizontalAlignment.Center:
                return (GameManager.ScreenSize.x - width) / 2f;
            case HorizontalAlignment.Right:
                return GameManager.ScreenSize.x - width;
            default:
                return 0;
        }
    }

    float GetHorizontalScreenPos()
    {
        switch (horizontalAlignment)
        {
            case HorizontalAlignment.Left:
                return GameManager.ScreenSize.x * widthRatio / 2f;
            case HorizontalAlignment.Center:
                return GameManager.ScreenSize.x / 2f;
            case HorizontalAlignment.Right:
                return GameManager.ScreenSize.x - (GameManager.ScreenSize.x * widthRatio / 2f);
            default:
                return 0;
        }
    }

    float GetVerticalScreenPos()
    {
        switch (verticalAlignment)
        {
            case VerticalAlignment.Bottom:
                return GameManager.ScreenSize.y * heightRatio / 2f;
            case VerticalAlignment.Center:
                return GameManager.ScreenSize.y / 2f;
            case VerticalAlignment.Top:
                return GameManager.ScreenSize.y - (GameManager.ScreenSize.y * heightRatio / 2f);
            default:
                return 0;
        }
    }

    public Component RetrieveComponentFromPairByIndex(int index) 
    {
        if (index < 0 || index >= exposedComponents.Count)
            throw new ArgumentOutOfRangeException();

        var component = exposedComponents[index].component;

        return component;
    }
}

// serializable class to hold exposed game object - component pairs
[System.Serializable]
public class ExposedGOComponentPair
{
    public GameObject gameObject;
    public Component component;

    public ExposedGOComponentPair() { }

    public ExposedGOComponentPair(GameObject go, Component comp)
    {
        gameObject = go;
        component = comp;
    }
}

// Layout control enums

public enum LayoutSpacing
{
    Equal,
    Custom
}

public enum LayoutScaling
{
    All,
    Custom
}

public enum HorizontalAlignment
{
    Left,
    Center,
    Right
}

public enum VerticalAlignment
{
    Top,
    Center,
    Bottom
}
