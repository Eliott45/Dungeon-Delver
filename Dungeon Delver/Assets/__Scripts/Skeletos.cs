using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skeletos : Enemy, IFacingMover
{
    [Header("Set in Inspector: Skeletos")]
    public int speed = 2; // Скорость пермещение
    public float timeThinkMin = 1f; // Минимальное время следующей смены направления
    public float timeThinkMax = 4f; // Максимальное время следующей смены направления

    [Header("Set Dynamically: Skeletos")]
    public int facing = 0; // Направление
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
        } else
        {
            speed = 2;
        }


        if (Time.time >= timeNextDecision) { // Если время смены направление прошло
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
