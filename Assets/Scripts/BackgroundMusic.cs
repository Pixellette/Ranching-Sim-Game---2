using System.Collections;
using UnityEngine;

public class BackgroundMusic : MonoBehaviour
{
    public AudioClip[] musicTracks;  // Array of audio tracks
    public float delayBetweenTracks = 2.0f;  // Delay between tracks in seconds

    private AudioSource audioSource;
    private int currentTrackIndex = 0;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        
        // Check if there are tracks available
        if (musicTracks.Length > 0)
        {
            PlayNextTrack();
        }
        else
        {
            Debug.LogWarning("No music tracks assigned!");
        }
    }

    void Update()
    {
        // Check if the current track has finished playing
        if (!audioSource.isPlaying && !IsInvoking(nameof(PlayNextTrack)))
        {
            StartCoroutine(WaitAndPlayNextTrack());
        }
    }

    IEnumerator WaitAndPlayNextTrack()
    {
        // Wait for the delay before playing the next track
        yield return new WaitForSeconds(delayBetweenTracks);
        PlayNextTrack();
    }

    void PlayNextTrack()
    {
        // Play the current track
        audioSource.clip = musicTracks[currentTrackIndex];
        audioSource.Play();

        // Move to the next track index
        currentTrackIndex = (currentTrackIndex + 1) % musicTracks.Length;
    }
}
