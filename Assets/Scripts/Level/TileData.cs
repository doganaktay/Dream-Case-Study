using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TileType
{
    Red,
    Green,
    Blue,
    Yellow
}

public struct TileData
{
    public TileType type;
    public Color color;
    public int score;

    public TileData(TileType type, Color color, int score)
    {
        this.type = type;
        this.color = color;
        this.score = score;
    }
}
