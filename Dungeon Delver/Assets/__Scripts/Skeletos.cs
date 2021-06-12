using UnityEngine;

namespace __Scripts
{
    public class Skeletos : Enemy, IFacingMover
    {
        [Header("Set in Inspector: Skeletos")]
        [SerializeField] private int speed = 2; // Скорость пермещение
        [SerializeField] private float timeThinkMin = 1f; // Минимальное время следующей смены направления
        [SerializeField] private float timeThinkMax = 4f; // Максимальное время следующей смены направления

        [Header("Set Dynamically: Skeletos")]
        [SerializeField] private int facing; // Направление
        [SerializeField] private float timeNextDecision;

        private InRoom _inRm;

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
                speed = 0;
                rigid.velocity = Directions[facing] * speed;
                return;
            } else
            {
                speed = 2;
            }

            if (Time.time >= timeNextDecision) { // Если время смены направление прошло
                DecideDirection(); // Решить куда двигаться дальше
            }

            rigid.velocity = Directions[facing] * speed;  // Поле rigid унаследовано от класса Enemy и инициализируется в Enemy.Awake()
        }

        /// <summary>
        /// Выбирается случайное направление, и случайное время следующей смены направления
        /// </summary>
        private void DecideDirection()
        {
            facing = Random.Range(0, 4); // Случайное направление
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
