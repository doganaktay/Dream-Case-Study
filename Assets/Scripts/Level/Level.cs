using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using TMPro;

[System.Serializable]
public class Level : Interactable
{
    [SerializeField] LayoutDesigner topBar, bottomBar;
    [SerializeField] GameObject levelQuad;
    [SerializeField] GameObject tileHolder;
    [SerializeField] Tile tilePrefab;
    [SerializeField] LevelCelebration celebration;

    PerObjectMaterialProperties prop;
    TextMeshPro currentScoreText, highScoreText, moveCountText;

    public Action OnLevelComplete;

    public LevelData data;
    public Tile[,] board;
    bool[] rowCompleted;

    float cellDimension = -1;
    float cellScale;
    Vector2 minCorner;
    Vector2Int startCell;
    bool switched = false;
    float moveMagnitudeThreshold = 1f;

    int currentScore;
    public int CurrentScore => currentScore;
    int highScore;
    bool newHighScore = false;

    private void OnEnable() => GameManager.Instance.CurrentLevel = this;    

    public void Init(LevelData data, int highScore)
    {
        this.data = data;

        this.highScore = highScore;

        currentScoreText = topBar.RetrieveComponentFromPairByIndex(0) as TextMeshPro;
        highScoreText = topBar.RetrieveComponentFromPairByIndex(1) as TextMeshPro;
        moveCountText = topBar.RetrieveComponentFromPairByIndex(2) as TextMeshPro;

        currentScoreText.Update(0.ToString());
        highScoreText.Update(highScore.ToString());
        moveCountText.Update(data.moveCount.ToString());

        SetLevelDimensions(data);
        SetBoard(data);
    }

    protected override void FingerBeginDrag(PointerEventData eventData)
    {
        switched = false;
        Vector2 touchPos = GetFingerPosRelative(eventData.position);
        startCell = GetGridPos(touchPos);
    }

    protected override void FingerDrag(PointerEventData eventData)
    {
        if (board[startCell.y, startCell.x].IsMatched)
            return;

        var delta = eventData.delta;
        if(!switched && delta.sqrMagnitude > moveMagnitudeThreshold * moveMagnitudeThreshold)
        {
            bool xMax = Mathf.Abs(delta.y) < Mathf.Abs(delta.x);
            var direction = xMax ? new Vector2Int((int)Mathf.Sign(delta.x) * 1, 0) : new Vector2Int(0, (int)Mathf.Sign(delta.y) * 1);
            var nextCell = startCell + direction;

            if(nextCell.x >= 0 && nextCell.x < data.gridColumns && nextCell.y >= 0 && nextCell.y < data.gridRows
                && !board[nextCell.y, nextCell.x].IsMatched)
                SwitchCells(startCell, nextCell);

            switched = true;
        }
    }

    protected void SwitchCells(Vector2Int a, Vector2Int b)
    {
        var aPos = board[a.y, a.x].transform.position;
        var bPos = board[b.y, b.x].transform.position;

        board[a.y, a.x].MoveTween(bPos, 0.5f);
        board[b.y, b.x].MoveTween(aPos, 0.5f);

        //board[a.y, a.x].transform.position = bPos;
        //board[b.y, b.x].transform.position = aPos;

        var temp = board[a.y, a.x];
        board[a.y, a.x] = board[b.y, b.x];
        board[b.y, b.x] = temp;

        if (CheckRowComplete(a.y))
            ClearRow(a.y);
        if (CheckRowComplete(b.y))
            ClearRow(b.y);

        data.moveCount--;
        moveCountText.Update(data.moveCount.ToString());

        if (data.moveCount <= 0 || IsLevelComplete())
            CompleteLevel();
    }

    void CompleteLevel()
    {
        OnLevelComplete?.Invoke();

        //DOTween.KillAll();

        if (newHighScore)
            celebration.PlayCelebration(highScore, LoadLevelSelection);
        else
            LoadLevelSelection();
    }

    void LoadLevelSelection()
    {
        GameManager.Instance.ChangeScene(gameObject.scene.buildIndex, 1);
    }

    bool CheckRowComplete(int rowIndex)
    {
        var typeToCheck = board[rowIndex, 0].data.type;
        for (int i = 1; i < data.gridColumns; i++)
            if (board[rowIndex, i].data.type != typeToCheck)
                return false;

        return true;
    }

    void ClearRow(int rowIndex)
    {
        for (int i = 0; i < data.gridColumns; i++)
        {
            var current = board[rowIndex, i];
            current.MatchTween(1, i * 0.05f);
            current.IsMatched = true;
        }

        currentScore += board[rowIndex, 0].data.score * data.gridColumns;
        currentScoreText.Update(currentScore.ToString());

        if(currentScore > highScore)
        {
            highScore = currentScore;
            highScoreText.Update(highScore.ToString());
            newHighScore = true;
        }

        rowCompleted[rowIndex] = true;
    }

    bool IsLevelComplete()
    {
        for(int i = 0; i < data.gridRows - 1; i++)
        {
            if (rowCompleted[i])
                continue;

            int r = 0, g = 0, b = 0, y = 0;

            while (i < data.gridRows && !rowCompleted[i])
            {
                AccumulateRow(i, ref r, ref g, ref b, ref y);

                if (r >= data.gridColumns || g >= data.gridColumns || b >= data.gridColumns || y >= data.gridColumns)
                    return false;

                // if the next row can't be considered in the while loop
                // then we don't need to consider it again in the for loop anyway
                i++;
            }
        }

        return true;
    }

    void AccumulateRow(int index, ref int r, ref int g, ref int b, ref int y)
    {
        for(int i = 0; i < data.gridColumns; i++)
        {
            switch(board[index, i].data.type)
            {
                case TileType.Red:
                    r++;
                    break;
                case TileType.Green:
                    g++;
                    break;
                case TileType.Blue:
                    b++;
                    break;
                case TileType.Yellow:
                    y++;
                    break;
            }
        }
    }

    Vector2 GetFingerPosRelative(Vector2 screenPos) => new Vector2(screenPos.x - minCorner.x, screenPos.y - minCorner.y);
    Vector2Int GetGridPos(Vector2 relativePos) => new Vector2Int(Mathf.FloorToInt(relativePos.x / cellDimension), Mathf.FloorToInt(relativePos.y / cellDimension));

    void SetBoard(LevelData data)
    {
        //data.PrintLevelInfo();
        tileHolder.ClearChildren();

        board = new Tile[data.gridRows, data.gridColumns];
        for (int i = 0; i < data.gridRows; i++)
            for (int j = 0; j < data.gridColumns; j++)
            {
                var key = data.state[i * data.gridColumns + j];
                board[i,j] = CreateTile(new Vector2Int(j, i), LevelUtility.TileSet[key]);
            }

        rowCompleted = new bool[data.gridRows];
    }

    void SetLevelDimensions(LevelData data)
    {
        // only do once on startup
        if (cellDimension == -1)
        {
            cellDimension = GameManager.ScreenSize.x / LevelUtility.MaxGridSize;
            cellScale = GameManager.ScreenWorldSize.x / LevelUtility.MaxGridSize;
        }

        // determine min corner for grid calculations
        var width = cellDimension * data.gridColumns;
        var marginLeft = (GameManager.ScreenSize.x - width) / 2f;
        var height = cellDimension * data.gridRows;
        var marginBottom = (GameManager.ScreenSize.y - height) / 2f;
        minCorner = new Vector2(marginLeft, marginBottom);

        // determine quad size for background display
        var xSize = cellScale * data.gridColumns;
        var ySize = cellScale * data.gridRows;
        var adjustedSize = new Vector2(xSize, ySize);
        levelQuad.transform.localScale = new Vector3(adjustedSize.x, adjustedSize.y, 1f);

        // set grid size on background shader for per cell uvs
        prop = levelQuad.GetComponent<PerObjectMaterialProperties>();
        prop.SetGridSize(data.gridSize);
    }

    Tile CreateTile(Vector2Int index, TileData tileData)
    {
        var tile = Instantiate(tilePrefab);

        var half = cellDimension / 2f;
        var x = minCorner.x + index.x * cellDimension + half;
        var y = minCorner.y + index.y * cellDimension + half;
        var worldPoint = GameManager.MainCam.ScreenToWorldPoint(new Vector2(x, y));
        tile.transform.position = new Vector3(worldPoint.x, worldPoint.y, -1);
        tile.transform.SetParent(tileHolder.transform);

        tile.data = tileData;
        tile.GetComponent<SpriteRenderer>().color = tileData.color;
        tile.gridPosition = index;
        tile.gameObject.name = $"{tileData.type}";

        return tile;
    }
}
