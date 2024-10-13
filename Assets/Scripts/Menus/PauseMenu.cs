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
    public GameObject buildMenuUI;
    public GameObject controlsUI;

    public String mainMenuName;

    
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Tab))
        {
            if (gameIsPaused)
            {
                Resume();
                Debug.Log("Unpausing Game");
            }
            else
            {
                Pause();

                Debug.Log("Pausing Game");
            }
        }
    }


    public void Resume()
    {
        pauseMenuUI.SetActive(false);

        if(!buildMenuUI.GetComponent<FenceBuilder>().isBuildModeActive) Time.timeScale = 1f; // can unpause as not in build mode 
        gameIsPaused = false;
    }


    void Pause()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        gameIsPaused = true;
    }

    public void ShowControls()
    {
        pauseMenuUI.SetActive(false);  
        controlsUI.SetActive(true);
    }

    public void ReturnToPauseMenu()
    {
        controlsUI.SetActive(false);   
        pauseMenuUI.SetActive(true);  
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
