using UnityEngine;

namespace __Scripts
{
    public class SwordProjectile : MonoBehaviour
    {
        private static readonly Vector3[] Directions = {
            Vector3.right, Vector3.up, Vector3.left, Vector3.down
        };

        [SerializeField] private float speed = 5f;
        public int direction;
        [SerializeField] private float flyDuration = 1.5f;
        [SerializeField] private float flyDone;
        
        private Vector3 _range;
        private int _cells = 0;
        private Rigidbody _rigid;

        private void Awake()
        {
            _range = transform.position;
            _rigid = GetComponent<Rigidbody>();
            flyDone = Time.time + flyDuration;
        }

        private void Update()
        {
            _rigid.velocity = Directions[direction] * speed;
        }

        private void LateUpdate()
        {
            if (flyDone <= Time.time)
            {
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter(Collider colld)
        {
            _cells++;
            var dEf = colld.gameObject.GetComponent<DamageEffect>(); // Получить из объекта скрипт DamageEffect

            if (dEf == null) return;

            Destroy(gameObject);
        }
    }
}
