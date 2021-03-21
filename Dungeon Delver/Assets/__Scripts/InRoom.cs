﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InRoom : MonoBehaviour
{
    static public float ROOM_W = 16; // Ширина комнаты
    static public float ROOM_H = 11; // Высота комнаты
    static public float WALL_T = 2;  // Толщина стен

    [Header("Set in Inspector")]
    public bool keepInRoom = true;
    public float gridMult = 1;

    private void LateUpdate()
    {
        if(keepInRoom)
        {
            Vector2 rPos = roomPos;
            // Mathf.Clamp - гарантирует, что координата будет иметь значение между минимальным значением WALL_T и максимальным значеним ROOM_W - 1 - WALL_T
            rPos.x = Mathf.Clamp(rPos.x, WALL_T, ROOM_W - 1 - WALL_T); 
            rPos.y = Mathf.Clamp(rPos.y, WALL_T, ROOM_H - 1 - WALL_T);
            roomPos = rPos;
        }
    }

    /// <summary>
    /// Узнать местоположение этого персонажа в локальных координатах комнаты.
    /// </summary>
    public Vector2 roomPos
    {
        get
        {
            Vector2 tPos = transform.position;
            tPos.x %= ROOM_W;
            tPos.y %= ROOM_H;
            return tPos;
        }
        set
        {
            Vector2 rm = roomNum;
            rm.x *= ROOM_W;
            rm.y *= ROOM_H;
            rm += value;
            transform.position = rm;
        }
    }

    /// <summary>
    /// Узнать в какой комнате находится этот персонаж.
    /// </summary>
    public Vector2 roomNum
    {
        get
        {
            Vector2 tPos = transform.position;
            tPos.x = Mathf.Floor(tPos.x / ROOM_W);
            tPos.y = Mathf.Floor(tPos.y / ROOM_H);
            return tPos;
        }
        set
        {
            Vector2 rPos = roomPos;
            Vector2 rm = value;
            rm.x *= ROOM_W;
            rm.y *= ROOM_H;
            transform.position = rm + rPos;
        }
    }
}
