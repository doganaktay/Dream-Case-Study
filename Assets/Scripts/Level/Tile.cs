using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Tile : MonoBehaviour
{
    [HideInInspector] public TileData data;
    public Vector2Int gridPosition;
    public bool IsMatched { get; set; }

    SpriteRenderer myRenderer;
    int startingSortingOrder;

    private void Awake()
    {
        myRenderer = transform.GetComponent<SpriteRenderer>();

        startingSortingOrder = myRenderer.sortingOrder;
    }

    public void MoveTween(Vector3 target, float duration)
    {
        var sequence = DOTween.Sequence();
        sequence.Append(transform.DOMove(target, duration));
        sequence.Insert(0, transform.DOShakeScale(duration, 0.3f));
        sequence.Play();
    }

    public void MatchTween(float duration, float delay)
    {
        myRenderer.sortingOrder = 10;

        var sequence = DOTween.Sequence();
        //sequence.Append(transform.DOShakeScale(duration, .5f, 1, 10, true));
        sequence.AppendInterval(delay);
        sequence.AppendInterval(0.5f);
        sequence.Append(transform.DOPunchPosition(Vector2.up, duration, 1, 0.1f));
        sequence.Join(myRenderer.DOColor(Color.white, duration)).OnComplete(ResetSortingOrder);

        sequence.Play();
    }

    void ResetSortingOrder() => myRenderer.sortingOrder = startingSortingOrder;
}
