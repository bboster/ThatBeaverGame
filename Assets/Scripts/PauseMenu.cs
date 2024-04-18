using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using System;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] GameObject pauseMenuScreen;
    [SerializeField] GameObject HowToPlayScreen;

    bool isPaused;

    // Main Menu Options
    void Start()
    {
        pauseMenuScreen.SetActive(false);
        HowToPlayScreen.SetActive(false);

        isPaused = false;
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
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
    }

    public void PauseTheGame()
    {
        pauseMenuScreen.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;

    }

    public void ReturnToGame()
    {
        pauseMenuScreen.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
    }

    public void ReturnToMenu()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
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

    }

    public void HowToBack()
    {
        Debug.Log("Good, you can read");
        pauseMenuScreen.SetActive(true);
        HowToPlayScreen.SetActive(false);
    }
}
