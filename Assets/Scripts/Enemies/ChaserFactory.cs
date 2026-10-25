// ChaserFactory.cs
// Naz Yagmur - GAME2014 Assignment 1
// makes raider enemies

using UnityEngine;

public class ChaserFactory : EnemyFactory
{
    [SerializeField] private ChaserEnemy prefab;

    public override EnemyBase CreateEnemy(Vector3 position, Transform player, BulletPool enemyBullets)
    {
        ChaserEnemy enemy = Instantiate(prefab, position, Quaternion.identity);
        enemy.Init(player, enemyBullets);
        return enemy;
    }
}
