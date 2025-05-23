using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Unity.FPS.UI
{
    public class ASyncLoader : MonoBehaviour
    {
        [Header("Menu Screens")]
        [SerializeField] private GameObject loadingScreen;
        [SerializeField] private GameObject mainMenu;

        [Header("Slider")]
        [SerializeField] private Slider loadingSlider;

        public void LoadLevelBtn(string levelToLoad)
        {
            mainMenu.SetActive(false);
            loadingScreen.SetActive(true);

            StartCoroutine(LoadLevelASync(levelToLoad));
        }

        IEnumerator LoadLevelASync(string levelToLoad)
        {
            AsyncOperation loadOperation = SceneManager.LoadSceneAsync(levelToLoad);

            float progressValue;
            while (!loadOperation.isDone)
            {
                progressValue = Mathf.Clamp01(loadOperation.progress / 0.09f);
                loadingSlider.value = progressValue;
                yield return null;
            }
        }
    }
}
