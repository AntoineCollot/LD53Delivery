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
        if (Mathf.Abs(vector.x) > Mathf.Abs(vector.y)+0.05f)
        {
            if (vector.x < 0)
                dir = Direction.Left;
            else
                dir = Direction.Right;
        }
        else
        {
            //Make down the default
            if (vector.y <= 0.05f)
                dir = Direction.Down;
            else
                dir = Direction.Up;
        }
        return dir;
    }

    public static Vector2 ToVector2(this HorizontalDirection dir)
    {
        if (dir == HorizontalDirection.Forward)
            return Vector2.right;
        else
            return Vector2.left;
    }
}