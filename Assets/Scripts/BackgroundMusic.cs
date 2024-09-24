using System.Collections;
using UnityEngine;

public class BackgroundMusic : MonoBehaviour
{
    public AudioClip[] musicTracks;  // Array of audio tracks
    public float delayBetweenTracks = 2.0f;  // Delay between tracks in seconds
    public float fadeDuration = 1.0f;  // Duration of the fade in/out in seconds
    public float volume = 1.0f;  // Adjustable volume

    private AudioSource audioSource;
    private int currentTrackIndex = 0;
    private bool isFading = false;  // To prevent triggering new tracks during fade


    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.volume = volume;

        if (musicTracks.Length > 0)
        {
            // Clear any loaded clip and start with track at index 0
            audioSource.Stop(); 
            audioSource.clip = musicTracks[0];
            currentTrackIndex = 0;  // Ensure track index starts at 0
            audioSource.Play();
        }
        else
        {
            Debug.LogWarning("No music tracks assigned!");
        }
    }

    void Update()
    {
        // Only trigger the next track if one is not already playing, no fading, and no coroutine is running
        if (!audioSource.isPlaying && !isFading && !IsInvoking(nameof(PlayNextTrack)))
        {
            StartCoroutine(WaitAndPlayNextTrack());
        }
    }

    IEnumerator WaitAndPlayNextTrack()
    {
        // Wait for the delay between tracks
        yield return new WaitForSeconds(delayBetweenTracks);
        StartCoroutine(FadeOutAndPlayNext());
    }

    IEnumerator FadeOutAndPlayNext()
    {
        isFading = true;

        // Fade out the current track
        yield return StartCoroutine(FadeOut());

        // Play the next track
        PlayNextTrack();

        // Fade in the new track
        yield return StartCoroutine(FadeIn());

        isFading = false;
    }

    void PlayNextTrack()
    {
        // Set the next track index
        currentTrackIndex = (currentTrackIndex + 1) % musicTracks.Length;

        // Play the current track
        audioSource.clip = musicTracks[currentTrackIndex];
        audioSource.Play();
    }

    IEnumerator FadeOut()
    {
        float startVolume = audioSource.volume;

        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            audioSource.volume = Mathf.Lerp(startVolume, 0, t / fadeDuration);
            yield return null;
        }

        audioSource.volume = 0;
        audioSource.Stop();
    }

    IEnumerator FadeIn()
    {
        float targetVolume = volume;

        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            audioSource.volume = Mathf.Lerp(0, targetVolume, t / fadeDuration);
            yield return null;
        }

        audioSource.volume = targetVolume;
    }

    // Method to adjust volume, can be called from a UI slider
    public void SetVolume(float newVolume)
    {
        volume = newVolume;
        audioSource.volume = volume;  // Update volume in the audio source immediately
    }
}
