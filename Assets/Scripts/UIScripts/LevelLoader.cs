using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    [Header("Transition Reference and Duration")]
    [SerializeField] private Animator transition;
    [SerializeField] private float transitionTime = 1f;

    // Starts the coroutine (as the coroutine is private) here to start the
    // transition.
    public void LoadNextLevelAndTransition()
    {
        // Uses the next scene's build index as the argument for the coroutine.
        StartCoroutine(LoadLevel(SceneManager.GetActiveScene().buildIndex + 1));
    }

    public float getTransitionTime()
    {
        return transitionTime;
    }

    // Starts the animation, waits a couple of seconds, then actually goes to
    // the next scene.
    private IEnumerator LoadLevel(int levelIndex)
    {
        transition.SetTrigger("Start");

        yield return new WaitForSecondsRealtime(transitionTime);

        SceneManager.LoadScene(levelIndex);
    }

    // Mainly utilized for PauseMenu, just calls RestartLevel();
    public void StartRestartCoroutine()
    {
        StartCoroutine(RestartLevel());
    }

    // Does the animation, minus the load next level part, of course.
    public IEnumerator RestartLevel()
    {
        transition.SetTrigger("Start");

        yield return new WaitForSecondsRealtime(transitionTime);

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
