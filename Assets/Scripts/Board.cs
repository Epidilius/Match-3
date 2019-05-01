using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    public enum GameState
    {
        Idle = 0,
        Swapping,
        Swapped
    }

    private GameState CurrentState;

    private int DropOffSet;
    public int ColumnCount = 7;
    public int RowCount = 10;

    [Tooltip("How many times the game should loop on creation, to try and stop any matches existing on startup")]
    [SerializeField] private int StartingMatchesIterationCount = 100;

    [SerializeField] private GameObject TilePrefab;
    [SerializeField] private GameObject DestroyParticle;
    [SerializeField] private GameObject[] GamePieces;

    public GamePiece CurrentPiece;
    public GameObject[,] GamePieceGrid;   //I want to remove this, but I can't remember how to have a one dimensional array of stacked items
    private List<GameObject> GamePiecePool;

    private MatchFinder MatchFinder;

    // Use this for initialization
    void Start()
    {
        ChangeGameState(GameState.Idle);
        MatchFinder = FindObjectOfType<MatchFinder>();

        GamePiecePool = new List<GameObject>();
        GamePieceGrid = new GameObject[ColumnCount, RowCount];

        SetUp();

        DropOffSet = 1;
    }

    private void Update()
    {
        if(CurrentState == GameState.Swapped)
        {
            MatchFinder.FindAllMatches();
            ChangeGameState(GameState.Idle);
        }
    }

    private void SetUp()
    {
        int matchCheckCount = 0;
        for (int i = 0; i < ColumnCount; i++)
        {
            for (int j = 0; j < RowCount; j++)
            {
                Vector2 tilePosition = new Vector2(i, j + DropOffSet);
                GameObject backgroundTile = Instantiate(TilePrefab, tilePosition, Quaternion.identity) as GameObject;

                backgroundTile.transform.parent = transform;
                backgroundTile.name = "( " + i + ", " + j + " )";
                
                int pieceIndex = Random.Range(0, GamePieces.Length);
                while (DoCoordsHaveMatches(i, j, GamePieces[pieceIndex]) && matchCheckCount < StartingMatchesIterationCount)
                {
                    pieceIndex = Random.Range(0, GamePieces.Length);
                    matchCheckCount++;
                }

                GamePieceGrid[i, j] = CreateNewPiece(pieceIndex, tilePosition, i, j);
            }
        }

        for(int i = 0; i < ColumnCount * RowCount; i++)
        {
            int pieceIndex = Random.Range(0, GamePieces.Length);
            GameObject piece = Instantiate(GamePieces[pieceIndex], Vector2.zero, Quaternion.identity);
            piece.SetActive(false);

            GamePiecePool.Add(piece);
        }
    }

    private GameObject CreateNewPiece(int pieceIndex, Vector2 position, int column, int row)
    {
        GameObject piece = null;
        if (GamePiecePool.Count > 0)
        {
            pieceIndex = Random.Range(0, GamePiecePool.Count);
            piece = GamePiecePool[pieceIndex];
            piece.transform.position = position;

            GamePiecePool.RemoveAt(pieceIndex);
        }
        else piece = Instantiate(GamePieces[pieceIndex], position, Quaternion.identity);
        
        piece.name = "( " + column + ", " + row + " )";
        piece.transform.parent = transform;
        piece.GetComponent<GamePiece>().Row    = row;
        piece.GetComponent<GamePiece>().Column = column;

        piece.SetActive(true);

        return piece;
    }

    private bool DoCoordsHaveMatches(int column, int row, GameObject piece)
    {
        if (column > 1 && row > 1)
        {
            if (GamePieceGrid[column - 1, row].tag == piece.tag && GamePieceGrid[column - 2, row].tag == piece.tag)
            {
                return true;
            }
            if (GamePieceGrid[column, row - 1].tag == piece.tag && GamePieceGrid[column, row - 2].tag == piece.tag)
            {
                return true;
            }
        }
        else if (column <= 1 || row <= 1)
        {
            if (row > 1)
            {
                if (GamePieceGrid[column, row - 1].tag == piece.tag && GamePieceGrid[column, row - 2].tag == piece.tag)
                {
                    return true;
                }
            }
            if (column > 1)
            {
                if (GamePieceGrid[column - 1, row].tag == piece.tag && GamePieceGrid[column - 2, row].tag == piece.tag)
                {
                    return true;
                }
            }
        }

        return false;
    }

    public void DestroyMatches()
    {
        for (int i = 0; i < ColumnCount; i++)
        {
            for (int j = 0; j < RowCount; j++)
            {
                if (GamePieceGrid[i, j] == null) continue;
                if (GamePieceGrid[i, j].GetComponent<GamePiece>().IsMatched)
                {
                    DestroyMatchAt(i, j);
                }
            }
        }

        MatchFinder.CurrentMatches.Clear();
        StartCoroutine(DecreaseRowCoroutine());
    }
    private void DestroyMatchAt(int column, int row)
    {
        GameObject piece = GamePieceGrid[column, row];

        if (MatchFinder.CurrentMatches.Count == 4 || MatchFinder.CurrentMatches.Count == 7)
        {
            MatchFinder.CreateBombs();
        }

        GameObject particle = Instantiate(DestroyParticle, piece.transform.position, Quaternion.identity);
        Destroy(particle, .5f);

        piece.GetComponent<GamePiece>().IsMatched = false;
        piece.SetActive(false);
        GamePiecePool.Add(piece);

        GamePieceGrid[column, row] = null;
    }
        
    private IEnumerator DecreaseRowCoroutine()
    {        
        for (int i = 0; i < ColumnCount; i++)
        {
            int emptySpacesInColumn = 0;

            for (int j = 0; j < RowCount; j++)
            {
                if (GamePieceGrid[i, j] == null) emptySpacesInColumn++;
                else if (emptySpacesInColumn > 0)
                {
                    GamePieceGrid[i, j].GetComponent<GamePiece>().Row -= emptySpacesInColumn;
                    GamePieceGrid[i, j] = null;
                }
            }
        }

        yield return new WaitForSeconds(.5f);
        StartCoroutine(FillBoardCoroutine());
    }

    private void RefillBoard()
    {
        for (int i = 0; i < ColumnCount; i++)
        {
            for (int j = 0; j < RowCount; j++)
            {
                if (GamePieceGrid[i, j] != null) continue;
                AddNewPieceToBoard(i, j);
            }
        }
    }
    private void AddNewPieceToBoard(int column, int row)
    {
        Vector2 dropPosition = new Vector2(column, row + DropOffSet);
        GameObject piece     = CreateNewPiece(Random.Range(0, GamePieces.Length), dropPosition, column, row);

        piece.GetComponent<GamePiece>().Row    = row;
        piece.GetComponent<GamePiece>().Column = column;

        GamePieceGrid[column, row] = piece;
    }

    private bool AreThereMatchesOnBoard()
    {
        for (int i = 0; i < ColumnCount; i++)
        {
            for (int j = 0; j < RowCount; j++)
            {
                if (GamePieceGrid[i, j] != null)
                {
                    if (GamePieceGrid[i, j].GetComponent<GamePiece>().IsMatched)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    private IEnumerator FillBoardCoroutine()
    {
        RefillBoard();
        yield return new WaitForSeconds(.5f);

        while (AreThereMatchesOnBoard())
        {
            yield return new WaitForSeconds(.5f);
            DestroyMatches();
        }
        
        CurrentPiece = null;

        yield return new WaitForSeconds(.5f);
        ChangeGameState(GameState.Idle);
    }

    public void ChangeGameState(GameState gameState)
    {
        CurrentState = gameState;
    }
    public bool IsGameIdle()
    {
        return CurrentState == GameState.Idle;
    }
}
