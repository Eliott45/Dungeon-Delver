using UnityEngine;

namespace __Scripts
{
    public class InRoom : MonoBehaviour
    {
        static public float ROOM_W = 16; // Ширина комнаты
        static public float ROOM_H = 11; // Высота комнаты
        static public float WALL_T = 2;  // Толщина стен

        // Максимальный размер карты
        static public int MAX_RM_X = 9;
        static public int MAX_RM_Y = 9;

        /// <summary>
        /// Хранит информацию об относительном расположении дверей
        /// </summary>
        static public Vector2[] DOORS = new Vector2[]
        {
            new Vector2(14, 5),
            new Vector2(7.5f, 9),
            new Vector2(1, 5),
            new Vector2(7.5f, 1)
        };

        [Header("Set in Inspector")]
        public bool keepInRoom = true;
        public float gridMult = 1;

        private void LateUpdate()
        {
            if(keepInRoom)
            {
                Vector2 rPos = RoomPos;
                // Mathf.Clamp - гарантирует, что координата будет иметь значение между минимальным значением WALL_T и максимальным значеним ROOM_W - 1 - WALL_T
                rPos.x = Mathf.Clamp(rPos.x, WALL_T, ROOM_W - 1 - WALL_T); 
                rPos.y = Mathf.Clamp(rPos.y, WALL_T, ROOM_H - 1 - WALL_T);
                RoomPos = rPos;
            }
        }

        /// <summary>
        /// Узнать местоположение этого персонажа в локальных координатах комнаты.
        /// </summary>
        public Vector2 RoomPos
        {
            get
            {
                Vector2 tPos = transform.position;
                tPos.x %= ROOM_W;
                tPos.y %= ROOM_H;
                return tPos;
            }
            set
            {
                Vector2 rm = RoomNum;
                rm.x *= ROOM_W;
                rm.y *= ROOM_H;
                rm += value;
                transform.position = rm;
            }
        }

        /// <summary>
        /// Узнать в какой комнате находится этот персонаж.
        /// </summary>
        public Vector2 RoomNum
        {
            get
            {
                Vector2 tPos = transform.position;
                tPos.x = Mathf.Floor(tPos.x / ROOM_W);
                tPos.y = Mathf.Floor(tPos.y / ROOM_H);
                return tPos;
            }
            set
            {
                Vector2 rPos = RoomPos;
                Vector2 rm = value;
                rm.x *= ROOM_W;
                rm.y *= ROOM_H;
                transform.position = rm + rPos;
            }
        }

        /// <summary>
        /// Вычисляет координаты узла сетки, ближайшего к данному персонажу
        /// </summary>
        public Vector2 GetRoomPosOnGrid(float mult = -1)
        {
            if(mult == -1)
            {
                mult = gridMult;
            }
            Vector2 rPos = RoomPos;
            rPos /= mult;
            rPos.x = Mathf.Round(rPos.x);
            rPos.y = Mathf.Round(rPos.y);
            rPos *= mult;
            return rPos;
        }
    }
}
