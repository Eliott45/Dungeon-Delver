using UnityEngine;

namespace __Scripts
{
    public class Enemy : MonoBehaviour
    {
        protected static readonly Vector3[] Directions = 
        {
            Vector3.right, Vector3.up, Vector3.left, Vector3.down
        };

        [Header("Set in Inspector: Enemy")]
        [SerializeField] private float maxHealth = 1;
        [SerializeField] private float knockbackSpeed = 10;
        [SerializeField] private float knockbackDuration = 0.25f;
        [SerializeField] private float invincibleDuration = 0.5f;
        [SerializeField] private float stunDuration = 1f;
        [SerializeField] private float dieDuration = 1f;
        [SerializeField] private GameObject[] randomItemDrops;
        public GameObject guaranteedItemDrop;

        [Header("Set in Inspector: Enemy sounds")]
        [SerializeField] private AudioClip damageReceivedSn;
        [SerializeField] private AudioClip dieSn;

        [Header("Set Dynamically: Enemy")]
        [SerializeField] private float health;
        public bool invincible;
        public bool knockback;
        public bool stun; // Наличие шока (оглушение)

        private float _stunDone;
        private float _invincibleDone;
        private float _knockbackDone;
        public Vector3 knockbackVel;

        protected Animator anim;
        protected Rigidbody rigid;
        
        private SpriteRenderer sRend;
        private AudioSource aud;

        protected virtual void Awake()
        {
            health = maxHealth;
            aud = GetComponent<AudioSource>();
            anim = GetComponent<Animator>();
            rigid = GetComponent<Rigidbody>();
            sRend = GetComponent<SpriteRenderer>();
        }

        protected virtual void Update()
        {
            // Проверить состояние неуязвимости и необходимость выполнить отскок
            if (invincible && Time.time > _invincibleDone) invincible = false;
            sRend.color = invincible ? Color.red : Color.white;
            if (knockback && rigid != null)
            {
                rigid.velocity = knockbackVel;
                if (Time.time < _knockbackDone) return;
            }

            knockback = false;

            sRend.color = stun ? Color.cyan : Color.white;
            if (stun)
            {
                if (Time.time < _stunDone) return;
            }

            stun = false;

            anim.speed = 1;
        }

        private void OnTriggerEnter(Collider coll)
        {
            if (invincible) return; // Выйти, если скелет пока неуязвим
            if (stun) return;
            var dEf = coll.gameObject.GetComponent<DamageEffect>(); // Получить из объекта скрипт DamageEffect

            if (dEf == null) return; // Если компонент DamageEffect отсуствует - выйти

            if (dEf.stun) // Если оружие может оглушить 
            {
                stun = true; 
                _stunDone = Time.time + stunDuration;
                anim.speed = 0;
                return;
            }
            aud.PlayOneShot(damageReceivedSn);
            health -= dEf.damage; // Вычесть вылечену ущерба из уровня здоровья 
            if (health <= 0) { // Если здоровье нету, то умереть.
                aud.PlayOneShot(dieSn);
                _knockbackDone = Time.time + dieDuration;
                _invincibleDone = Time.time + dieDuration;
                GetComponent<SphereCollider>().enabled = false;
                invincible = true;
                knockback = true;
                Invoke(nameof(Die), dieDuration);
                return;
            }
            invincible = true; // Сделать врага неуязвимым 
            _invincibleDone = Time.time + invincibleDuration;

            if (!dEf.knockback) return;
            // Определить направление отбрасывания
            var delta = transform.position - coll.transform.position;
            if (Mathf.Abs(delta.x) >= Mathf.Abs(delta.y))
            {
                // Отбрасывание по горизонтали
                delta.x = (delta.x > 0) ? 1 : -1;
                delta.y = 0;
            }
            else
            {
                // Отбрасывание по вертикали
                delta.x = 0;
                delta.y = (delta.y > 0) ? 1 : -1;
            }

            // Применить скорость отскока к компоненту Rigidbody
            knockbackVel = delta * knockbackSpeed;
            rigid.velocity = knockbackVel;

            // Установить режим knockback и время прекращения отбрасывания
            knockback = true;
            _knockbackDone = Time.time + knockbackDuration;
            anim.speed = 0;
        }

        private void Die()
        {
            GameObject go;
            if(guaranteedItemDrop != null)
            {
                go = Instantiate(guaranteedItemDrop);
                go.transform.position = transform.position;
            } else if (randomItemDrops.Length > 0)
            {
                var n = Random.Range(0, randomItemDrops.Length);
                var prefab = randomItemDrops[n];
                if (prefab != null)
                {
                    go = Instantiate(prefab);
                    go.transform.position = transform.position;
                }
            }
            Destroy(gameObject);
        }
    }
}
