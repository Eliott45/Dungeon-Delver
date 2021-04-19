using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordController : MonoBehaviour
{
    public GameObject prefabSword;

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
        _sword.SetActive(_dray.mode == Dray.eMode.attack);
        _upgradeSword.SetActive(_dray.mode == Dray.eMode.attack_2);
        if (_dray.mode == Dray.eMode.attack_2 && !_dray.dropingSwords)
        {
            DropSword();
            _dray.dropingSwords = true;
        }
    }

    void DropSword()
    {
        GameObject go = Instantiate(prefabSword);
        go.transform.position = _dray.transform.position;
        go.GetComponent<SwordProjectile>().direction = _dray.facing;
        go.transform.rotation = Quaternion.Euler(0, 0, 90 * _dray.facing);
    }
}
