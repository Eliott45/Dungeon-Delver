using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    protected static Vector3[] directions = new Vector3[]
    {
        Vector3.right, Vector3.up, Vector3.left, Vector3.down
    };

    [Header("Set in Inspector: Enemy")]
    public float maxHealth = 1;
    public float knockbackSpeed = 10;
    public float knockbackDuration = 0.25f;
    public float invincibleDuration = 0.5f;
    public float stunDuration = 1f;
    public float dieDuration = 1f;
    public GameObject[] randomItemDrops;
    public GameObject guranteedItemDrop = null;

    [Header("Set in Inspector: Enemy sounds")]
    public AudioClip damageReceivedSn;
    public AudioClip dieSn;

    [Header("Set Dynamically: Enemy")]
    public float health;
    public bool invincible = false;
    public bool knockback = false;
    public bool stun = false; // Наличие шока (оглушение)
    public bool dead = false;

    private float _stunDone = 0;
    private float _invincibleDone = 0;
    private float _knockbackDone = 0;
    public Vector3 knockbackVel;

    protected Animator anim;
    protected Rigidbody rigid;
    protected SpriteRenderer sRend;
    protected AudioSource aud;

    protected virtual void Awake()
    {
        health = maxHealth;
        aud = GetComponent<AudioSource>();
        anim = GetComponent<Animator>();
        rigid = GetComponent<Rigidbody>();
        sRend = GetComponent<SpriteRenderer>();
    }

    protected virtual void Update()
    {
        // Проверить состояние неуязвимости и необходимость выполнить отскок
        if (invincible && Time.time > _invincibleDone) invincible = false;
        sRend.color = invincible ? Color.red : Color.white;
        if (knockback && rigid != null)
        {
            rigid.velocity = knockbackVel;
            if (Time.time < _knockbackDone) return;
        }

        knockback = false;

        sRend.color = stun ? Color.cyan : Color.white;
        if (stun)
        {
            if (Time.time < _stunDone) return;
        }

        stun = false;

        anim.speed = 1;
    }

    private void OnTriggerEnter(Collider colld)
    {
        if (invincible) return; // Выйти, если скелет пока неуязвим
        if (stun) return;
        DamageEffect dEf = colld.gameObject.GetComponent<DamageEffect>(); // Получить из объекта скрипт DamageEffect

        if (dEf == null) return; // Если компонент DamageEffect отсуствует - выйти

        if (dEf.stun) // Если оружие может оглушить 
        {
            stun = true; 
            _stunDone = Time.time + stunDuration;
            anim.speed = 0;
            return;
        }
        aud.PlayOneShot(damageReceivedSn);
        health -= dEf.damage; // Вычесть вылечену ущерба из уровня здоровья 
        if (health <= 0) { // Если здоровье нету, то умереть.
            aud.PlayOneShot(dieSn);
            _knockbackDone = Time.time + dieDuration;
            _invincibleDone = Time.time + dieDuration;
            GetComponent<SphereCollider>().enabled = false;
            invincible = true;
            knockback = true;
            Invoke("Die", dieDuration);
            return;
        }
        invincible = true; // Сделать врага неуязвимым 
        _invincibleDone = Time.time + invincibleDuration;

        if (dEf.knockback) // Выполнить отбрасывание
        {
            // Определить направление отбрасывания
            Vector3 delta = transform.position - colld.transform.position;
            if (Mathf.Abs(delta.x) >= Mathf.Abs(delta.y))
            {
                // Отбрасывание по горизонтали
                delta.x = (delta.x > 0) ? 1 : -1;
                delta.y = 0;
            }
            else
            {
                // Отбрасывание по вертикали
                delta.x = 0;
                delta.y = (delta.y > 0) ? 1 : -1;
            }

            // Применить скорость отскока к компоненту Rigidbody
            knockbackVel = delta * knockbackSpeed;
            rigid.velocity = knockbackVel;

            // Установить режим knockback и время прекращения отбрасывания
            knockback = true;
            _knockbackDone = Time.time + knockbackDuration;
            anim.speed = 0;
        }
    }

    void Die()
    {
        GameObject go;
        if(guranteedItemDrop != null)
        {
            go = Instantiate<GameObject>(guranteedItemDrop);
            go.transform.position = transform.position;
        } else if (randomItemDrops.Length > 0)
        {
            int n = Random.Range(0, randomItemDrops.Length);
            GameObject prefab = randomItemDrops[n];
            if (prefab != null)
            {
                go = Instantiate<GameObject>(prefab);
                go.transform.position = transform.position;
            }
        }
        Destroy(gameObject);
    }
}
