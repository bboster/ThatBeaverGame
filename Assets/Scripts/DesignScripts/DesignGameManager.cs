/*****************************************************************************
 * Author: Brad Dixon
 * File: DesignGameManager
 * Date: 4/21/2024
 * Brief: A remake of a previous script that allows for debugging in a scene
 * **************************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DesignGameManager : MonoBehaviour
{
    //Allows any scene to be reset
    [SerializeField] private string SceneName;

    /// <summary>
    /// Restarts the scene whenever r is pressed
    /// </summary>
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
            UnityEngine.SceneManagement.SceneManager.LoadScene(SceneName);
    }
}
