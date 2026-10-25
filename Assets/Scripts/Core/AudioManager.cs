// AudioManager.cs
// Naz Yagmur - GAME2014 Assignment 1
// plays the music and the sound effects. it stays alive between scenes so the music doesnt stop

using UnityEngine;

public class AudioManager : MonoBehaviour
{
    // singleton
    public static AudioManager Instance { get; private set; }

    [Header("Sources")]
    [SerializeField] private AudioSource musicSource; // music
    [SerializeField] private AudioSource sfxSource; // sound effects

    [Header("Volumes")]
    [SerializeField, Range(0f, 1f)] private float musicVolume = 0.45f;
    [SerializeField, Range(0f, 1f)] private float sfxVolume = 0.9f;

    private void Awake()
    {
        // every scene has one so i can test any scene,
        // if there is already one we delete the new one
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        musicSource.loop = true;
        musicSource.volume = musicVolume;
        sfxSource.volume = sfxVolume;
    }

    // play music (if its already playing dont restart it)
    public void PlayMusic(AudioClip clip)
    {
        if (clip == null) return;
        if (musicSource.clip == clip && musicSource.isPlaying) return;
        musicSource.clip = clip;
        musicSource.Play();
    }

    // play a sound effect
    public void PlaySFX(AudioClip clip, float volumeScale = 1f)
    {
        if (clip == null) return;
        sfxSource.PlayOneShot(clip, volumeScale);
    }

    // shortcut so other scripts can just call AudioManager.Play(clip)
    public static void Play(AudioClip clip, float volumeScale = 1f)
    {
        if (Instance != null) Instance.PlaySFX(clip, volumeScale);
    }
}
