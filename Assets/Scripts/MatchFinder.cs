using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MatchFinder : MonoBehaviour
{
    private Board GameBoard;
    public List<GameObject> CurrentMatches;

    // Use this for initialization
    void Start()
    {
        GameBoard = FindObjectOfType<Board>();
        CurrentMatches = new List<GameObject>();
    }

    public void FindAllMatches()
    {
        StartCoroutine(FindAllMatchesCoroutine());
    }

    private void HandleColourBomb(GamePiece pieceOne, GamePiece pieceTwo, GamePiece pieceThree)
    {
        List<GameObject> allMatches = new List<GameObject>();

        if (pieceOne.IsColourBomb())   allMatches.AddRange(GetAllPiecesInColumn(pieceOne.Column));
        if (pieceTwo.IsColourBomb())   allMatches.AddRange(GetAllPiecesInColumn(pieceTwo.Column));
        if (pieceThree.IsColourBomb()) allMatches.AddRange(GetAllPiecesInColumn(pieceThree.Column));

        AddToMatchList(allMatches);
    }
    private void HandleVerticalBomb(GamePiece pieceOne, GamePiece pieceTwo, GamePiece pieceThree)
    {
        List<GameObject> allMatches = new List<GameObject>();

        if (pieceOne.IsVerticalBomb())   allMatches.AddRange(GetAllPiecesInColumn(pieceOne.Column));
        if (pieceTwo.IsVerticalBomb())   allMatches.AddRange(GetAllPiecesInColumn(pieceTwo.Column));
        if (pieceThree.IsVerticalBomb()) allMatches.AddRange(GetAllPiecesInColumn(pieceThree.Column));

        AddToMatchList(allMatches);
    }
    private void HandleHorizontalBomb(GamePiece pieceOne, GamePiece pieceTwo, GamePiece pieceThree)
    {
        List<GameObject> allMatches = new List<GameObject>();

        if (pieceOne.IsHorizontalBomb())   allMatches.AddRange(GetAllPiecesInRow(pieceOne.Row));
        if (pieceTwo.IsHorizontalBomb())   allMatches.AddRange(GetAllPiecesInRow(pieceTwo.Row));
        if (pieceThree.IsHorizontalBomb()) allMatches.AddRange(GetAllPiecesInRow(pieceThree.Row));

        AddToMatchList(allMatches);
    }

    private void AddToMatchList(GameObject piece)
    {
        if (!CurrentMatches.Contains(piece))
        {
            CurrentMatches.Add(piece);
        }
        piece.GetComponent<GamePiece>().IsMatched = true;
    }
    private void AddToMatchList(List<GameObject> pieces)
    {
        CurrentMatches.Union(pieces);
    }

    private IEnumerator FindAllMatchesCoroutine()
    {
        yield return new WaitForSeconds(.2f);

        for (int i = 0; i < GameBoard.ColumnCount; i++)
        {
            for (int j = 0; j < GameBoard.RowCount; j++)
            {
                GameObject currentPiece = GameBoard.GamePieceGrid[i, j];                   
                if (currentPiece == null) continue; //This would look so much nicer with the others

                if (i > 0 && i < GameBoard.ColumnCount - 1)
                {
                    GameObject pieceOne = GameBoard.GamePieceGrid[i - 1, j];
                    GameObject pieceTwo = GameBoard.GamePieceGrid[i + 1, j];

                    AddPiecesToMatchList(currentPiece, pieceOne, pieceTwo);                    
                }
                if (j > 0 && j < GameBoard.RowCount - 1)
                {
                    GameObject pieceOne = GameBoard.GamePieceGrid[i, j + 1];
                    GameObject pieceTwo = GameBoard.GamePieceGrid[i, j - 1];

                    AddPiecesToMatchList(currentPiece, pieceOne, pieceTwo);                    
                }
            }
        }
    }
    
    private void AddPiecesToMatchList(GameObject currentPiece, GameObject pieceOne, GameObject pieceTwo)
    {
        if (pieceOne == null || pieceTwo == null) return;
        if (pieceOne.tag != currentPiece.tag || pieceTwo.tag != currentPiece.tag) return;
        
        AddToMatchList(pieceOne);
        AddToMatchList(pieceTwo);
        AddToMatchList(currentPiece);

        HandleColourBomb(currentPiece.GetComponent<GamePiece>(), pieceOne.GetComponent<GamePiece>(), pieceTwo.GetComponent<GamePiece>());
        HandleVerticalBomb(currentPiece.GetComponent<GamePiece>(), pieceOne.GetComponent<GamePiece>(), pieceTwo.GetComponent<GamePiece>());
        HandleHorizontalBomb(currentPiece.GetComponent<GamePiece>(), pieceOne.GetComponent<GamePiece>(), pieceTwo.GetComponent<GamePiece>());
    }

    public void MatchAllPiecesOfColour(string colourTag)
    {
        for (int i = 0; i < GameBoard.ColumnCount; i++)
        {
            for (int j = 0; j < GameBoard.RowCount; j++)
            {
                if (GameBoard.GamePieceGrid[i, j] != null)
                {
                    if (GameBoard.GamePieceGrid[i, j].CompareTag(colourTag))
                    {
                        GameBoard.GamePieceGrid[i, j].GetComponent<GamePiece>().IsMatched = true;
                    }
                }
            }
        }
    }

    List<GameObject> GetAllPiecesInRow(int row)
    {
        List<GameObject> pieces = new List<GameObject>();
        for (int i = 0; i < GameBoard.ColumnCount; i++)
        {
            if (GameBoard.GamePieceGrid[i, row] != null)
            {
                pieces.Add(GameBoard.GamePieceGrid[i, row]);
                GameBoard.GamePieceGrid[i, row].GetComponent<GamePiece>().IsMatched = true;
            }
        }
        return pieces;
    }
    List<GameObject> GetAllPiecesInColumn(int column)
    {
        List<GameObject> pieces = new List<GameObject>();
        for (int i = 0; i < GameBoard.RowCount; i++)
        {
            if (GameBoard.GamePieceGrid[column, i] != null)
            {
                pieces.Add(GameBoard.GamePieceGrid[column, i]);
                GameBoard.GamePieceGrid[column, i].GetComponent<GamePiece>().IsMatched = true;
            }
        }
        return pieces;
    }

    public void CreateBombs()
    {
        if (GameBoard.CurrentPiece != null)
        {
            if (GameBoard.CurrentPiece.IsMatched)
            {
                MakePieceBomb(GameBoard.CurrentPiece);
            }
            else if (GameBoard.CurrentPiece.PieceSwappingWith != null)
            {
                MakePieceBomb(GameBoard.CurrentPiece.PieceSwappingWith.GetComponent<GamePiece>());
            }
        }
    }

    private void MakePieceBomb(GamePiece piece)
    {
        if (!piece.IsMatched) return;

        piece.IsMatched = false;

        if (IsSwipeHorizontal())
        {
            piece.MakeHorizontalBomb();
        }
        else
        {
            piece.MakeVerticalBomb();
        }
    }
    private bool IsSwipeHorizontal()
    {
        return (GameBoard.CurrentPiece.GetAngle()> -45 && GameBoard.CurrentPiece.GetAngle() <= 45) || (GameBoard.CurrentPiece.GetAngle() < -135 || GameBoard.CurrentPiece.GetAngle() >= 135);
    }
}
