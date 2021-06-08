using UnityEngine;

namespace __Scripts
{
    public class Bat : Enemy, IFacingMover
    {
        [Header("Set in Inspector: Bat")]
        [SerializeField] private int speed = 4; // Скорость пермещение
        [SerializeField] private float timeThinkMin = 0.6f; // Минимальное время следущей смены направления
        [SerializeField] private float timeThinkMax = 1.3f; // Максимальное время следущей смены направления

        [Header("Set Dynamically: Bat")]
        [SerializeField] private int facing ;
        [SerializeField] private float timeNextDecision;

        private InRoom _inRm;
        private const int MinSpeed = 0;
        private const int MaxSpeed = 4;

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
                speed = MinSpeed;
                rigid.velocity = directions[facing] * speed;
                return;
            }
            else
            {
                speed = MaxSpeed;
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

        public bool Moving => (true);

        public float GetSpeed()
        {
            return speed;
        }

        public float GridMult => _inRm.gridMult;

        public Vector2 RoomPos
        {
            get => _inRm.RoomPos;
            set => _inRm.RoomPos = value;
        }

        public Vector2 RoomNum
        {
            get => _inRm.RoomNum;
            set => _inRm.RoomNum = value;
        }

        public Vector2 GetRoomPosOnGrid(float mult = -1)
        {
            return _inRm.GetRoomPosOnGrid(mult);
        }
    }
}
