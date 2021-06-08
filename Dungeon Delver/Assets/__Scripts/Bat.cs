using UnityEngine;

namespace __Scripts
{
    public class Bat : Enemy, IFacingMover
    {
        [Header("Set in Inspector: Bat")]
        public int speed = 4; // Скорость пермещение
        public float timeThinkMin = 0.8f; // Минимальное время следущей смены направления
        public float timeThinkMax = 1.5f; // Максимальное время следущей смены направления

        [Header("Set Dynamically: Bat")]
        public int facing = 0;
        public float timeNextDecision = 0;

        private InRoom _inRm;
        private const int _MinSpeed = 0;
        private const int _MaxSpeed = 4;

        protected override void Awake()
        {
            base.Awake();
            _inRm = GetComponent<InRoom>();
        }

        protected override void Update()
        {
            base.Update();
            if (knockback) return; // Если скелет неуязвим 
            if (stun) // Если скелет под эфектом шока
            {
                speed = _MinSpeed;
                rigid.velocity = directions[facing] * speed;
                return;
            }
            else
            {
                speed = _MaxSpeed;
            }


            if (Time.time >= timeNextDecision)
            { // Если время смены направление прошло
                DecideDirection(); // Решить куда двигаться дальше
            }

            rigid.velocity = directions[facing] * speed;  // Поле rigid унаследовано от класса Enemy и инициализируется в Enemy.Awake()
        }

        /// <summary>
        /// Выбирается случайное направление, и случайное время следующей смены направления
        /// </summary>
        void DecideDirection()
        {
            facing = Random.Range(0, 4); // Случайное направление
            anim.CrossFade("Bat_" + facing, 0);
            timeNextDecision = Time.time + Random.Range(timeThinkMin, timeThinkMax); // Случайное время следующей смены направления
        }

        // Реализация интерфейс IFacingMover
        public int GetFacing()
        {
            return facing;
        }

        public bool Moving
        {
            get
            {
                return (true);
            }
        }

        public float GetSpeed()
        {
            return speed;
        }

        public float GridMult
        {
            get { return _inRm.gridMult; }
        }

        public Vector2 RoomPos
        {
            get { return _inRm.RoomPos; }
            set { _inRm.RoomPos = value; }
        }

        public Vector2 RoomNum
        {
            get { return _inRm.RoomNum; }
            set { _inRm.RoomNum = value; }
        }

        public Vector2 GetRoomPosOnGrid(float mult = -1)
        {
            return _inRm.GetRoomPosOnGrid(mult);
        }
    }
}
