using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneAsyncUtility
{
    public static IEnumerator LoadScene(int sceneIndex, bool loadAdditive = true)
    {
        var load = SceneManager.LoadSceneAsync(sceneIndex, loadAdditive ? LoadSceneMode.Additive : LoadSceneMode.Single);

        while (!load.isDone)
            yield return null;
    }

    public static IEnumerator UnloadScene(int sceneIndex)
    {
        var unload = SceneManager.UnloadSceneAsync(sceneIndex);

        while (!unload.isDone)
            yield return null;
    }
}
