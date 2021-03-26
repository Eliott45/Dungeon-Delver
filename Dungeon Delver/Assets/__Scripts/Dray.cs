﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dray : MonoBehaviour, IFacingMover, IKeyMaster
{
    public enum eMode { idle, move, attack, attack_2, transition, knockback };

    [Header("Set in Inpector")]
    public float speed = 5f; // Скорость передвижения
    public float attackDuration = 0.25f; // Продолжительность атаки
    public float attackDelay = 0.5f; // Задержка между атаками
    public float transitionDelay = 0.5f; // Задержка перехода между комнатами 
    public float dropSwordDurationg = 0.5f;
    public float dropSwordDelay = 1.5f;
    public int maxHealth = 10; // Уровень здоровья персонажа
    public float knockbackSpeed = 10;
    public float knockbackDuration = 0.25f;
    public float invincibleDuration = 0.5f; // Секунд неуязвимости после удара

    [Header("Set Dynamically")]
    public int dirHeld = -1; // Направление, соответствующее удерживаемой клавише
    public int facing = 3; // Направление движения Дрея
    public eMode mode = eMode.idle;
    public int numKeys = 0;
    public bool invincible = false;
    public bool hasGrappler = false;
    public bool hasUpgradeSword = false;
    public Vector3 lastSafeLoc;
    public int lastSafeFacing;
    public bool dropingSwords = false;

    [SerializeField]
    private int _health;

    public int health
    {
        get { return _health; }
        set { _health = value; }
    }

    private float timeAtkDone = 0; // Время, когда должна завершиться анимация атаки
    private float timeAtkNext = 0; // Время, когда Дрей сможет повторить атаку

    private float timeDropDone = 0; 
    private float timeDropNext = 0; 

    private float transitionDone = 0;
    private Vector2 transitionPos;
    private float knockbackDone = 0;
    private float invincibleDone = 0;
    private Vector3 knockbackVel;

    private SpriteRenderer sRend;
    private Rigidbody rigid;
    private Animator anim;
    private InRoom inRm;
    private Vector3[] directions = new Vector3[] { Vector3.right, Vector3.up, Vector3.left, Vector3.down };
    private KeyCode[] keys = new KeyCode[] { KeyCode.RightArrow, KeyCode.UpArrow, KeyCode.LeftArrow, KeyCode.DownArrow };

    private void Awake()
    {
        sRend = GetComponent<SpriteRenderer>();
        rigid = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        inRm = GetComponent<InRoom>();
        health = maxHealth; // Назначить максимальное здоровье, при начале игры
        lastSafeLoc = transform.position; // Начальная позиция безопасна
        lastSafeFacing = facing;
    }

    private void Update()
    {
        // Проверить состояние неуязвимости и необходимость выполнить отбрасывание
        if (invincible && Time.time > invincibleDone) invincible = false;
        sRend.color = invincible ? Color.red : Color.white;
        if (mode == eMode.knockback)
        {
            rigid.velocity = knockbackVel;
            if (Time.time < knockbackDone) return;
        }

        if(mode == eMode.transition)
        {
            rigid.velocity = Vector3.zero;
            anim.speed = 0;
            roomPos = transitionPos; // Оставить Дрея на месте
            if (Time.time < transitionDone) return;
            mode = eMode.idle;
        }

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
        if(Input.GetKeyDown(KeyCode.Z) && Time.time >= timeAtkDone && Time.time >= timeAtkNext ) // Если нажата клавиша "Z" и прошло достаточко много времени после предыдущей атаки
        {
            mode = eMode.attack;
            timeAtkDone = Time.time + attackDuration;
            timeAtkNext = Time.time + attackDelay;
        }

        if (Input.GetKeyDown(KeyCode.C) && Time.time >= timeDropDone && hasUpgradeSword && Time.time >= timeDropNext)
        {
            dropingSwords = false;
            mode = eMode.attack_2;
            timeDropDone = Time.time + dropSwordDurationg;
            timeDropNext = Time.time + dropSwordDelay;
        }

        // Завершить атаку, если время истекло
        if (Time.time >= timeAtkDone && Time.time >= timeDropDone)
        {
            mode = eMode.idle;
        }


        // Выбрать правильный режим, если дрей не атакует
        if (mode != eMode.attack && mode != eMode.attack_2)
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
            case eMode.attack_2:
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

    private void LateUpdate()
    {
        // Получить координаты узла сетки, с размером ячейки в половину единицы, ближайшего к данному персонажу
        Vector2 rPos = GetRoomPosOnGrid(0.5f); // Размер ячейки в пол-единицы

        // Проверить находится ли персонаж на плитке с дверью
        int doorNum;
        for (doorNum = 0; doorNum<4; doorNum++)
        {
            if(rPos == InRoom.DOORS[doorNum])
            {
                break;
            }
        }

        if (doorNum > 3 || doorNum != facing) return;

        // Перейти в следующую комнату
        Vector2 rm = roomNum;
        switch(doorNum)
        {
            case 0:
                rm.x += 1;
                break;
            case 1:
                rm.y += 1;
                break;
            case 2:
                rm.x -= 1;
                break;
            case 3:
                rm.y -= 1;
                break;
        }

        // Проверить, можно ли выполнить переход в комнату rm
        if (rm.x >= 0 && rm.x <= InRoom.MAX_RM_X)
        {
            if(rm.y >= 0 && rm.y <= InRoom.MAX_RM_Y)
            {
                roomNum = rm;
                transitionPos = InRoom.DOORS[(doorNum + 2) % 4];
                roomPos = transitionPos;
                lastSafeLoc = transform.position;
                lastSafeFacing = facing;
                mode = eMode.transition;
                transitionDone = Time.time + transitionDelay;
            }
        }
    }

    private void OnCollisionEnter(Collision coll)
    {
        if (invincible) return; // Выйти, если Дрей пока неуязвим
        DamageEffect dEf = coll.gameObject.GetComponent<DamageEffect>();
        if (dEf == null) return; // Если компонент DamageEffect отсуствует - выйти

        health -= dEf.damage; // Вычесть вылечену ущерба из уровня здоровья 
        invincible = true; // Сделать Дрея неуязвимым 
        invincibleDone = Time.time + invincibleDuration;

        if (dEf.knockback) // Выполнить отбрасывание
        {
            // Определить направление отбрасывания
            Vector3 delta = transform.position - coll.transform.position;
            if (Mathf.Abs(delta.x) >= Mathf.Abs(delta.y))
            {
                // Отбрасывание по горизонтали
                delta.x = (delta.x > 0) ? 1 : -1;
                delta.y = 0;
            } else
            {
                // Отбрасывание по вертикали
                delta.x = 0;
                delta.y = (delta.y > 0) ? 1 : -1;
            }

            // Применить скорость отскока к компоненту Rigidbody
            knockbackVel = delta * knockbackSpeed;
            rigid.velocity = knockbackVel;

            // Установить режим knockback и время прекращения отбрасывания
            mode = eMode.knockback;
            knockbackDone = Time.time + knockbackDuration;
        }
    }

    private void OnTriggerEnter(Collider colld)
    {
        PickUp pup = colld.GetComponent<PickUp>(); // Получить скрипт PickUp
        if (pup == null) return;

        switch(pup.itemType)
        {
            case PickUp.eType.health:
                health = Mathf.Min(health + 2, maxHealth);
                break;
            case PickUp.eType.key:
                keyCount++;
                break;  
            case PickUp.eType.grappler:
                hasGrappler = true;
                break;
            case PickUp.eType.upgrade_sword:
                hasUpgradeSword = true;
                break;
        }

        Destroy(colld.gameObject);
    }

    public void ResetInRoom(int healthLoss = 0)
    {
        transform.position = lastSafeLoc;
        facing = lastSafeFacing;
        health -= healthLoss;

        invincible = true; // Сделать Дрея неуязвимым
        invincibleDone = Time.time + invincibleDuration;
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

    // Реализация интерфейса IKeyMaster
    public int keyCount
    {
        get { return numKeys; }
        set { numKeys = value; }
    }
}
