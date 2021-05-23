using System.IO;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.AddressableAssets;
using Debug = UnityEngine.Debug;

public static class LevelUtility
{
    public static readonly int requiredLevelCount = 25;

    public static readonly string LocalLevelsPath = Application.dataPath + "/Levels/";

#if UNITY_EDITOR
    public static readonly string DownloadedLevelsPath = Application.dataPath + "/Levels/";
#else
    public static readonly string DownloadedLevelsPath = Application.persistentDataPath + "/Levels/";
#endif

    static Dictionary<int, LevelData> availableLevels;
    public static Dictionary<int, LevelData> AvailableLevels { get { return availableLevels; } private set { } }
    public static int AvailableLevelCount { get { if (availableLevels != null) return availableLevels.Count; else return 0;} }

    static readonly Dictionary<string, int> levelMap = new Dictionary<string, int>
    {
        {"RM_A", 15},
        {"RM_B", 10}
    };

    public static readonly Dictionary<char, TileData> TileSet = new Dictionary<char, TileData>
    {
        { 'r', new TileData(TileType.Red, Color.red, 100) },
        { 'g', new TileData(TileType.Green, Color.green, 150) },
        { 'b', new TileData(TileType.Blue, Color.blue, 200) },
        { 'y', new TileData(TileType.Yellow, Color.yellow, 250) },
    };

    public static readonly int MaxGridSize = 9;

    static Mesh primitiveQuad;
    public static Mesh PrimitiveQuad
    {
        get
        {
            if (primitiveQuad != null)
                return primitiveQuad;

            var go = GameObject.CreatePrimitive(PrimitiveType.Quad);
            go.SetActive(false);
            primitiveQuad = go.GetComponent<MeshFilter>().mesh;

            return primitiveQuad;
        }
    }

    public static List<KeyValuePair<int, LevelData>> GetAvailableLevelsSorted()
    {
        var levels = from pair in AvailableLevels
                     orderby pair.Key ascending
                     select pair;

        return levels.ToList();
    }

    public static bool AllLevelsDownloaded()
    {
        var dir = new DirectoryInfo(DownloadedLevelsPath);
        var fileCount = dir.EnumerateFiles().Where(file => file.Extension != ".meta").Count();

#if UNITY_EDITOR
        return fileCount == requiredLevelCount;
#else
        // 10 is the level count shipped with app
        return fileCount == requiredLevelCount - 10;
#endif
    }

    public static void LoadDownloadedLevels()
    {
        if(availableLevels == null)
            availableLevels = new Dictionary<int, LevelData>();

        // load downloaded

        var dir = new DirectoryInfo(DownloadedLevelsPath);

        foreach (var file in dir.EnumerateFiles().Where(file => file.Extension != ".meta"))
        {
            LevelData level = ParseLevel(file.Name);

            if (!availableLevels.ContainsKey(level.levelIndex))
                availableLevels.Add(level.levelIndex, level);
            else
                Debug.LogWarning("Duplicate index on load");
        }
    }

    public static IEnumerator LoadLocalLevelsAsync()
    {
        if (availableLevels == null)
            availableLevels = new Dictionary<int, LevelData>();

        AsyncOperationHandle<IList<IResourceLocation>> locationHandle = Addressables.LoadResourceLocationsAsync("level");
        yield return locationHandle;

        foreach(var result in locationHandle.Result)
        {
            var textHandle = Addressables.LoadAssetAsync<TextAsset>(result);
            yield return textHandle;

            if(textHandle.Status == AsyncOperationStatus.Succeeded)
            {
                LevelData data = ParseLevel(textHandle.Result);
                Debug.Log($"Loading local level {data.levelIndex}");

                if (!availableLevels.ContainsKey(data.levelIndex))
                    availableLevels.Add(data.levelIndex, data);
                else
                    Debug.LogWarning("Duplicate index on load");
            }
        }

        yield return null;
    }

    public static string[] GetFileNames(params LevelRequest[] levelRequest)
    {
        int total = 0;
        foreach(var data in levelRequest)
        {
            total += levelMap[data.prefix] + 1 - data.startIndex;
        }

        string[] fileNames = new string[total];
        int count = 0;

        for(int i = 0; i < levelRequest.Length; i++)
        {
            if (!levelMap.ContainsKey(levelRequest[i].prefix))
            {
                throw new KeyNotFoundException();
            }
            else
            {
                for (int j = levelRequest[i].startIndex; j < levelMap[levelRequest[i].prefix] + 1; j++)
                {
                    fileNames[count++] = levelRequest[i].prefix + j;
                }
            }
        }

        return fileNames;
    }

    public static void SaveLevel(UnityWebRequest request, string fileName)
    {
        Debug.Log($"Saving: {fileName}");
        string path = DownloadedLevelsPath + fileName;
        Directory.CreateDirectory(DownloadedLevelsPath);
        File.WriteAllText(path, request.downloadHandler.text);
    }

    /// <summary>
    /// Parse level file for LevelData.
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public static LevelData ParseLevel(string fileName)
    {
        string[] lines = File.ReadAllLines(DownloadedLevelsPath + fileName);

        var levelNumber = int.Parse(Regex.Match(lines[0], @"\d+").Value);
        var width = int.Parse(Regex.Match(lines[1], @"\d+").Value);
        var height = int.Parse(Regex.Match(lines[2], @"\d+").Value);
        var moveCount = int.Parse(Regex.Match(lines[3], @"\d+").Value);
        lines[4] = lines[4].Substring(6).Replace(",","");
        var layout = lines[4].ToCharArray();

        var level = new LevelData(levelNumber, width, height, moveCount, layout);
        return level;
    }

    /// <summary>
    /// Parse text asset for LevelData.
    /// </summary>
    /// <param name="textAsset"></param>
    /// <returns></returns>
    public static LevelData ParseLevel(TextAsset textAsset)
    {
        string[] lines = Regex.Split(textAsset.text, "\n|\r|\r\n");

        var levelNumber = int.Parse(Regex.Match(lines[0], @"\d+").Value);
        var width = int.Parse(Regex.Match(lines[1], @"\d+").Value);
        var height = int.Parse(Regex.Match(lines[2], @"\d+").Value);
        var moveCount = int.Parse(Regex.Match(lines[3], @"\d+").Value);
        lines[4] = lines[4].Substring(6).Replace(",", "");
        var layout = lines[4].ToCharArray();

        var level = new LevelData(levelNumber, width, height, moveCount, layout);
        return level;
    }

}

public struct LevelRequest
{
    public string prefix;
    public int startIndex;

    public LevelRequest(string prefix)
    {
        this.prefix = prefix;
        startIndex = 1;
    }

    public LevelRequest(string prefix, int startIndex)
    {
        this.prefix = prefix;
        this.startIndex = startIndex;
    }
}
