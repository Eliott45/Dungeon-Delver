﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bat : Enemy, IFacingMover
{
    [Header("Set in Inspector: Bat")]
    public int speed = 4; // Скорость пермещение
    public float timeThinkMin = 0.8f; // Минимальное время следущей смены направления
    public float timeThinkMax = 1.5f; // Максимальное время следущей смены направления

    [Header("Set Dynamically: Bat")]
    public int facing = 0;
    public float timeNextDecision = 0;

    private InRoom inRm;

    protected override void Awake()
    {
        base.Awake();
        inRm = GetComponent<InRoom>();
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
            speed = 4;
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
        anim.CrossFade("Bat_" + facing, 0);
        timeNextDecision = Time.time + Random.Range(timeThinkMin, timeThinkMax); // Случайное время следующей смены направления
    }

    // Реализация интерфейс IFacingMover
    public int GetFacing()
    {
        return facing;
    }

    public bool moving
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

    public float gridMult
    {
        get { return inRm.gridMult; }
    }

    public Vector2 roomPos
    {
        get { return inRm.roomPos; }
        set { inRm.roomPos = value; }
    }

    public Vector2 roomNum
    {
        get { return inRm.roomNum; }
        set { inRm.roomNum = value; }
    }

    public Vector2 GetRoomPosOnGrid(float mult = -1)
    {
        return inRm.GetRoomPosOnGrid(mult);
    }
}
