// HUDController.cs
// Naz Yagmur - GAME2014 Assignment 1
// shows the score and the lives (hearts) in the play scene

using UnityEngine;
using UnityEngine.UI;

public class HUDController : MonoBehaviour
{
    [SerializeField] private Text scoreText;
    [SerializeField] private Image[] hearts; // one heart for each life
    [SerializeField] private Sprite heartFull;
    [SerializeField] private Sprite heartEmpty;

    private GameManager gm;

    private void Start()
    {
        gm = GameManager.Instance;
        gm.ScoreChanged += OnScoreChanged;
        gm.LivesChanged += OnLivesChanged;

        OnScoreChanged(gm.Score);
        OnLivesChanged(gm.Lives);
    }

    private void OnDestroy()
    {
        if (gm == null) return;
        gm.ScoreChanged -= OnScoreChanged;
        gm.LivesChanged -= OnLivesChanged;
    }

    private void OnScoreChanged(int score)
    {
        scoreText.text = score.ToString("000000");
    }

    private void OnLivesChanged(int lives)
    {
        for (int i = 0; i < hearts.Length; i++)
            hearts[i].sprite = i < lives ? heartFull : heartEmpty;
    }
}
