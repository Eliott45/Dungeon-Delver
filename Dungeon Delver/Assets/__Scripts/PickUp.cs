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
        public enum EType { key, health, grappler, upgradeSword}

        private static float COLLIDER_DELAY = 0.5f;

        [Header("Set in Inspector")]
        public EType itemType; // Установить тип предмета

        // Awake() и Activate() деактивируют коллайдер на 0.5 секунд
        private void Awake()
        {
            GetComponent<Collider>().enabled = false;
            Invoke(nameof(Activate), COLLIDER_DELAY);
        }

        private void Activate()
        {
            GetComponent<Collider>().enabled = true;
        }
    }
}
