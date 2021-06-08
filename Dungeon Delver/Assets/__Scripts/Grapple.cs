﻿using System.Collections.Generic;
using __Scripts.ProtoTools;
using UnityEngine;

namespace __Scripts
{
    public class Grapple : MonoBehaviour
    {
        public enum eMode { none, gOut, gInMiss, gInHit}

        [Header("Set in Inspector")]
        public float grappleSpd = 10; // Скорость крюка
        public float grappleLength = 7;
        public float grappleInLength = 0.5f;
        public int unsafeTileHealthPenalty = 2;
        public TextAsset mapGrappleable;

        [Header("Set Dynamically")]
        public eMode mode = eMode.none;
        // Номера плиток, на которые можно забросить крюк
        public List<int> grappleTiles;
        public List<int> unsafeTiles;

        private Dray dray;
        private Rigidbody rigid;
        private Animator anim;
        private Collider drayColld;

        private GameObject grapHead;
        private LineRenderer grapLine;
        private Vector3 p0, p1;
        private int facing;

        private Vector3[] directions = new Vector3[]
        {
            Vector3.right, Vector3.up, Vector3.left, Vector3.down
        };

        private void Awake()
        {
            string gTiles = mapGrappleable.text;
            gTiles = Utils.RemoveLineEndings(gTiles);
            grappleTiles = new List<int>();
            unsafeTiles = new List<int>();
            for (int i = 0; i <gTiles.Length; i++)
            {
                switch (gTiles[i])
                {
                    case 'S':
                        grappleTiles.Add(i);
                        break;
                    case 'X':
                        unsafeTiles.Add(i);
                        break;
                }
            }

            dray = GetComponent<Dray>();
            rigid = GetComponent<Rigidbody>();
            anim = GetComponent<Animator>();
            drayColld = GetComponent<Collider>();

            Transform trans = transform.Find("Grappler");
            grapHead = trans.gameObject;
            grapLine = grapHead.GetComponent<LineRenderer>();
            grapHead.SetActive(false);
        }

        private void Update()
        {
            if (!dray.hasGrappler) return;

            switch(mode)
            {
                case eMode.none:
                    // Если нажата клавиша применения крюка
                    if (Input.GetKeyDown(KeyCode.X))
                    {
                        StartGrapple();
                    }
                    break;
            }
        }

        void StartGrapple()
        {
            facing = dray.GetFacing();
            dray.enabled = false;
            anim.CrossFade("Dray_Attack_" + facing, 0);
            drayColld.enabled = false;
            rigid.velocity = Vector3.zero;

            grapHead.SetActive(true);

            p0 = transform.position + (directions[facing] * 0.5f);
            p1 = p0;
            grapHead.transform.position = p1;
            grapHead.transform.rotation = Quaternion.Euler(0, 0, 90 * facing);

            grapLine.positionCount = 2;
            grapLine.SetPosition(0, p0);
            grapLine.SetPosition(1, p1);
            mode = eMode.gOut;
        }

        private void FixedUpdate()
        {
            switch (mode)
            {
                case eMode.gOut: // Крюк брошен
                    p1 += directions[facing] * grappleSpd * Time.fixedDeltaTime;
                    grapHead.transform.position = p1;
                    grapLine.SetPosition(1, p1);

                    // Проверить, попал ли крюк куда нибудь
                    int tileNum = TileCamera.GET_MAP(p1.x, p1.y);
                    if (grappleTiles.IndexOf(tileNum) != -1)
                    {
                        // Крюк попал на плитку, за которую можно зацепиться!
                        mode = eMode.gInHit;
                        break;
                    }
                    if ( (p1-p0).magnitude >= grappleLength)
                    {
                        // Крюк улетел на всю длину веревки, но никуда не попал
                        mode = eMode.gInMiss;
                    }
                    break;

                case eMode.gInMiss: // Игрок промахнулся, вернуть крюк на удвоенной скорости
                    p1 -= directions[facing] * 2 * grappleSpd * Time.fixedDeltaTime;
                    if ( Vector3.Dot( (p1-p0), directions[facing] ) > 0 )
                    {
                        // Крюк еще перед дреем
                        grapHead.transform.position = p1;
                        grapLine.SetPosition(1, p1);
                    } else
                    {
                        StopGrapple();
                    }
                    break;
                case eMode.gInHit: // Крюк зацепился, поднять дрея на стену
                    float dist = grappleInLength + grappleSpd * Time.fixedDeltaTime;
                    if (dist > (p1-p0).magnitude)
                    {
                        p0 = p1 - (directions[facing] * grappleInLength);
                        transform.position = p0;
                        StopGrapple();
                        break;
                    }
                    p0 += directions[facing] * grappleSpd * Time.fixedDeltaTime;
                    transform.position = p0;
                    grapLine.SetPosition(0, p0);
                    grapHead.transform.position = p1;
                    break;
            }
        }

        void StopGrapple()
        {
            dray.enabled = true;
            drayColld.enabled = true;

            // Проверить безопасность плитки
            int tileNum = TileCamera.GET_MAP(p0.x, p0.y);
            if (mode == eMode.gInHit && unsafeTiles.IndexOf(tileNum) != -1)
            {
                // Дрей попал не небезопасную плитку
                dray.ResetInRoom(unsafeTileHealthPenalty);
            }

            grapHead.SetActive(false);

            mode = eMode.none;
        }

        private void OnTriggerEnter(Collider colld)
        {
            Enemy e = colld.GetComponent<Enemy>();
            if (e == null) return;

            mode = eMode.gInMiss;
        }
    }
}