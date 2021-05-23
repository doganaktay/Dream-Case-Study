using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Archi.IO
{
    public class SaveData
    {
        public Dictionary<int,int> highScores; // can't serialize this with JSON utility
        public int topAvailableLevelIndex;
        public LevelData currentLevel;

        public SaveData()
        {
            highScores = new Dictionary<int, int>();
            topAvailableLevelIndex = 1;
            currentLevel = new LevelData();
        }

        public void SubmitScore(int index, int score)
        {
            if (!highScores.ContainsKey(index))
                highScores.Add(index, score);
            else
                highScores[index] = Mathf.Max(highScores[index], score);
        }

        public int RequestHighScore(int index)
        {
            if (highScores.ContainsKey(index))
                return highScores[index];
            else
                return 0;
        }
    }

}

