using UnityEngine;

public class AngleChecker : MonoBehaviour
{
    public enum CursorDirection
    {
        Up = 0,
        Down,
        Left,
        Right,
        None
    }
    public Vector2 FirstCursorPosition;
    public Vector2 CurrentCursorPosition;
    public float Angle;
    public bool ShouldCalculateAngle;
    public CursorDirection SwipeDirection;

    // Use this for initialization
    void Start()
    {
        ShouldCalculateAngle = false;
        SwipeDirection = CursorDirection.None;
    }

    // Update is called once per frame
    void Update()
    {
        if (ShouldCalculateAngle)
        {
            Angle = GetAngle();
            SwipeDirection = GetDirection();

            ShouldCalculateAngle = false;
        }
    }

    private void OnMouseDown()
    {
        FirstCursorPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }
    private void OnMouseUp()
    {
        ShouldCalculateAngle = true;
    }

    private float GetAngle()
    {
        CurrentCursorPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        if ((CurrentCursorPosition.x - FirstCursorPosition.x) != 0f)
        {
            return Mathf.Atan2(CurrentCursorPosition.y - FirstCursorPosition.y, CurrentCursorPosition.x - FirstCursorPosition.x) * 180 / Mathf.PI;
        }
        else if (CurrentCursorPosition.y > FirstCursorPosition.y)
        {
            return 90f;
        }
        
        return -90f;
    }

    private CursorDirection GetDirection()
    {
        if (Angle >= 45f && Angle < 135f)   return CursorDirection.Up;
        if (Angle >= -135f && Angle < -45f) return CursorDirection.Down;
        if (Angle >= 135f || Angle < -135f) return CursorDirection.Left;
        if (Angle < 45f || Angle > -45f)    return CursorDirection.Right;

        return CursorDirection.None;
    }
}
