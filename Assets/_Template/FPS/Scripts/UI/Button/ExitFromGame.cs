using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.FPS.UI.Buttons
{
    public class ExitFromGame : MonoBehaviour
    {
        public void ExitGameButton()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
