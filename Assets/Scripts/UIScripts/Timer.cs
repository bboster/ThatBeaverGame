using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Timer : MonoBehaviour
{
    // the amount of time.
    [SerializeField] private float timer = 300;

    // UI stuff
    public TMP_Text timerUI;
    public TMP_Text minutes;
    public TMP_Text seconds;

    // bool that starts the timer.
    [SerializeField] private bool timerCanStart;

    // anticipating there being a game over screen.
    [SerializeField] private GameObject GameOverScreen;

    private void Start()
    {
        timerCanStart = true;
    }

    private void Update()
    {
        if(timer > 0 && timerCanStart)
        {
            timer -= Time.deltaTime;
        }
        else if(timer <= 0 && timerCanStart) // we can switch this to being another scene later if preferred.
        {
            GameOverScreen.SetActive(true);
            Time.timeScale = 0.0f;
        }

        float minute = Mathf.FloorToInt(timer / 60);
        float second = Mathf.FloorToInt(timer % 60);

        if(second < 10f)
        {
            seconds.text = ":0" + second.ToString();
        }
        else
        {
            seconds.text = ":" + second.ToString();
        }

        minutes.text = minute.ToString();
        timerUI.text = string.Format("{0,00}:{1,00}", minute, second);
    }
}
