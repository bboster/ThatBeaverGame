using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using System;

public class MainMenu : MonoBehaviour
{
    [SerializeField] GameObject mainMenuScreen;
    [SerializeField] GameObject CreditsScreen;
    [SerializeField] GameObject HowToPlayScreen;

    // Main Menu Options
    void Start()
    {

        Time.timeScale = 1f;

        mainMenuScreen.SetActive(true);
        CreditsScreen.SetActive(false);
        HowToPlayScreen.SetActive(false);
    }

    public void PlayGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Uh oh, it works");
    }

    // HowToPlay Options
    public void HowToPlay()
    {
        Debug.Log("Haha, this guy needs instructions");
        mainMenuScreen.SetActive(false);
        HowToPlayScreen.SetActive(true);

    }

    public void HowToBack()
    {
        Debug.Log("Good, you can read");
        mainMenuScreen.SetActive(true);
        HowToPlayScreen.SetActive(false);
    }

    // Credits options
    public void Credits()
    {
        Debug.Log("Appreciate us");
        mainMenuScreen.SetActive(false);
        CreditsScreen.SetActive(true);
    }

    public void CreditsBack()
    {
        Debug.Log("Appreciate our game now");
        mainMenuScreen.SetActive(true);
        CreditsScreen.SetActive(false);
    }
}
