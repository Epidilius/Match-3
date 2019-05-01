using System.Collections;
using UnityEngine;

public class GamePiece : MonoBehaviour
{
    public enum GamePieceSubType
    {
        Normal = 0,
        HorizontalBomb,
        VerticalBomb,
        ColourBomb
    }

    public int Column;
    public int Row;
    private int PreviousColumn;
    private int PreviousRow;
    public bool IsMatched;

    public GameObject PieceSwappingWith { get; private set; }

    private Board GameBoard;
    private Vector2 CursorDownPosition;
    private Vector2 CursorUpPosition;
    private Vector2 TargetPosition; //Used to save on object creation/deletion

    [SerializeField] private float CursorMovementAngle  = 0;
    [SerializeField] private float CursorMovementResist = 1f;

    private GamePieceSubType GamePieceType;

    [SerializeField] private GameObject RowArrow;
    [SerializeField] private GameObject ColumnArrow;
    [SerializeField] private GameObject ColourBomb;

    // Use this for initialization
    void Start()
    {
        GamePieceType = GamePieceSubType.Normal;
        IsMatched    = false;

        GameBoard   = FindObjectOfType<Board>();
    }

    // Update is called once per frame
    void Update()
    {
        MovePiece(Column, Row);
    }
    
    private void MovePiece(int xTarget, int yTarget)
    {
        MoveHorizontal(xTarget);
        MoveVertical(yTarget);        
    }
    private void MoveHorizontal(int target)
    {
        TargetPosition = new Vector2(target, transform.position.y);

        if (Mathf.Abs(target - transform.position.x) < .1) transform.position = TargetPosition;
        else LerpPiece(0.6f);
    }
    private void MoveVertical(int target)
    {
        TargetPosition = new Vector2(transform.position.x, target);

        if (Mathf.Abs(target - transform.position.y) < .1) transform.position = TargetPosition;
        else LerpPiece(0.6f);
    }
    private void LerpPiece(float lerpSpeed)
    {
        transform.position = Vector2.Lerp(transform.position, TargetPosition, lerpSpeed);

        if (GameBoard.GamePieceGrid[Column, Row] != gameObject)
        {
            GameBoard.GamePieceGrid[Column, Row] = gameObject;
        }

        GameBoard.ChangeGameState(Board.GameState.Swapped);
    }

    public IEnumerator CheckMoveCoroutine()
    {
        yield return new WaitForSeconds(.5f);
        SwapPlaces();
        yield return new WaitForSeconds(.5f);
    }

    private void SwapPlaces()
    {
        if (PieceSwappingWith == null) return;
       
        if (!IsMatched && !PieceSwappingWith.GetComponent<GamePiece>().IsMatched)
        {
            PieceSwappingWith.GetComponent<GamePiece>().Row = Row;
            PieceSwappingWith.GetComponent<GamePiece>().Column = Column;

            Row = PreviousRow;
            Column = PreviousColumn;

            GameBoard.CurrentPiece = null;
            GameBoard.ChangeGameState(Board.GameState.Idle);
        }
        else
        {
            GameBoard.DestroyMatches();
        }
    }

    private void OnMouseDown()
    {
        if (GameBoard.IsGameIdle())
        {
            CursorDownPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }
    }
    private void OnMouseUp()
    {
        if (GameBoard.IsGameIdle())
        {
            CursorUpPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            CalculateAngle();
        }
    }

    private bool MinimumMovementReached()
    {
        return Mathf.Abs(CursorUpPosition.y - CursorDownPosition.y) > CursorMovementResist || Mathf.Abs(CursorUpPosition.x - CursorDownPosition.x) > CursorMovementResist ;
    }
    void CalculateAngle()
    {
        if(MinimumMovementReached() == false)
        {
            GameBoard.ChangeGameState(Board.GameState.Idle);
            return;
        }

        CursorMovementAngle = Mathf.Atan2(CursorUpPosition.y - CursorDownPosition.y, CursorUpPosition.x - CursorDownPosition.x) * 180 / Mathf.PI;

        MovePieces();

        GameBoard.ChangeGameState(Board.GameState.Swapping);
        GameBoard.CurrentPiece = this;
    }
    public float GetAngle()
    {
        return CursorMovementAngle;
    }

    void MovePieces()
    {
        if (CursorMovementAngle > -45 && CursorMovementAngle <= 45 && Column < GameBoard.ColumnCount - 1)
        {
            //Right
            PieceSwappingWith = GameBoard.GamePieceGrid[Column + 1, Row];
            PieceSwappingWith.GetComponent<GamePiece>().Column--;

            SetPreviousPosition();
            Column++;
        }
        else if (CursorMovementAngle > 45 && CursorMovementAngle <= 135 && Row < GameBoard.RowCount - 1)
        {
            //Up
            PieceSwappingWith = GameBoard.GamePieceGrid[Column, Row + 1];
            PieceSwappingWith.GetComponent<GamePiece>().Row--;

            SetPreviousPosition();
            Row++;
        }
        else if ((CursorMovementAngle > 135 || CursorMovementAngle <= -135) && Column > 0)
        {
            //Left
            PieceSwappingWith = GameBoard.GamePieceGrid[Column - 1, Row];
            PieceSwappingWith.GetComponent<GamePiece>().Column++;

            SetPreviousPosition();
            Column--;
        }
        else if (CursorMovementAngle < -45 && CursorMovementAngle >= -135 && Row > 0)
        {
            //Down
            PieceSwappingWith = GameBoard.GamePieceGrid[Column, Row - 1];
            PieceSwappingWith.GetComponent<GamePiece>().Row++;

            SetPreviousPosition();
            Row--;
        }

        StartCoroutine(CheckMoveCoroutine());
    }
    private void SetPreviousPosition()
    {
        PreviousRow    = Row;
        PreviousColumn = Column;
    }

    public void MakeColorBomb()
    {
        GamePieceType = GamePieceSubType.ColourBomb;
        GameObject colour = Instantiate(ColourBomb, transform.position, Quaternion.identity);
        colour.transform.parent = transform;
    }
    public void MakeVerticalBomb()
    {
        GamePieceType = GamePieceSubType.VerticalBomb;
        GameObject arrow = Instantiate(ColumnArrow, transform.position, Quaternion.identity);
        arrow.transform.parent = transform;
    }
    public void MakeHorizontalBomb()
    {
        GamePieceType = GamePieceSubType.HorizontalBomb;
        GameObject arrow = Instantiate(RowArrow, transform.position, Quaternion.identity);
        arrow.transform.parent = transform;
    }

    public bool IsColourBomb()
    {
        return GamePieceType == GamePieceSubType.ColourBomb;
    }
    public bool IsVerticalBomb()
    {
        return GamePieceType == GamePieceSubType.VerticalBomb;
    }
    public bool IsHorizontalBomb()
    {
        return GamePieceType == GamePieceSubType.HorizontalBomb;
    }
}
