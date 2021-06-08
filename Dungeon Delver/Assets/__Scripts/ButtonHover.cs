using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace __Scripts
{
    public class ButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Color hoverColor;

        private Text text;
        private Color standard;

        private void Awake()
        {
            text = GetComponent<Text>();
            standard = text.color;
        }


        public void OnPointerEnter(PointerEventData eventData)
        {
            text.color = hoverColor;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            text.color = standard;
        }
    }
}
