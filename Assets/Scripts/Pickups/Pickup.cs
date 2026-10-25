// Pickup.cs
// Naz Yagmur - GAME2014 Assignment 1
// loot that falls down. the player gets points when it collects it

using UnityEngine;

public class Pickup : MonoBehaviour
{
    [SerializeField] private int points = 50;
    [SerializeField] private float fallSpeed = 1.8f;
    [SerializeField] private AudioClip collectSfx;

    private void Update()
    {
        transform.position += Vector3.down * fallSpeed * Time.deltaTime;

        // off the bottom of the screen
        if (transform.position.y < ScreenBounds.World.yMin - 1f) Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<PlayerController>() == null) return;
        if (GameManager.Instance == null || GameManager.Instance.IsGameOver) return;

        GameManager.Instance.AddScore(points);
        AudioManager.Play(collectSfx);
        Destroy(gameObject);
    }
}
