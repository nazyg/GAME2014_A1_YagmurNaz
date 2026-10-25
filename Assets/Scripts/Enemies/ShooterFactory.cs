// ShooterFactory.cs
// Naz Yagmur - GAME2014 Assignment 1
// makes saucer enemies

using UnityEngine;

public class ShooterFactory : EnemyFactory
{
    [SerializeField] private ShooterEnemy prefab;

    public override EnemyBase CreateEnemy(Vector3 position, Transform player, BulletPool enemyBullets)
    {
        ShooterEnemy enemy = Instantiate(prefab, position, Quaternion.identity);
        enemy.Init(player, enemyBullets);
        return enemy;
    }
}
