using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public static bool gameIsPaused = false;
    public GameObject pauseMenuUI;

    public String mainMenuName;

    
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Tab))
        {
            if (gameIsPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }


    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        gameIsPaused = false;
    }


    void Pause()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        gameIsPaused = true;
    }


    public void LoadMenu()
    {
        Debug.Log("Loading Main Menu");
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuName);
    }


    public void QuitGame()
    {
        Debug.Log("Quitting Game");
        Application.Quit();
    }
}
