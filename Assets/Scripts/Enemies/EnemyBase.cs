// EnemyBase.cs
// Naz Yagmur - GAME2014 Assignment 1
// base class for all enemies (the "product" in the factory method pattern).
// has health, score, taking damage and exploding

using System.Collections;
using UnityEngine;

public abstract class EnemyBase : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] protected int maxHealth = 1;
    [SerializeField] protected int scoreValue = 100;
    [SerializeField] protected float speed = 2.5f;

    [Header("AI")]
    [Tooltip("how close the player has to be")]
    [SerializeField] protected float detectionRadius = 4.5f;

    [Header("Death")]
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private AudioClip explosionSfx;

    protected Transform player;
    protected BulletPool enemyBulletPool;
    protected SpriteRenderer spriteRenderer;
    protected int health;

    public bool IsAlive => health > 0;

    protected virtual void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        health = maxHealth;
    }

    // factory calls this after creating the enemy
    public virtual void Init(Transform playerTarget, BulletPool bulletPool)
    {
        player = playerTarget;
        enemyBulletPool = bulletPool;
        health = maxHealth;
    }

    protected virtual void Update()
    {
        if (!IsAlive) return;
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;

        Move();
        Attack();

        // went off the bottom, delete it (no points)
        if (transform.position.y < ScreenBounds.World.yMin - 2f) Destroy(gameObject);
    }

    // each enemy type does these differently
    protected abstract void Move();
    protected abstract void Attack();

    // detection radius check
    protected bool PlayerInDetectionRadius()
    {
        return PlayerIsAlive() &&
               Vector2.Distance(transform.position, player.position) <= detectionRadius;
    }

    protected bool PlayerIsAlive() => player != null && player.gameObject.activeInHierarchy;

    public void TakeDamage(int amount)
    {
        if (!IsAlive) return;
        health -= amount;
        if (health <= 0) Die(true);
        else StartCoroutine(HitFlash());
    }

    private IEnumerator HitFlash()
    {
        spriteRenderer.color = new Color(1f, 0.45f, 0.45f);
        yield return new WaitForSeconds(0.08f);
        spriteRenderer.color = Color.white;
    }

    // only give points if the player killed it
    protected void Die(bool killedByPlayer)
    {
        health = 0;
        if (killedByPlayer && GameManager.Instance != null)
            GameManager.Instance.AddScore(scoreValue);
        AudioManager.Play(explosionSfx, 0.8f);
        if (explosionPrefab != null) Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }

    // if it crashes into the player both get hurt
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsAlive) return;
        PlayerController p = other.GetComponent<PlayerController>();
        if (p != null)
        {
            p.TakeHit();
            Die(false);
        }
    }

    // draw detection radius in scene view
    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
