using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skeletos : Enemy
{
    [Header("Set in Inspector: Skeletos")]
    public int speed = 2;
    public float timeThinkMin = 1f;
    public float timeThinkMax;

    [Header("Set Dynamically: Skeletos")]
    public int facing = 0;
    public float timeNextDecision = 0;

    private void Update()
    {
        if(Time.time >= timeNextDecision) { // Если время смены направление прошло
            DecideDirection(); // Решить куда двигаться дальше
        }
        // Поле rigid унаследовано от класса Enemy и инициализируется в Enemy.Awake()
        rigid.velocity = directions[facing] * speed;
    }

    /// <summary>
    /// Выбирается случайное направление, и случайное время следующей смены направления
    /// </summary>
    void DecideDirection()
    {
        facing = Random.Range(0, 4); // Случайное направление
        timeNextDecision = Time.time + Random.Range(timeThinkMin, timeThinkMax); // Случайное время следующей смены направления
    }
}
