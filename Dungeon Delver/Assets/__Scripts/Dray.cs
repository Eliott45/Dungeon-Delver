using UnityEngine;

namespace __Scripts
{
    public class Dray : MonoBehaviour, IFacingMover, IKeyMaster
    {
        public enum eMode { idle, move, attack, attack_2, transition, knockback, dead };

        [Header("Set in Inpector")]
        public float speed = 5f; // Скорость передвижения
        public float attackDuration = 0.25f; // Продолжительность атаки
        public float attackDelay = 0.5f; // Задержка между атаками
        public float transitionDelay = 0.5f; // Задержка перехода между комнатами 
        public float dropSwordDurationg = 0.5f;
        public float dropSwordDelay = 1.5f;
        public int maxHealth = 10; // Уровень здоровья персонажа
        public float knockbackSpeed = 10;
        public float knockbackDuration = 0.25f;
        public float invincibleDuration = 0.5f; // Секунд неуязвимости после удара

        [Header("Set in Inspector: Sounds")]
        public AudioClip swordSn;
        public AudioClip upgradeSwordSn;
        public AudioClip damageSn;
        public AudioClip healthSn;
        public AudioClip keySn;
        public AudioClip upgradeSn;
        public AudioClip fallSn;
        public AudioClip dieSn;
        public AudioClip switchDoorSn;

        [Header("Set in Inspector: UI")]
        public GameObject deathScreen;
        public GameObject pauseScreen;
        public GameObject tips;

        [Header("Set Dynamically")]
        public int dirHeld = -1; // Направление, соответствующее удерживаемой клавише
        public int facing = 3; // Направление движения Дрея
        public eMode mode = eMode.idle;
        public int numKeys = 0;
        public bool invincible = false;
        public bool hasGrappler = false;
        public bool hasUpgradeSword = false;
        public Vector3 lastSafeLoc;
        public int lastSafeFacing;
        public bool dropingSwords = false;

        [SerializeField]
        private int _health;

        public int health
        {
            get { return _health; }
            set { _health = value; }
        }

        private bool _pause = false;
        private bool _tipsShowing = false;

        private float _timeAtkDone = 0; // Время, когда должна завершиться анимация атаки
        private float _timeAtkNext = 0; // Время, когда Дрей сможет повторить атаку

        private float _timeDropDone = 0; 
        private float _timeDropNext = 0; 

        private float _transitionDone = 0;
        private Vector2 _transitionPos;
        private float _knockbackDone = 0;
        private float _invincibleDone = 0;
        private Vector3 _knockbackVel;

        private SpriteRenderer _sRend;
        private Rigidbody _rigid;
        private Animator _anim;
        private InRoom _inRm;
        private AudioSource _aud;
        private Vector3[] directions = new Vector3[] { Vector3.right, Vector3.up, Vector3.left, Vector3.down };
        private KeyCode[] keys = new KeyCode[] { KeyCode.RightArrow, KeyCode.UpArrow, KeyCode.LeftArrow, KeyCode.DownArrow };

        private void Awake()
        {
            _aud = GetComponent<AudioSource>();
            _sRend = GetComponent<SpriteRenderer>();
            _rigid = GetComponent<Rigidbody>();
            _anim = GetComponent<Animator>();
            _inRm = GetComponent<InRoom>();
            health = maxHealth; // Назначить максимальное здоровье, при начале игры
            lastSafeLoc = transform.position; // Начальная позиция безопасна
            lastSafeFacing = facing;
        }

        private void Update()
        {
            if (mode == eMode.dead) return;

            // Проверить состояние неуязвимости и необходимость выполнить отбрасывание
            if (invincible && Time.time > _invincibleDone) invincible = false;
            _sRend.color = invincible ? Color.red : Color.white;
            if (mode == eMode.knockback)
            {
                _rigid.velocity = _knockbackVel;
                if (Time.time < _knockbackDone) return;
            }

            if(mode == eMode.transition)
            {
                _rigid.velocity = Vector3.zero;
                _anim.speed = 0;
                RoomPos = _transitionPos; // Оставить Дрея на месте
                if (Time.time < _transitionDone) return;
                mode = eMode.idle;
            }


            //----Обработка ввода с клавиатуры и управление режимами eMode----\\
            dirHeld = -1; // Задать направление персонажу 
            for (int i = 0; i < 4; i++)
            {
                if (Input.GetKey(keys[i]))
                {
                    dirHeld = i;
                }
            }

            /*
        if (Input.GetKey(KeyCode.RightArrow)) dirHeld = 0;
        if (Input.GetKey(KeyCode.UpArrow)) dirHeld = 1;
        if (Input.GetKey(KeyCode.LeftArrow)) dirHeld = 2;
        if (Input.GetKey(KeyCode.DownArrow)) dirHeld = 3;
        */

            // Нажата клавиша атаки
            if(Input.GetKeyDown(KeyCode.Z) && Time.time >= _timeAtkDone && Time.time >= _timeAtkNext ) // Если нажата клавиша "Z" и прошло достаточко много времени после предыдущей атаки
            {
                _aud.PlayOneShot(swordSn);
                mode = eMode.attack;
                _timeAtkDone = Time.time + attackDuration;
                _timeAtkNext = Time.time + attackDelay;
            }

            if (Input.GetKeyDown(KeyCode.C) && Time.time >= _timeDropDone && hasUpgradeSword && Time.time >= _timeDropNext)
            {
                _aud.PlayOneShot(upgradeSwordSn);
                dropingSwords = false;
                mode = eMode.attack_2;
                _timeDropDone = Time.time + dropSwordDurationg;
                _timeDropNext = Time.time + dropSwordDelay;
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                _pause = !_pause;
                pauseScreen.SetActive(_pause);
            }

            if (Input.GetKeyDown(KeyCode.H))
            {
                _tipsShowing = !_tipsShowing;
                tips.SetActive(_tipsShowing);
            }

            // Завершить атаку, если время истекло
            if (Time.time >= _timeAtkDone && Time.time >= _timeDropDone)
            {
                mode = eMode.idle;
            }


            // Выбрать правильный режим, если дрей не атакует
            if (mode != eMode.attack && mode != eMode.attack_2)
            {
                if (dirHeld == -1)
                {
                    mode = eMode.idle;
                } else
                {
                    facing = dirHeld;
                    mode = eMode.move;
                }
            }

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

            switch(mode)
            {
                case eMode.attack:
                case eMode.attack_2:
                    _anim.CrossFade("Dray_Attack_" + facing, 0);
                    _anim.speed = 0;
                    break;
                case eMode.idle:
                    _anim.CrossFade("Dray_Walk_" + facing, 0); // Переключить анимациию
                    _anim.speed = 0;
                    break;
                case eMode.move:
                    vel = directions[dirHeld];
                    _anim.CrossFade("Dray_Walk_" + facing, 0);
                    _anim.speed = 1;
                    break;
            }

            _rigid.velocity = vel * speed;

        }

        private void LateUpdate()
        {
            // Получить координаты узла сетки, с размером ячейки в половину единицы, ближайшего к данному персонажу
            Vector2 rPos = GetRoomPosOnGrid(0.5f); // Размер ячейки в пол-единицы

            // Проверить находится ли персонаж на плитке с дверью
            int doorNum;
            for (doorNum = 0; doorNum<4; doorNum++)
            {
                if(rPos == InRoom.DOORS[doorNum])
                {
                    break;
                }
            }

            if (doorNum > 3 || doorNum != facing) return;

            // Перейти в следующую комнату
            Vector2 rm = RoomNum;
            switch(doorNum)
            {
                case 0:
                    rm.x += 1;
                    break;
                case 1:
                    rm.y += 1;
                    break;
                case 2:
                    rm.x -= 1;
                    break;
                case 3:
                    rm.y -= 1;
                    break;
            }

            // Проверить, можно ли выполнить переход в комнату rm
            if (rm.x >= 0 && rm.x <= InRoom.MAX_RM_X)
            {
                if(rm.y >= 0 && rm.y <= InRoom.MAX_RM_Y)
                {
                    RoomNum = rm;
                    _transitionPos = InRoom.DOORS[(doorNum + 2) % 4];
                    RoomPos = _transitionPos;
                    lastSafeLoc = transform.position;
                    lastSafeFacing = facing;
                    mode = eMode.transition;
                    _aud.PlayOneShot(switchDoorSn);
                    _transitionDone = Time.time + transitionDelay;
                }
            }
        }

        private void OnCollisionEnter(Collision coll)
        {
            if (mode == eMode.dead) return;
            if (invincible) return; // Выйти, если Дрей пока неуязвим
            DamageEffect dEf = coll.gameObject.GetComponent<DamageEffect>();
            if (dEf == null) return; // Если компонент DamageEffect отсуствует - выйти

            health -= dEf.damage; // Вычесть вылечену ущерба из уровня здоровья 
            _aud.PlayOneShot(damageSn);
            if(health <= 0)
            {
                _aud.PlayOneShot(dieSn);
                deathScreen.SetActive(true);
                mode = eMode.dead;
                return;
            }
            invincible = true; // Сделать Дрея неуязвимым 
            _invincibleDone = Time.time + invincibleDuration;

            if (dEf.knockback) // Выполнить отбрасывание
            {
                // Определить направление отбрасывания
                Vector3 delta = transform.position - coll.transform.position;
                if (Mathf.Abs(delta.x) >= Mathf.Abs(delta.y))
                {
                    // Отбрасывание по горизонтали
                    delta.x = (delta.x > 0) ? 1 : -1;
                    delta.y = 0;
                } else
                {
                    // Отбрасывание по вертикали
                    delta.x = 0;
                    delta.y = (delta.y > 0) ? 1 : -1;
                }

                // Применить скорость отскока к компоненту Rigidbody
                _knockbackVel = delta * knockbackSpeed;
                _rigid.velocity = _knockbackVel;

                // Установить режим knockback и время прекращения отбрасывания
                mode = eMode.knockback;
                _knockbackDone = Time.time + knockbackDuration;
            }
        }

        private void OnTriggerEnter(Collider colld)
        {
            PickUp pup = colld.GetComponent<PickUp>(); // Получить скрипт PickUp
            if (pup == null) return;

            switch(pup.itemType)
            {
                case PickUp.eType.health:
                    health = Mathf.Min(health + 2, maxHealth);
                    _aud.PlayOneShot(healthSn);
                    break;
                case PickUp.eType.key:
                    KeyCount++;
                    _aud.PlayOneShot(keySn);
                    break;  
                case PickUp.eType.grappler:
                    hasGrappler = true;
                    _aud.PlayOneShot(upgradeSn);
                    break;
                case PickUp.eType.upgrade_sword:
                    hasUpgradeSword = true;
                    _aud.PlayOneShot(upgradeSn);
                    break;
            }

            Destroy(colld.gameObject);
        }

        public void ResetInRoom(int healthLoss = 0)
        {
            transform.position = lastSafeLoc;
            facing = lastSafeFacing;
            health -= healthLoss;
            _aud.PlayOneShot(damageSn);
            _aud.PlayOneShot(fallSn);
            invincible = true; // Сделать Дрея неуязвимым
            _invincibleDone = Time.time + invincibleDuration;
            if (health <= 0)
            {
                _aud.PlayOneShot(dieSn);
                deathScreen.SetActive(true);
                mode = eMode.dead;
            }
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
                return (mode == eMode.move);
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

        // Реализация интерфейса IKeyMaster
        public int KeyCount
        {
            get { return numKeys; }
            set { numKeys = value; }
        }
    }
}
