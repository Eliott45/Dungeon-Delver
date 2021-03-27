using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordProjectile : MonoBehaviour
{
    protected static Vector3[] directions = new Vector3[]{
        Vector3.right, Vector3.up, Vector3.left, Vector3.down
    };

    public float speed = 3f;
    public int direction;
    public float flyDuration = 1f;
    public float flyDone;
    private Vector3 range;
    private int cells = 0;
    private Rigidbody rigid;

    private void Awake()
    {
        range = transform.position;
        rigid = GetComponent<Rigidbody>();
        flyDone = Time.time + flyDuration;
    }

    private void Update()
    {
        rigid.velocity = directions[direction] * speed;
    }

    private void LateUpdate()
    {
        if (flyDone <= Time.time)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider colld)
    {
        cells++;
        DamageEffect dEf = colld.gameObject.GetComponent<DamageEffect>(); // Получить из объекта скрипт DamageEffect

        if (dEf == null) return;

        Destroy(gameObject);
    }
}
