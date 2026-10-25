// Bullet.cs
// Naz Yagmur - GAME2014 Assignment 1
// bullet that moves straight. player bullets hit enemies and enemy bullets hit the player.
// when it hits something or goes off screen it goes back to the pool

using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Bullet : MonoBehaviour
{
    public enum Owner { Player, Enemy }

    [SerializeField] private Owner owner = Owner.Player;
    [SerializeField] private float speed = 14f;
    [SerializeField] private int damage = 1;

    private Vector2 direction;
    private BulletPool pool;

    // pool calls this when we shoot
    public void Launch(BulletPool ownerPool, Vector3 position, Vector2 dir)
    {
        pool = ownerPool;
        transform.position = position;
        direction = dir.normalized;
        transform.up = direction; // rotate sprite
        gameObject.SetActive(true);
    }

    private void Update()
    {
        transform.position += (Vector3)(direction * speed * Time.deltaTime);

        // off screen -> back to pool
        if (!ScreenBounds.IsInside(transform.position, 1f)) ReturnToPool();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (owner == Owner.Player)
        {
            EnemyBase enemy = other.GetComponentInParent<EnemyBase>();
            if (enemy != null && enemy.IsAlive)
            {
                enemy.TakeDamage(damage);
                ReturnToPool();
            }
        }
        else
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeHit();
                ReturnToPool();
            }
        }
    }

    private void ReturnToPool()
    {
        if (pool != null) pool.Release(this);
        else gameObject.SetActive(false);
    }
}
