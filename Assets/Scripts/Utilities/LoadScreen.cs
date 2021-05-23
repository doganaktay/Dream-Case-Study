using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class LoadScreen : MonoBehaviour
{
    [SerializeField] PerObjectMaterialProperties backgroundProps;
    [SerializeField] TextMeshPro textMesh;
    [SerializeField] float fadeInTime = 0.5f, fadeOutTime = 1f;
    [SerializeField] bool forceFadeOutDelay = false;
    [SerializeField] float forcedDelay;

    public void FadeIn() => StartCoroutine(FadeInRoutine());
    public void FadeOut() => StartCoroutine(FadeOutRoutine());

    private IEnumerator FadeInRoutine()
    {
        var textColor = textMesh.color;

        float currentTime = 0f;

        while(currentTime <= fadeInTime)
        {
            currentTime += Time.deltaTime;

            float alphaPercent = currentTime / fadeInTime;

            backgroundProps.SetBaseColorAlpha(alphaPercent);

            textMesh.color = new Color(textColor.r, textColor.g, textColor.b, alphaPercent);

            yield return null;
        }
    }

    private IEnumerator FadeOutRoutine()
    {
        if (forceFadeOutDelay)
            yield return new WaitForSeconds(forcedDelay);

        var textColor = textMesh.color;
        float currentTime = 0f;

        while (currentTime <= fadeOutTime)
        {
            currentTime += Time.deltaTime;

            float alphaPercent = 1 - currentTime / fadeOutTime;

            backgroundProps.SetBaseColorAlpha(alphaPercent);

            textMesh.color = new Color(textColor.r, textColor.g, textColor.b, alphaPercent);

            yield return null;
        }
    }
}
