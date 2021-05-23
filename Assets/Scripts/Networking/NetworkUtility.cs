using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public static class NetworkUtility
{
    public static readonly string LevelsServerPath = "https://row-match.s3.amazonaws.com/levels/";
    public static readonly string UnsplashAPIPath = "https://source.unsplash.com/random/";
    public static readonly string[] UnSplashKeyWords = new string[] { "nature", "water", "animals"};

    public static bool IsConnected => Application.internetReachability != NetworkReachability.NotReachable;

    /// <summary>
    /// Takes an array of file names with one callback called for each,
    /// and a final callback called once for OnComplete
    /// </summary>
    /// <param name="fileNames"></param>
    /// <param name="callback"></param>
    /// <param name="onComplete"></param>
    /// <returns></returns>
    public static IEnumerator RequestLevels(string[] fileNames, Action<UnityWebRequest, string> callback, Action onComplete)
    {
        for(int i = 0; i < fileNames.Length; i++)
            yield return RequestLevel(fileNames[i], callback);

        onComplete();
    }

    public static IEnumerator RequestLevel(string fileName, Action<UnityWebRequest,string> callback)
    {
        var path = LevelsServerPath + fileName;

        using(UnityWebRequest request = UnityWebRequest.Get(path))
        {
            yield return request.SendWebRequest();

            if (request.isHttpError || request.isNetworkError)
                Debug.LogWarning($"{request.error}: {request.downloadHandler.text}");
            else
                callback(request,fileName);
        }
    }

    public static IEnumerator GetRandomImageFromUnsplash(Action<UnityWebRequest> callback)
    {
        var path = UnsplashAPIPath + GameManager.ScreenSize.x + "x" + GameManager.ScreenSize.y;
        path += "/?";

        for(int i = 0; i < UnSplashKeyWords.Length; i++)
        {
            path += UnSplashKeyWords[i];

            if (i < UnSplashKeyWords.Length - 1)
                path += ",";
        }

        using (UnityWebRequest request = UnityWebRequest.Get(path))
        {
            yield return request.SendWebRequest();

            if (request.isHttpError || request.isNetworkError)
                Debug.LogWarning($"{request.error}: {request.downloadHandler.text}");
            else
                callback(request);
            
        }
    }
}
