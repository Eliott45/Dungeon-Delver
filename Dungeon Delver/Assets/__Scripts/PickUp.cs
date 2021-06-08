using UnityEngine;

namespace __Scripts
{
    /// <summary>
    /// Предмет.
    /// </summary>
    public class PickUp : MonoBehaviour
    {
        /// <summary>
        /// Тип предмета.
        /// </summary>
        public enum eType { key, health, grappler, upgrade_sword}

        public static float COLLIDER_DELAY = 0.5f;

        [Header("Set in Inspector")]
        public eType itemType; // Установить тип предмета

        // Awake() и Activate() деактивируют коллайдер на 0.5 секунд
        private void Awake()
        {
            GetComponent<Collider>().enabled = false;
            Invoke("Activate", COLLIDER_DELAY);
        }

        void Activate()
        {
            GetComponent<Collider>().enabled = true;
        }
    }
}
