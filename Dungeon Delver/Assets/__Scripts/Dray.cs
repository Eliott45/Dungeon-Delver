using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dray : MonoBehaviour
{
    [Header("Set in Inpector")]
    public float speed = 5f;

    [Header("Set Dynamically")]
    public int dirHeld = -1; // Направление, соответствующее удерживаемой клавише
    private Rigidbody rigid;
    private Vector3[] directions = new Vector3[] { Vector3.right, Vector3.up, Vector3.left, Vector3.down };

    private void Awake()
    {
        rigid = GetComponent<Rigidbody>(); 
    }

    private void Update()
    {
        // Задать направление персонажу 
        dirHeld = -1;
        if (Input.GetKey(KeyCode.RightArrow)) dirHeld = 0;
        if (Input.GetKey(KeyCode.UpArrow)) dirHeld = 1;
        if (Input.GetKey(KeyCode.LeftArrow)) dirHeld = 2;
        if (Input.GetKey(KeyCode.DownArrow)) dirHeld = 3;

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


        if (dirHeld > -1) vel = directions[dirHeld];

        rigid.velocity = vel * speed;
    }
}
