using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dray : MonoBehaviour, IFacingMover
{
    public enum eMode { idle, move, attack, transition };

    [Header("Set in Inpector")]
    public float speed = 5f; // Скорость передвижения
    public float attackDuration = 0.25f; // Продолжительность атаки
    public float attackDelay = 0.5f; // Задержка между атаками

    [Header("Set Dynamically")]
    public int dirHeld = -1; // Направление, соответствующее удерживаемой клавише
    public int facing = 1; // Направление движения Дрея
    public eMode mode = eMode.idle;

    private float timeAtkDone = 0; // Время, когда должна завершиться анимация атаки
    private float timeAtkNext = 0; // Время, когда Дрей сможет повторить атаку
    private Rigidbody rigid;
    private Animator anim;
    private InRoom inRm;
    private Vector3[] directions = new Vector3[] { Vector3.right, Vector3.up, Vector3.left, Vector3.down };
    private KeyCode[] keys = new KeyCode[] { KeyCode.RightArrow, KeyCode.UpArrow, KeyCode.LeftArrow, KeyCode.DownArrow };

    private void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        inRm = GetComponent<InRoom>();
    }

    private void Update()
    {
        //----Обработка ввода с клавиатуры и управление режимами eMode----\\
        dirHeld = -1; // Задать направление персонажу 
        for (int i = 0; i < 4; i++)
        {
            if (Input.GetKey(keys[i])) dirHeld = i;
        }

        /*
        if (Input.GetKey(KeyCode.RightArrow)) dirHeld = 0;
        if (Input.GetKey(KeyCode.UpArrow)) dirHeld = 1;
        if (Input.GetKey(KeyCode.LeftArrow)) dirHeld = 2;
        if (Input.GetKey(KeyCode.DownArrow)) dirHeld = 3;
        */

        // Нажата клавиша атаки
        if(Input.GetKeyDown(KeyCode.Z) && Time.time >= timeAtkDone) // Если нажата клавиша "Z" и прошло достаточко много времени после предыдущей атаки
        {
            mode = eMode.attack;
            timeAtkDone = Time.time + attackDuration;
            timeAtkNext = Time.time + attackDelay;
        }

        // Завершить атаку, если время истекло
        if(Time.time >= timeAtkDone)
        {
            mode = eMode.idle;
        }

        // Выбрать правильный режим, если дрей не атакует
        if (mode != eMode.attack)
        {
            if (dirHeld == -1)
            {
                mode = eMode.idle;
            } else
            {
                facing = dirHeld;
                mode = eMode.move;
            }
        }

        Vector3 vel = Vector3.zero;

        /*
        switch(dirHeld)
        {
            case 0:
                vel = Vector3.right;
                break;
            case 1:
                vel = Vector3.up;
                break;
            case 2:
                vel = Vector3.left;
                break;
            case 3:
                vel = Vector3.down;
                break;
        }
        */

        switch(mode)
        {
            case eMode.attack:
                anim.CrossFade("Dray_Attack_" + facing, 0);
                anim.speed = 0;
                break;
            case eMode.idle:
                anim.CrossFade("Dray_Walk_" + facing, 0); // Переключить анимациию
                anim.speed = 0;
                break;
            case eMode.move:
                vel = directions[dirHeld];
                anim.CrossFade("Dray_Walk_" + facing, 0);
                anim.speed = 1;
                break;
        }

        rigid.velocity = vel * speed;

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
            return (mode == eMode.move);
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
