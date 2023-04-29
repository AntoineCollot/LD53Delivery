using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Windows;

public enum HorizontalDirection { Forward, Backward }
public class HorizontalDirEvent : UnityEvent<HorizontalDirection> { }

public enum Direction { Up, Right, Left, Down}

public static class DirectionExtensions
{
    public static Direction ToDirection(this Vector2 vector)
    {
        Direction dir;
        if (Mathf.Abs(vector.x) >= Mathf.Abs(vector.y))
        {
            if (vector.x < 0)
                dir = Direction.Left;
            else
                dir = Direction.Right;
        }
        else
        {
            if (vector.y < 0)
                dir = Direction.Down;
            else
                dir = Direction.Up;
        }
        return dir;
    }
}