using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.FPS.UI.Buttons
{
    public class CancelDialogPanel : MonoBehaviour
    {
        public GameObject dialogPanelObject;

        public void CanelDialogPanel()
        {
            this.dialogPanelObject.SetActive(false);
        }
    }
}
