using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace __Scripts
{
    public class GuiPanel : MonoBehaviour
    {
        [Header("Set in Inspector")]
        [SerializeField] private Dray dray;
        [SerializeField] private Sprite healthEmpty;
        [SerializeField] private Sprite healthHalf;
        [SerializeField] private Sprite healthFull;

        private Text keyCountText;
        private List<Image> healthImages;

        private void Start()
        {
            // Счетчик ключей
            var trans = transform.Find("Key Count");
            keyCountText = trans.GetComponent<Text>();

            // Индикатор уровня здоровья
            var healthPanel = transform.Find("Health Panel");
            healthImages = new List<Image>();
            if (healthPanel == null) return;
            for (var i = 0; i < 20; i++)
            {
                trans = healthPanel.Find("H_" + i);
                if (trans == null) break;
                healthImages.Add(trans.GetComponent<Image>());
            }
        }

        private void Update()
        {
            // Показать количество ключей
            keyCountText.text = dray.numKeys.ToString();

            // Показать уровень здоровья
            var health = dray.Health;
            foreach (var t in healthImages)
            {
                if(health > 1)
                {
                    t.sprite = healthFull;
                } else if(health == 1)
                {
                    t.sprite = healthHalf;
                } else
                {
                    t.sprite = healthEmpty;
                }
                health -= 2;
            }
        }
    }
}
