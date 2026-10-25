// SceneLoader.cs
// Naz Yagmur - GAME2014 Assignment 1
// changes scenes when buttons are pressed and plays a click sound

using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    // scene names (same as build settings)
    public const string MainMenu = "MainMenu";
    public const string Instructions = "Instructions";
    public const string Play = "Play";
    public const string GameOver = "GameOver";

    [SerializeField] private AudioClip clickSfx;

    // buttons call this with the scene name
    public void LoadScene(string sceneName)
    {
        AudioManager.Play(clickSfx);
        SceneManager.LoadScene(sceneName);
    }
}
