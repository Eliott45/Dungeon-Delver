using UnityEngine;

namespace __Scripts
{
    public class SwordController : MonoBehaviour
    {
        [SerializeField] private GameObject prefabSword;

        private GameObject _sword;
        private GameObject _upgradeSword;
        private Dray _dray;

        private void Start()
        {
            _sword = transform.Find("Sword").gameObject; // Получить объект меча
            _upgradeSword = transform.Find("Upgrade_Sword").gameObject; 
            _dray = transform.parent.GetComponent<Dray>(); 
            _sword.SetActive(false); // Деактивировать меч
            _upgradeSword.SetActive(false); // Деактивировать меч
        }

        private void Update()
        {
            transform.rotation = Quaternion.Euler(0, 0, 90 * _dray.facing);
            _sword.SetActive(_dray.mode == Dray.EMode.attack);
            _upgradeSword.SetActive(_dray.mode == Dray.EMode.attack2);
            if (_dray.mode != Dray.EMode.attack2 || _dray.droppingSwords) return;
            DropSword();
            _dray.droppingSwords = true;
        }

        private void DropSword()
        {
            var go = Instantiate(prefabSword);
            go.transform.position = _dray.transform.position;
            go.GetComponent<SwordProjectile>().direction = _dray.facing;
            go.transform.rotation = Quaternion.Euler(0, 0, 90 * _dray.facing);
        }
    }
}
