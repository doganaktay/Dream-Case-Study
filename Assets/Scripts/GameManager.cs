using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.AddressableAssets;
using Archi.IO;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public static readonly string saveFilePath = "save.royal";
    SaveData save;
    public SaveData Save => save;

    public static Action<int> PreSceneChange;

    public Level levelPrefab;
    Level currentLevel;
    public Level CurrentLevel { get { return currentLevel; } set { currentLevel = value; } }
    public static int MaxAvailableLevel { get; private set; }

    public static Camera MainCam;
    public static Vector2 ScreenSize;
    public static Vector2 ScreenWorldSize;

    public static Texture2D[] localBackgrounds;
    public static List<Texture2D> downloadedBackgrounds;
    static int bgImageIndex = 0;

    [SerializeField] GameObject gameBackground;
    [SerializeField] GameObject loadScreenBackground;
    [SerializeField] LoadScreen loadScreen;

    void Awake()
    {
        Instance = this;

        MainCam = Camera.main;
        ScreenSize = new Vector2(Screen.width, Screen.height);
        ScreenWorldSize.y = Camera.main.orthographicSize * 2f;
        ScreenWorldSize.x = ScreenWorldSize.y * ScreenSize.x / ScreenSize.y;

        save = SaveSystem.Load(saveFilePath);
        if (save == null)
            save = new SaveData();

        MaxAvailableLevel = save.topAvailableLevelIndex;

        StartCoroutine(LevelUtility.LoadLocalLevelsAsync());

        if (NetworkUtility.IsConnected && !LevelUtility.AllLevelsDownloaded())
        {
            var fileNames = LevelUtility.GetFileNames(new LevelRequest("RM_A", 11), new LevelRequest("RM_B"));
            StartCoroutine(NetworkUtility.RequestLevels(fileNames, LevelUtility.SaveLevel, LevelUtility.LoadDownloadedLevels));
        }
        else
            LevelUtility.LoadDownloadedLevels();

        loadScreenBackground.transform.localScale = new Vector3(ScreenWorldSize.x, ScreenWorldSize.y, 1);
        loadScreenBackground.GetComponent<Renderer>().sortingLayerName = "Loading";

        gameBackground.transform.localScale = new Vector3(ScreenWorldSize.x, ScreenWorldSize.y, 1);
        gameBackground.GetComponent<Renderer>().sortingLayerName = "Background";

        StartCoroutine(LoadLocalTextures());
        StartCoroutine(NetworkUtility.GetRandomImageFromUnsplash(LoadTexture));
    }

    void OnDestroy()
    {
        SaveSystem.Save(saveFilePath, save);
    }

    void OnLevelComplete()
    {
        save.SubmitScore(currentLevel.data.levelIndex, currentLevel.CurrentScore);
        MaxAvailableLevel = save.topAvailableLevelIndex = currentLevel.data.levelIndex + 1;
    }

    public void InitLevel(int levelIndex)
    {
        var data = LevelUtility.AvailableLevels[levelIndex];
        currentLevel.gameObject.name = "Level " + data.levelIndex;

        if (save.highScores.ContainsKey(data.levelIndex))
            currentLevel.Init(data, save.highScores[data.levelIndex]);
        else
            currentLevel.Init(data, 0);

        currentLevel.OnLevelComplete += OnLevelComplete;
    }

    public void LoadLevel(int levelIndex) => StartCoroutine(LoadLevelRoutine(levelIndex));

    public void ChangeScene(int sceneIndexToUnload = -1, int sceneIndexToLoad = -1) => StartCoroutine(ChangeSceneRoutine(sceneIndexToUnload, sceneIndexToLoad));

    IEnumerator ChangeSceneRoutine(int sceneIndexToUnload = -1, int sceneIndexToLoad = -1)
    {
        loadScreen.FadeIn();

        // don't want to unload main game scene
        // so check against that

        if (sceneIndexToUnload > 0)
            yield return SceneAsyncUtility.UnloadScene(sceneIndexToUnload);

        if (sceneIndexToLoad == 0)
        {
            //don't load scene but fire event
            //for object activation management
            PreSceneChange(0);
        }
        else if (sceneIndexToLoad > 0)
        {
            PreSceneChange(sceneIndexToLoad);
            yield return SceneAsyncUtility.LoadScene(sceneIndexToLoad);

            SetBackgroundTexture();
        }

        loadScreen.FadeOut();
    }

    IEnumerator LoadLevelRoutine(int levelIndex)
    {
        yield return StartCoroutine(ChangeSceneRoutine(1, 2));

        InitLevel(levelIndex);
    }

    IEnumerator LoadLocalTextures()
    {
        AsyncOperationHandle<IList<IResourceLocation>> locationHandle = Addressables.LoadResourceLocationsAsync("background");
        yield return locationHandle;

        if (locationHandle.Result.Count > 0)
            localBackgrounds = new Texture2D[locationHandle.Result.Count];

        int i = 0;

        foreach (var result in locationHandle.Result)
        {
            var textureHandle = Addressables.LoadAssetAsync<Texture2D>(result);
            yield return textureHandle;

            if (textureHandle.Status == AsyncOperationStatus.Succeeded)
                localBackgrounds[i++] = textureHandle.Result;
        }
    }

    void LoadTexture(UnityWebRequest request)
    {
        if (downloadedBackgrounds == null)
            downloadedBackgrounds = new List<Texture2D>(3);

        // arbitrary 3 image limit for the downloadable images
        var index = bgImageIndex;
        bgImageIndex = (bgImageIndex++) % 3;

        downloadedBackgrounds.Add(new Texture2D(1, 1));
        downloadedBackgrounds[index].LoadImage(request.downloadHandler.data);
    }

    public void SetBackgroundTexture()
    {
        Texture2D selected;

        if (downloadedBackgrounds != null && downloadedBackgrounds.Count > 0)
            selected = downloadedBackgrounds[Random.Range(0, downloadedBackgrounds.Count)];
        else
            selected = localBackgrounds[Random.Range(0, localBackgrounds.Length)];

        var renderer = gameBackground.GetComponent<Renderer>();
        var mat = renderer.material;
        mat.SetTexture("_MainTex", selected);
    }
}
