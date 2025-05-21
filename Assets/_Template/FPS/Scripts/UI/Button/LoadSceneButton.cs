using Unity.FPS.Game;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace Unity.FPS.UI.Buttons
{
    public class LoadSceneButton : MonoBehaviour
    {
        public string SceneName = "";
        public ASyncLoader asyncLoader;

        void Update()
        {
            if (EventSystem.current.currentSelectedGameObject == gameObject
                && Input.GetButtonDown(GameConstants.k_ButtonNameSubmit))
            {
                LoadTargetScene();
            }
        }

        public void LoadTargetScene()
        {
            // If async loader assigned then use it else use standart load screene loagic
            if (asyncLoader != null)
                asyncLoader.LoadLevelBtn(SceneName);
            else
                SceneManager.LoadScene(SceneName);
        }
    }
}