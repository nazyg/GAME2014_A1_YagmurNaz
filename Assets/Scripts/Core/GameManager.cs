// GameManager.cs
// Naz Yagmur - GAME2014 Assignment 1
// keeps track of score and lives in the play scene. also handles game over

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private int startingLives = 3;
    [SerializeField] private AudioClip gameOverSfx;
    [SerializeField] private float delayBeforeGameOverScreen = 1.5f;

    public int Score { get; private set; }
    public int Lives { get; private set; }
    public bool IsGameOver { get; private set; }

    // events for the hud
    public event Action<int> ScoreChanged;
    public event Action<int> LivesChanged;

    private void Awake()
    {
        Instance = this;
        Lives = startingLives;
        Score = 0;
    }

    public void AddScore(int amount)
    {
        if (IsGameOver) return;
        Score += amount;
        ScoreChanged?.Invoke(Score);
    }

    public void LoseLife()
    {
        if (IsGameOver) return;
        Lives = Mathf.Max(0, Lives - 1);
        LivesChanged?.Invoke(Lives);
        if (Lives == 0) StartCoroutine(GameOverRoutine());
    }

    private IEnumerator GameOverRoutine()
    {
        IsGameOver = true;
        GameSession.LastScore = Score;
        AudioManager.Play(gameOverSfx);
        // wait a bit so you can see the explosion
        yield return new WaitForSeconds(delayBeforeGameOverScreen);
        SceneManager.LoadScene(SceneLoader.GameOver);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}
