﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slime : Enemy, IFacingMover
{
    [Header("Set in Inspector: Slime")]
    public int speed = 1; // Скорость пермещение
    public float timeThinkMin = 2f; // Минимальное время следущей смены направления
    public float timeThinkMax = 4f; // Максимальное время следущей смены направления

    [Header("Set Dynamically: Slime")]
    public int facing = 0;
    public float timeNextDecision = 0;

    private InRoom _inRm;

    protected override void Awake()
    {
        base.Awake();
        _inRm = GetComponent<InRoom>();
    }

    override protected void Update()
    {
        base.Update();
        if (knockback) return; // Если скелет неуязвим 
        if (stun) // Если скелет под эфектом шока
        {
            speed = 0;
            rigid.velocity = directions[facing] * speed;
            return;
        }
        else
        {
            speed = 1;
        }


        if (Time.time >= timeNextDecision)
        { // Если время смены направление прошло
            DecideDirection(); // Решить куда двигаться дальше
        }

        rigid.velocity = directions[facing] * speed;  // Поле rigid унаследовано от класса Enemy и инициализируется в Enemy.Awake()
    }

    /// <summary>
    /// Выбирается случайное направление, и случайное время следующей смены направления
    /// </summary>
    void DecideDirection()
    {
        facing = Random.Range(0, 4); // Случайное направление
        timeNextDecision = Time.time + Random.Range(timeThinkMin, timeThinkMax); // Случайное время следующей смены направления
    }

    // Реализация интерфейс IFacingMover
    public int GetFacing()
    {
        return facing;
    }

    public bool Moving
    {
        get
        {
            return (true);
        }
    }

    public float GetSpeed()
    {
        return speed;
    }

    public float GridMult
    {
        get { return _inRm.gridMult; }
    }

    public Vector2 RoomPos
    {
        get { return _inRm.RoomPos; }
        set { _inRm.RoomPos = value; }
    }

    public Vector2 RoomNum
    {
        get { return _inRm.RoomNum; }
        set { _inRm.RoomNum = value; }
    }

    public Vector2 GetRoomPosOnGrid(float mult = -1)
    {
        return _inRm.GetRoomPosOnGrid(mult);
    }
}
