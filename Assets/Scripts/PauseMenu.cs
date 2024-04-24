using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using System;

public class PauseMenu : MonoBehaviour
{
    [Header("Screen References")]
    [SerializeField] GameObject pauseMenuScreen;
    [SerializeField] GameObject HowToPlayScreen;

    [Header("Transition Reference")]
    [SerializeField] private LevelLoader transitionObject;

    bool isPaused;
    bool canPause;

    // Main Menu Options
    void Start()
    {
        Time.timeScale = 1f;

        pauseMenuScreen.SetActive(false);
        HowToPlayScreen.SetActive(false);

        isPaused = false;
        canPause = true;
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if(canPause == true)
            {
                if (isPaused == true)
                {
                    ReturnToGame();
                }
                else
                {
                    PauseTheGame();
                }
            }
            else
            {
                Debug.Log("PLEASE STOP TRYING TO BREAK THE MENU!");
            }
        }
    }

    public void PauseTheGame()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        pauseMenuScreen.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;

        Debug.Log("Destruction halted, Jev is eepy");

    }

    public void ReturnToGame()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        pauseMenuScreen.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;

        Debug.Log("JEV IS NO LONGER EEPY, KILL KILL KILL!");
    }

    public void ReturnToMenu()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
        Debug.Log("Jev is strategizing his next attack");
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Uh oh, it works here too");
    }

    // HowToPlay Options
    public void HowToPlay()
    {
        Debug.Log("Haha, this guy STILL needs instructions");
        pauseMenuScreen.SetActive(false);
        HowToPlayScreen.SetActive(true);
        canPause = false;

    }

    public void HowToBack()
    {
        Debug.Log("Good, you can read");
        pauseMenuScreen.SetActive(true);
        HowToPlayScreen.SetActive(false);
        canPause = true;
    }

    public void ReloadScene()
    {
        Debug.Log("Hey i work!");
        transitionObject.StartRestartCoroutine();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        Debug.Log("Jev is born anew, REJOICE AND DESTROY!!!");
    }
}
