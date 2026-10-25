// GameOverUI.cs
// Naz Yagmur - GAME2014 Assignment 1
// shows the final score on the game over screen

using UnityEngine;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private Text finalScoreText;

    private void Start()
    {
        finalScoreText.text = GameSession.LastScore.ToString("000000");
    }
}
