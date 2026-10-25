// SceneMusic.cs
// Naz Yagmur - GAME2014 Assignment 1
// tells the audio manager which song to play in this scene

using UnityEngine;

public class SceneMusic : MonoBehaviour
{
    [SerializeField] private AudioClip music;

    private void Start()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayMusic(music);
    }
}
