using UnityEngine;

public enum Direction
{
    North,
    East,
    South,
    West
}

public enum DirectionChange
{
    None,
    TurnRight,
    TurnLeft,
    TurnAround
}

public static class DirectionExtensions
{
    private static Quaternion[] rotations =
    {
            Quaternion.identity,
            Quaternion.Euler(0.0f, 90.0f, 0.0f),
            Quaternion.Euler(0.0f, 180.0f, 0.0f),
            Quaternion.Euler(0.0f, 270.0f, 0.0f)
    };

    private static Vector3[] halfVectors =
    {
            Vector3.forward * 0.5f,
            Vector3.right * 0.5f,
            Vector3.back * 0.5f,
            Vector3.left * 0.5f
    };

    public static Quaternion GetRotation(this Direction _direction) => rotations[(int) _direction];

    public static DirectionChange GetDirectionChangeTo(this Direction _current, Direction _next)
    {
        if (_current == _next)
        {
            return DirectionChange.None;
        }

        if (_current + 1 == _next
            || _current - 3 == _next)
        {
            return DirectionChange.TurnRight;
        }

        if (_current - 1 == _next
            || _current + 3 == _next)
        {
            return DirectionChange.TurnLeft;
        }

        return DirectionChange.TurnAround;
    }

    public static float GetAngle(this Direction _direction) => (float) _direction * 90.0f;

    public static Vector3 GetHalfVector(this Direction _direction) => halfVectors[(int) _direction];
}