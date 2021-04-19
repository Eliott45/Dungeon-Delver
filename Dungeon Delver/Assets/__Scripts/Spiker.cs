using UnityEngine;
using System.Collections;

public class Spiker : MonoBehaviour {

    enum eMode { search, attack, retract };

    [Header("Set in Inspector")]
    public float            sensorRange = 0.75f;
    public float            attackSpeed = 6;
    public float            retractSpeed = 3;
    public float            radius = 0.4f;

    private eMode           mode = eMode.search;
    private InRoom          _inRm;
    private Dray            _dray;
    private Vector3         p0, p1;

	void Start () {
        _inRm = GetComponent<InRoom>();

        GameObject go = GameObject.Find("Dray");
        _dray = go.GetComponent<Dray>();
	}
	
	void Update () {
        switch (mode) {
            case eMode.search:
                // Проверить в этой ли комнате Дрейк
                if (_dray.RoomNum != _inRm.RoomNum) return;

                float moveAmt;
                if ( Mathf.Abs( _dray.RoomPos.x - _inRm.RoomPos.x ) < sensorRange ) {
                    // Attack Vertically
                    moveAmt = ( InRoom.ROOM_H - (InRoom.WALL_T*2) )/2 - 1; //0.5f;
                    // The -0.5f above accounts for radius of Spiker
                    p1 = p0 = transform.position;
                    if (_inRm.RoomPos.y < InRoom.ROOM_H/2) {
                        p1.y += moveAmt; 
                    } else {
                        p1.y -= moveAmt;
                    }
                    mode = eMode.attack;
                }

                if ( Mathf.Abs( _dray.RoomPos.y - _inRm.RoomPos.y ) < sensorRange ) {
                    // Attack Horizontally
                    moveAmt = ( InRoom.ROOM_W - (InRoom.WALL_T*2) )/2 - 1;//0.5f;
                    p1 = p0 = transform.position;
                    if (_inRm.RoomPos.x < InRoom.ROOM_W/2) {
                        p1.x += moveAmt; 
                    } else {
                        p1.x -= moveAmt;
                    }
                    mode = eMode.attack;
                }
                break;
        }
    }

    void FixedUpdate() {
        Vector3 dir, pos, delta;

        switch (mode) {
            case eMode.attack:
                dir = (p1 - p0).normalized;
                pos = transform.position;
                delta = dir * attackSpeed * Time.fixedDeltaTime;
                if (delta.magnitude > (p1-pos).magnitude) {
                    // We're close enough to switch directions
                    transform.position = p1;
                    mode = eMode.retract;
                    break;
                }
                transform.position = pos + delta;
                break;

            case eMode.retract:
                dir = (p1 - p0).normalized;
                pos = transform.position;
                delta = dir * retractSpeed * Time.fixedDeltaTime;
                if (delta.magnitude > (p0-pos).magnitude) {
                    // We're close enough to switch directions
                    transform.position = p0;
                    mode = eMode.search;
                    break;
                }
                transform.position = pos - delta;
                break;

        }
	}
}
