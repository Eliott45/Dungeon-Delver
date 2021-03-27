using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordController : MonoBehaviour
{
    public GameObject prefabSword;

    private GameObject sword;
    private GameObject upgradeSword;
    private Dray dray;

    private void Start()
    {
        sword = transform.Find("Sword").gameObject; // Получить объект меча
        upgradeSword = transform.Find("Upgrade_Sword").gameObject; 
        dray = transform.parent.GetComponent<Dray>(); 
        sword.SetActive(false); // Деактивировать меч
        upgradeSword.SetActive(false); // Деактивировать меч
    }

    private void Update()
    {
        transform.rotation = Quaternion.Euler(0, 0, 90 * dray.facing);
        sword.SetActive(dray.mode == Dray.eMode.attack);
        upgradeSword.SetActive(dray.mode == Dray.eMode.attack_2);
        if (dray.mode == Dray.eMode.attack_2 && !dray.dropingSwords)
        {
            DropSword();
            dray.dropingSwords = true;
        }
    }

    void DropSword()
    {
        GameObject go = Instantiate(prefabSword);
        go.transform.position = dray.transform.position;
        go.GetComponent<SwordProjectile>().direction = dray.facing;
        go.transform.rotation = Quaternion.Euler(0, 0, 90 * dray.facing);
    }
}
