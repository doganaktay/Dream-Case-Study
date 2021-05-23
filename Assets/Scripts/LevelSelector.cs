using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using TMPro;
using DG.Tweening;

public class LevelSelector : Interactable
{
    [SerializeField, Range(0,1)] float widthRatio, heightRatio;
    [SerializeField, Range(0, 1)] float widthMarginRatio, heightMarginRatio;
    [SerializeField] float startMarginMultiplier = 4f;
    [SerializeField] GameObject viewportQuad;
    [SerializeField] GameObject contentBackground, contentMask, content;
    [SerializeField] LayoutDesigner itemPrefab;
    [SerializeField] float itemSpacing;
    [SerializeField] float scrollSpeedScalar = 0.1f;
    [SerializeField] float maxScrollDelta = 200f;

    List<LayoutDesigner> levelItems = new List<LayoutDesigner>();
    float contentTopYLimit, contentBottomYLimit;

    private void Awake()
    {
        SetViewportAndContentDimensions();
        CreateLevelList();
    }

    protected override void FingerDrag(PointerEventData eventData)
    {
        var delta = eventData.delta;
        var deltaY = ScaleScrollDelta(delta.y);

        MoveItemContainer(deltaY);
    }

    void MoveItemContainer(float delta)
    {
        content.transform.DOMoveY(content.transform.position.y + delta, 1 - (delta / maxScrollDelta)).SetEase(Ease.OutQuart).SetAutoKill(true);
    }

    float ScaleScrollDelta(float deltaY)
    {
        var sign = Mathf.Sign(deltaY);
        deltaY = Mathf.Min(Mathf.Abs(deltaY), maxScrollDelta);
        deltaY *= sign;

        if (levelItems[0].transform.position.y + deltaY < contentTopYLimit)
            deltaY = contentTopYLimit - levelItems[0].transform.position.y;
        else if (levelItems[levelItems.Count - 1].transform.position.y + deltaY > contentBottomYLimit)
            deltaY = contentBottomYLimit - levelItems[levelItems.Count - 1].transform.position.y;

        return deltaY;
    }

    void CreateLevelList()
    {
        int i = 0;
        foreach(var level in LevelUtility.GetAvailableLevelsSorted())
        {
            contentTopYLimit = GameManager.ScreenWorldSize.y * (heightRatio - heightMarginRatio * 2f * startMarginMultiplier) / 2f;
            contentBottomYLimit = -GameManager.ScreenWorldSize.y * (heightRatio - heightMarginRatio * 2f * startMarginMultiplier) / 2f;

            var highScore = GameManager.Instance.Save.RequestHighScore(level.Key);
            float yPos = transform.position.y
                       + contentTopYLimit - (i * itemSpacing);
            var item = CreateLevelItem(level.Value, highScore, new Vector3(transform.position.x, yPos, -1f));
            levelItems.Add(item);

            i++;
        }
    }

    LayoutDesigner CreateLevelItem(LevelData data, int highScore, Vector3 position)
    {
        var layout = Instantiate(itemPrefab, position, Quaternion.identity);
        layout.transform.SetParent(content.transform);
        layout.gameObject.name = $"Selection: Level {data.levelIndex}";

        var components = layout.GetComponentsInChildren<Renderer>();
        foreach(var comp in components)
            comp.GetComponent<Renderer>().sortingLayerName = "Foreground";

        var topText = layout.RetrieveComponentFromPairByIndex(0) as TextMeshPro;
        var bottomText = layout.RetrieveComponentFromPairByIndex(1) as TextMeshPro;
        topText.Update($"Level {data.levelIndex} - {data.moveCount} Moves");
        bottomText.Update($"High Score: {highScore}");

        var button = layout.RetrieveComponentFromPairByIndex(2) as Button;
        button.levelToLoad = data.levelIndex;

        bool canPlay = data.levelIndex <= GameManager.MaxAvailableLevel;
        button.SetState(canPlay);
        var buttonText = layout.RetrieveComponentFromPairByIndex(3) as TextMeshPro;
        buttonText.Update(canPlay ? "Play" : "Locked");

        return layout;
    }

    void SetViewportAndContentDimensions()
    {
        var width = GameManager.ScreenWorldSize.x * widthRatio;
        var height = GameManager.ScreenWorldSize.y * heightRatio;
        viewportQuad.transform.localScale = new Vector3(width, height, 1f);
        viewportQuad.GetComponent<Renderer>().sortingLayerName = "Middleground";

        width = GameManager.ScreenWorldSize.x * (widthRatio - widthMarginRatio);
        height = GameManager.ScreenWorldSize.y * (heightRatio - heightMarginRatio);
        contentBackground.transform.localScale = new Vector3(width, height, 1f);
        contentBackground.GetComponent<Renderer>().sortingLayerName = "Middleground";
        contentBackground.GetComponent<Renderer>().sortingOrder = 1;

        contentMask.transform.localScale = new Vector3(width, height, 1f);
        content.transform.localScale = new Vector3(width, height, 1f);
    }
}
