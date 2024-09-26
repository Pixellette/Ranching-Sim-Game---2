using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{

    public String newGameScene;
    public GameObject loadingScreen;
    public GameObject MainMenuScreen;
    public Slider slider;
    

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void NewGame()
    {

        // SceneManager.LoadScene(newGameScene);
        StartCoroutine(LoadAsynchronously());
    }

    public void QuitGame()
    {
        Application.Quit();
    }


    IEnumerator LoadAsynchronously ()
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(newGameScene);

        loadingScreen.SetActive(true);
        MainMenuScreen.SetActive(false);
        

        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            // Debug.Log(progress);

            slider.value = progress;
            

            yield return null;
        }
    }
}
