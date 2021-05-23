using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct LevelData
{
    public int levelIndex;
    public int gridColumns, gridRows;
    public Vector2Int gridSize => new Vector2Int(gridRows, gridColumns);
    public int moveCount;
    public char[] state;

    public LevelData(int levelIndex, int gridWidth, int gridHeight, int moveCount, char[] state)
    {
        this.levelIndex = levelIndex;
        gridColumns = gridWidth;
        gridRows = gridHeight;
        this.moveCount = moveCount;
        this.state = state;
    }

#if UNITY_EDITOR

    public void PrintLevelInfo()
    {
        Debug.Log($"Level index: {levelIndex} width: {gridColumns} height: {gridRows} move count: {moveCount}");
    }

#endif
}
