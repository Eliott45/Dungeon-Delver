using UnityEngine;

namespace __Scripts
{
    public class DamageEffect : MonoBehaviour
    {
        [Header("Set in Inspector")]
        public int damage = 1;
        public bool knockback = true;
        public bool stun = false;
    }
}
