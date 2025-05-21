using UnityEngine;

namespace Unity.FPS.UI.Buttons
{
    public class EnableObjectButton : MonoBehaviour
    {
        [SerializeField] private GameObject targetObject;

        public void SwitchObjectState()
        {
            if (targetObject != null)
            {
                targetObject.SetActive(!targetObject.activeSelf);
            }
        }

        public void EnableObject()
        {
            if (targetObject != null)
            {
                targetObject.SetActive(true);
            }
        }

        public void DisableObject()
        {
            if (targetObject != null)
            {
                targetObject.SetActive(false);
            }
        }
    }
}
