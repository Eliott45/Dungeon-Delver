using System;
using UnityEngine;

namespace __Scripts
{
    public class Dray : MonoBehaviour, IFacingMover, IKeyMaster
    {
        public enum EMode { idle, move, attack, attack2, transition, knockback, dead };

        [Header("Set in Inpector")]
        [SerializeField] private float speed = 5f; // Скорость передвижения
        [SerializeField] private float attackDuration = 0.25f; // Продолжительность атаки
        [SerializeField] private float attackDelay = 0.5f; // Задержка между атаками
        [SerializeField] private float transitionDelay = 0.5f; // Задержка перехода между комнатами 
        [SerializeField] private float dropSwordDurationg = 0.5f;
        [SerializeField] private float dropSwordDelay = 1.5f;
        [SerializeField] private int maxHealth = 10; // Уровень здоровья персонажа
        [SerializeField] private float knockbackSpeed = 10;
        [SerializeField] private float knockbackDuration = 0.25f;
        [SerializeField] private float invincibleDuration = 0.5f; // Секунд неуязвимости после удара

        [Header("Set in Inspector: Sounds")]
        [SerializeField] private AudioClip swordSn;
        [SerializeField] private AudioClip upgradeSwordSn;
        [SerializeField] private AudioClip damageSn;
        [SerializeField] private AudioClip healthSn;
        [SerializeField] private AudioClip keySn;
        [SerializeField] private AudioClip upgradeSn;
        [SerializeField] private AudioClip fallSn;
        [SerializeField] private AudioClip dieSn;
        [SerializeField] private AudioClip switchDoorSn;

        [Header("Set in Inspector: UI")]
        [SerializeField] private GameObject deathScreen;
        [SerializeField] private GameObject pauseScreen;
        [SerializeField] private GameObject tips;

        [Header("Set Dynamically")]
        [SerializeField] private int dirHeld = -1; // Направление, соответствующее удерживаемой клавише
        public int facing = 3; // Направление движения Дрея
        public EMode mode = EMode.idle;
        public int numKeys;
        [SerializeField] private bool invincible;
        public bool hasGrapple;
        [SerializeField] private bool hasUpgradeSword;
        [SerializeField] private Vector3 lastSafeLoc;
        [SerializeField] private int lastSafeFacing;
        public bool droppingSwords;

        public int Health { get; private set; }

        private bool _pause;
        private bool _tipsShowing;

        private float _timeAtkDone; // Время, когда должна завершиться анимация атаки
        private float _timeAtkNext; // Время, когда Дрей сможет повторить атаку

        private float _timeDropDone; 
        private float _timeDropNext; 

        private float _transitionDone;
        private Vector2 _transitionPos;
        private float _knockbackDone;
        private float _invincibleDone;
        private Vector3 _knockbackVel;

        private SpriteRenderer _sRend;
        private Rigidbody _rigid;
        private Animator _anim;
        private InRoom _inRm;
        private AudioSource _aud;
        private readonly Vector3[] directions = { Vector3.right, Vector3.up, Vector3.left, Vector3.down };
        private readonly KeyCode[] keys = { KeyCode.RightArrow, KeyCode.UpArrow, KeyCode.LeftArrow, KeyCode.DownArrow };

        private void Awake()
        {
            _aud = GetComponent<AudioSource>();
            _sRend = GetComponent<SpriteRenderer>();
            _rigid = GetComponent<Rigidbody>();
            _anim = GetComponent<Animator>();
            _inRm = GetComponent<InRoom>();
            Health = maxHealth; // Назначить максимальное здоровье, при начале игры
            lastSafeLoc = transform.position; // Начальная позиция безопасна
            lastSafeFacing = facing;
        }

        private void Update()
        {
            if (mode == EMode.dead) return;

            // Проверить состояние неуязвимости и необходимость выполнить отбрасывание
            if (invincible && Time.time > _invincibleDone) invincible = false;
            _sRend.color = invincible ? Color.red : Color.white;
            
            if (mode == EMode.knockback)
            {
                _rigid.velocity = _knockbackVel;
                if (Time.time < _knockbackDone) return;
            }

            if(mode == EMode.transition)
            {
                _rigid.velocity = Vector3.zero;
                _anim.speed = 0;
                RoomPos = _transitionPos; // Оставить Дрея на месте
                if (Time.time < _transitionDone) return;
                mode = EMode.idle;
            }


            //----Обработка ввода с клавиатуры и управление режимами eMode----\\
            dirHeld = -1; // Задать направление персонажу 
            for (var i = 0; i < 4; i++)
            {
                if (Input.GetKey(keys[i]))
                {
                    dirHeld = i;
                }
            }
            
            // Нажата клавиша атаки
            if(Input.GetKeyDown(KeyCode.Z) && Time.time >= _timeAtkDone && Time.time >= _timeAtkNext ) // Если нажата клавиша "Z" и прошло достаточко много времени после предыдущей атаки
            {
                _aud.PlayOneShot(swordSn);
                mode = EMode.attack;
                _timeAtkDone = Time.time + attackDuration;
                _timeAtkNext = Time.time + attackDelay;
            }

            if (Input.GetKeyDown(KeyCode.C) && Time.time >= _timeDropDone && hasUpgradeSword && Time.time >= _timeDropNext)
            {
                _aud.PlayOneShot(upgradeSwordSn);
                droppingSwords = false;
                mode = EMode.attack2;
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
                mode = EMode.idle;
            }


            // Выбрать правильный режим, если дрей не атакует
            if (mode != EMode.attack && mode != EMode.attack2)
            {
                if (dirHeld == -1)
                {
                    mode = EMode.idle;
                } else
                {
                    facing = dirHeld;
                    mode = EMode.move;
                }
            }

            var vel = Vector3.zero;
            

            switch(mode)
            {
                case EMode.attack:
                case EMode.attack2:
                    _anim.CrossFade("Dray_Attack_" + facing, 0);
                    _anim.speed = 0;
                    break;
                case EMode.idle:
                    _anim.CrossFade("Dray_Walk_" + facing, 0); // Переключить анимациию
                    _anim.speed = 0;
                    break;
                case EMode.move:
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
            var rPos = GetRoomPosOnGrid(0.5f); // Размер ячейки в пол-единицы

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
            var rm = RoomNum;
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
            if (!(rm.x >= 0) || !(rm.x <= InRoom.MAX_RM_X)) return;
            if (!(rm.y >= 0) || !(rm.y <= InRoom.MAX_RM_Y)) return;
            RoomNum = rm;
            _transitionPos = InRoom.DOORS[(doorNum + 2) % 4];
            RoomPos = _transitionPos;
            lastSafeLoc = transform.position;
            lastSafeFacing = facing;
            mode = EMode.transition;
            _aud.PlayOneShot(switchDoorSn);
            _transitionDone = Time.time + transitionDelay;
        }

        private void OnCollisionEnter(Collision coll)
        {
            if (mode == EMode.dead) return;
            if (invincible) return; // Выйти, если Дрей пока неуязвим
            var dEf = coll.gameObject.GetComponent<DamageEffect>();
            if (dEf == null) return; // Если компонент DamageEffect отсуствует - выйти

            Health -= dEf.damage; // Вычесть вылечену ущерба из уровня здоровья 
            _aud.PlayOneShot(damageSn);
            if(Health <= 0)
            {
                _aud.PlayOneShot(dieSn);
                deathScreen.SetActive(true);
                mode = EMode.dead;
                return;
            }
            invincible = true; // Сделать Дрея неуязвимым 
            _invincibleDone = Time.time + invincibleDuration;

            if (!dEf.knockback) return;
            // Определить направление отбрасывания
            var delta = transform.position - coll.transform.position;
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
            mode = EMode.knockback;
            _knockbackDone = Time.time + knockbackDuration;
        }

        private void OnTriggerEnter(Collider coll)
        {
            var pup = coll.GetComponent<PickUp>(); // Получить скрипт PickUp
            if (pup == null) return;

            switch(pup.itemType)
            {
                case PickUp.eType.health:
                    Health = Mathf.Min(Health + 2, maxHealth);
                    _aud.PlayOneShot(healthSn);
                    break;
                case PickUp.eType.key:
                    KeyCount++;
                    _aud.PlayOneShot(keySn);
                    break;  
                case PickUp.eType.grappler:
                    hasGrapple = true;
                    _aud.PlayOneShot(upgradeSn);
                    break;
                case PickUp.eType.upgrade_sword:
                    hasUpgradeSword = true;
                    _aud.PlayOneShot(upgradeSn);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            Destroy(coll.gameObject);
        }

        public void ResetInRoom(int healthLoss = 0)
        {
            transform.position = lastSafeLoc;
            facing = lastSafeFacing;
            Health -= healthLoss;
            _aud.PlayOneShot(damageSn);
            _aud.PlayOneShot(fallSn);
            invincible = true; // Сделать Дрея неуязвимым
            _invincibleDone = Time.time + invincibleDuration;
            if (Health > 0) return;
            _aud.PlayOneShot(dieSn);
            deathScreen.SetActive(true);
            mode = EMode.dead;
        }


        // Реализация интерфейс IFacingMover
        public int GetFacing()
        {
            return facing;
        }

        public bool Moving => (mode == EMode.move);

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

        // Реализация интерфейса IKeyMaster
        public int KeyCount
        {
            get => numKeys;
            set => numKeys = value;
        }
    }
}
