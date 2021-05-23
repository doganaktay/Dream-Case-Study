using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GameManager))]
public class GameManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        using(new GUILayout.HorizontalScope())
        {
            if(GUILayout.Button("Load Random"))
            {
                var manager = target as GameManager;
                var index = 0;
                if (LevelUtility.AvailableLevels != null)
                    index = Random.Range(1, LevelUtility.requiredLevelCount + 1);

                manager.InitLevel(index);
            }
        }

        base.OnInspectorGUI();
    }
}
