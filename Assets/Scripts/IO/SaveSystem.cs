using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace Archi.IO
{
    public static class SaveSystem
    {
        #region With SaveData

        // All saving is handled with JSON

        private static readonly string savePath = Application.persistentDataPath + "/Save/";

        /// <summary>
        /// Convert SaveData to JSON and write to file
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="saveData"></param>
        /// <returns></returns>
        public static void Save(string filePath, SaveData saveData)
        {
            string path = ConstructFullPath(filePath);
            string json = JsonConvert.SerializeObject(saveData, Formatting.None);

            File.WriteAllText(path, json);
        }

        /// <summary>
        /// Retrieve JSON and return SaveData
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static SaveData Load(string filePath)
        {
            string path = ConstructFullPath(filePath);

            if (File.Exists(path))
            {
                string saveData = File.ReadAllText(path);
                return JsonConvert.DeserializeObject<SaveData>(saveData);
            }
            else
                return null;
        }

        #endregion

        #region Utility

        private static string ConstructFullPath(string filePath)
        {
            if (!Directory.Exists(savePath))
                Directory.CreateDirectory(savePath);

            return Path.Combine(savePath, filePath);
        }

        #endregion
    }
}

