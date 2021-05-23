using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class LevelCelebration : MonoBehaviour
{
    [SerializeField] GameObject transparentBackground;
    [SerializeField] GameObject animatedScoreBackground;
    [SerializeField] LayoutDesigner highScoreLayout;
    [SerializeField] ParticleSystem explosion, shower;

    public void PlayCelebration(int highScore, TweenCallback callback)
    {
        transparentBackground.transform.localScale = new Vector3(GameManager.ScreenWorldSize.x, GameManager.ScreenWorldSize.y, 1f);
        transparentBackground.GetComponent<Renderer>().sortingLayerName = "OnTop";

        var animatedRenderer = animatedScoreBackground.GetComponent<Renderer>();
        animatedRenderer.sortingLayerName = "OnTop";
        animatedRenderer.sortingOrder = 1;

        var animatedMat = animatedScoreBackground.GetComponent<Renderer>().material;
        var animatedFinalScale = new Vector3(GameManager.ScreenWorldSize.x * highScoreLayout.WidthRatio,
                                             GameManager.ScreenWorldSize.y * highScoreLayout.HeightRatio,
                                             1);

        var highScoreText = highScoreLayout.RetrieveComponentFromPairByIndex(0) as TextMeshPro;
        var highScoreAmountText = highScoreLayout.RetrieveComponentFromPairByIndex(1) as TextMeshPro;

        var backgroundMat = transparentBackground.GetComponent<Renderer>().material;

        Sequence celebration = DOTween.Sequence();
        celebration.Append(backgroundMat.DOFade(0.6f, PerObjectMaterialProperties.baseColorId, 1));
        celebration.Append(animatedScoreBackground.transform.DOScale(animatedFinalScale, 1).SetEase(Ease.InOutBounce));
        celebration.Join(animatedMat.DOFade(1, PerObjectMaterialProperties.baseColorId, 1));
        celebration.Append(highScoreText.DOText("High\nScore:", 1).OnComplete(PlayParticles));
        celebration.Append(highScoreAmountText.DOText($"{highScore}", 1));
        celebration.AppendInterval(5);
        celebration.OnComplete(callback);

        celebration.Play();
    }

    void PlayParticles()
    {
        explosion.Play();
        shower.Play();
    }
}
