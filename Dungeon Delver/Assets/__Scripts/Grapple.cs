using System.Collections.Generic;
using __Scripts.ProtoTools;
using UnityEngine;

namespace __Scripts
{
    public class Grapple : MonoBehaviour
    {
        private enum EMode { none, gOut, gInMiss, gInHit}

        [Header("Set in Inspector")]
        [SerializeField] private float grappleSpd = 10; // Скорость крюка
        [SerializeField] private float grappleLength = 7;
        [SerializeField] private float grappleInLength = 0.5f;
        [SerializeField] private int unsafeTileHealthPenalty = 2;
        [SerializeField] private TextAsset mapGrappleable;

        [Header("Set Dynamically")]
        [SerializeField] private EMode mode = EMode.none;
        // Номера плиток, на которые можно забросить крюк
        [SerializeField] private List<int> grappleTiles;
        [SerializeField] private List<int> unsafeTiles;

        private Dray dray;
        private Rigidbody rigid;
        private Animator anim;
        private Collider drayColld;

        private GameObject grapHead;
        private LineRenderer grapLine;
        private Vector3 p0, p1;
        private int facing;

        private readonly Vector3[] directions = new Vector3[]
        {
            Vector3.right, Vector3.up, Vector3.left, Vector3.down
        };

        private void Awake()
        {
            var gTiles = mapGrappleable.text;
            gTiles = Utils.RemoveLineEndings(gTiles);
            grappleTiles = new List<int>();
            unsafeTiles = new List<int>();
            for (var i = 0; i <gTiles.Length; i++)
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

            var trans = transform.Find("Grappler");
            grapHead = trans.gameObject;
            grapLine = grapHead.GetComponent<LineRenderer>();
            grapHead.SetActive(false);
        }

        private void Update()
        {
            if (!dray.hasGrapple) return;

            switch(mode)
            {
                case EMode.none:
                    // Если нажата клавиша применения крюка
                    if (Input.GetKeyDown(KeyCode.X))
                    {
                        StartGrapple();
                    }
                    break;
            }
        }

        private void StartGrapple()
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
            mode = EMode.gOut;
        }

        private void FixedUpdate()
        {
            switch (mode)
            {
                case EMode.gOut: // Крюк брошен
                    p1 += directions[facing] * (grappleSpd * Time.fixedDeltaTime);
                    grapHead.transform.position = p1;
                    grapLine.SetPosition(1, p1);

                    // Проверить, попал ли крюк куда нибудь
                    int tileNum = TileCamera.GET_MAP(p1.x, p1.y);
                    if (grappleTiles.IndexOf(tileNum) != -1)
                    {
                        // Крюк попал на плитку, за которую можно зацепиться!
                        mode = EMode.gInHit;
                        break;
                    }
                    if ( (p1-p0).magnitude >= grappleLength)
                    {
                        // Крюк улетел на всю длину веревки, но никуда не попал
                        mode = EMode.gInMiss;
                    }
                    break;

                case EMode.gInMiss: // Игрок промахнулся, вернуть крюк на удвоенной скорости
                    p1 -= directions[facing] * (2 * grappleSpd * Time.fixedDeltaTime);
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
                case EMode.gInHit: // Крюк зацепился, поднять дрея на стену
                    var dist = grappleInLength + grappleSpd * Time.fixedDeltaTime;
                    if (dist > (p1-p0).magnitude)
                    {
                        p0 = p1 - (directions[facing] * grappleInLength);
                        transform.position = p0;
                        StopGrapple();
                        break;
                    }
                    p0 += directions[facing] * (grappleSpd * Time.fixedDeltaTime);
                    transform.position = p0;
                    grapLine.SetPosition(0, p0);
                    grapHead.transform.position = p1;
                    break;
            }
        }

        private void StopGrapple()
        {
            dray.enabled = true;
            drayColld.enabled = true;

            // Проверить безопасность плитки
            var tileNum = TileCamera.GET_MAP(p0.x, p0.y);
            if (mode == EMode.gInHit && unsafeTiles.IndexOf(tileNum) != -1)
            {
                // Дрей попал не небезопасную плитку
                dray.ResetInRoom(unsafeTileHealthPenalty);
            }

            grapHead.SetActive(false);

            mode = EMode.none;
        }

        private void OnTriggerEnter(Collider colld)
        {
            var e = colld.GetComponent<Enemy>();
            if (e == null) return;

            mode = EMode.gInMiss;
        }
    }
}