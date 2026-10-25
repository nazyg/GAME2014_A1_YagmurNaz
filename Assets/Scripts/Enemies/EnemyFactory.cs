// EnemyFactory.cs
// Naz Yagmur - GAME2014 Assignment 1
// factory method pattern (week 6). this is the creator class.
// ChaserFactory and ShooterFactory override CreateEnemy and EnemySpawner uses them

using UnityEngine;

public abstract class EnemyFactory : MonoBehaviour
{
    [Tooltip("bigger = spawns more")]
    [SerializeField] private float spawnWeight = 1f;

    public float SpawnWeight => spawnWeight;

    // the factory method
    public abstract EnemyBase CreateEnemy(Vector3 position, Transform player, BulletPool enemyBullets);
}
