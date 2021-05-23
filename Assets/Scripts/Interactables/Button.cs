using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(SpriteRenderer))]
public class Button : Interactable
{
    [SerializeField] ButtonExecutionType executionType;
    [SerializeField] int sceneToUnload = -1, sceneToLoad = -1;
    [SerializeField] Color upColor, downColor, disabledColor;

    SpriteRenderer myRenderer;
    public bool Enabled { get; set; } = true;
    [HideInInspector] public int levelToLoad = -1;

    private void Awake() => myRenderer = GetComponent<SpriteRenderer>();
    private void OnEnable() => SetState(true);

    public void SetState(bool condition)
    {
        Enabled = condition;

        if(!Enabled)
            myRenderer.material.color = disabledColor;
        else
            myRenderer.material.color = upColor;
    }

    protected override void FingerDown(PointerEventData eventData)
    {
        if (!Enabled)
            return;

        Execute();
        myRenderer.material.color = downColor;
    }

    protected override void FingerUp(PointerEventData eventData)
    {
        if (!Enabled)
            return;

        myRenderer.material.color = upColor;
    }

    void Execute()
    {
        switch (executionType)
        {
            case ButtonExecutionType.LoadScene:
                GameManager.Instance.ChangeScene(sceneToUnload, sceneToLoad);
                break;
            case ButtonExecutionType.BackToMainScene:
                GameManager.Instance.ChangeScene(gameObject.scene.buildIndex, 0);
                break;
            case ButtonExecutionType.LoadLevel:
                GameManager.Instance.LoadLevel(levelToLoad);
                break;
            default:
                break;
        }
    }
}

public enum ButtonExecutionType
{
    LoadScene,
    BackToMainScene,
    LoadLevel
}