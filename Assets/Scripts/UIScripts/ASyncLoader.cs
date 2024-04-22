using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ASyncLoader : MonoBehaviour
{
    [Header("Menu Screens")]
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private GameObject mainMenuScreen;

    [Header("Loading Bar / Slider")]
    [SerializeField] private Slider loadingBar;

    [Header("Transition Reference")]
    [SerializeField] private LevelLoader transitionObject;

    public void PreloadNextScene(int levelIndex)
    {
        mainMenuScreen.SetActive(false);
        loadingScreen.SetActive(true);

        // Starts the async loading.
        StartCoroutine(LoadLevelASync(levelIndex));
    }

    IEnumerator LoadLevelASync(int levelIndex)
    {
        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(levelIndex);

        // While it's not done loading...
        while (!loadOperation.isDone)
        {
            // Grab the loading progress...
            float progressValue = Mathf.Clamp01(loadOperation.progress / 0.9f);
            // Update the loadingBar gameobject in accordance to that value...
            loadingBar.value = progressValue;
            yield return null;
        }

        transitionObject.LoadNextLevelAndTransition();
    }
}
