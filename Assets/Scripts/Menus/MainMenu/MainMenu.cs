using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{

    public String newGameScene;
    private AudioSource backgroundMusicAudioSource;
    

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
        StopBackgroundMusic();
        SceneManager.LoadScene(newGameScene);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void StopBackgroundMusic()
    {
        // Find the GameObject named "BackgroundMusic" and get its AudioSource
        GameObject backgroundMusic = GameObject.Find("BackgroundMusic");


        if (backgroundMusic != null)
        {
            backgroundMusicAudioSource = backgroundMusic.GetComponent<AudioSource>();

            if (backgroundMusicAudioSource != null)
            {
                // Stop the music or clear the clip before loading the new scene
                backgroundMusicAudioSource.Stop();
                backgroundMusicAudioSource.clip = null;  // Clear the clip if necessary
            }
        }
    }
}
