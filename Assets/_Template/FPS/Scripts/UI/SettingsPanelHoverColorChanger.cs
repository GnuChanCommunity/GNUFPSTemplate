using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Unity.FPS.UI
{
    [RequireComponent(typeof(Image))]
    public class HoverColorChanger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public Color hoverColor = Color.red;        // Fare üzerindeyken görünmesini istediğiniz renk
        private Color originalColor;                // Eski rengi saklayacağız

        private Image image;

        void Start()
        {
            image = GetComponent<Image>();
            if (image != null)
            {
                originalColor = image.color;
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (image != null)
            {
                image.color = hoverColor;
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (image != null)
            {
                image.color = originalColor;
            }
        }
    }
}
