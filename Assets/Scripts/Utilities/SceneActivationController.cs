using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneActivationController : MonoBehaviour
{
    [SerializeField] List<int> activeInScenes;

    private void Awake() => GameManager.PreSceneChange += PreSceneChange;
    private void OnDestroy() => GameManager.PreSceneChange -= PreSceneChange;

    void PreSceneChange(int nextIndex)
    {
        if(activeInScenes != null)
        {
            if (activeInScenes.Contains(nextIndex) && !gameObject.activeSelf)
                gameObject.SetActive(true);
            else if (gameObject.activeSelf)
                gameObject.SetActive(false);
        }
    }
}
