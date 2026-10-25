// EnemySpawner.cs
// Naz Yagmur - GAME2014 Assignment 1
// spawns enemies at the top of the screen using the factories

using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private EnemyFactory[] factories;
    [SerializeField] private Transform player;
    [SerializeField] private BulletPool enemyBulletPool;
    [SerializeField] private float spawnInterval = 1.5f;
    [SerializeField] private int maxAlive = 6;

    private readonly List<EnemyBase> alive = new List<EnemyBase>();
    private float timer;

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;

        alive.RemoveAll(e => e == null); // remove dead ones
        timer -= Time.deltaTime;
        if (timer > 0f) return;
        timer = spawnInterval;

        if (alive.Count >= maxAlive) return;

        // random x position at the top
        Rect r = ScreenBounds.World;
        Vector3 pos = new Vector3(Random.Range(r.xMin + 1f, r.xMax - 1f), r.yMax + 1f, 0f);
        alive.Add(PickFactory().CreateEnemy(pos, player, enemyBulletPool));
    }

    // pick a random factory (weighted)
    private EnemyFactory PickFactory()
    {
        float total = 0f;
        foreach (EnemyFactory f in factories) total += f.SpawnWeight;

        float roll = Random.Range(0f, total);
        foreach (EnemyFactory f in factories)
        {
            roll -= f.SpawnWeight;
            if (roll <= 0f) return f;
        }
        return factories[0];
    }
}
