// PickupSpawner.cs
// Naz Yagmur - GAME2014 Assignment 1
// spawns a random pickup at a random place every few seconds

using UnityEngine;

public class PickupSpawner : MonoBehaviour
{
    [SerializeField] private Pickup[] pickups;
    [SerializeField] private float minInterval = 3f;
    [SerializeField] private float maxInterval = 6f;

    private float timer;

    private void Start()
    {
        timer = Random.Range(minInterval, maxInterval);
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;

        timer -= Time.deltaTime;
        if (timer > 0f) return;
        timer = Random.Range(minInterval, maxInterval);

        // random pickup at a random x position at the top
        Pickup prefab = pickups[Random.Range(0, pickups.Length)];
        Rect r = ScreenBounds.World;
        Vector3 pos = new Vector3(Random.Range(r.xMin + 1f, r.xMax - 1f), r.yMax + 0.8f, 0f);
        Instantiate(prefab, pos, Quaternion.identity);
    }
}
