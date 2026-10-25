// ChaserEnemy.cs
// Naz Yagmur - GAME2014 Assignment 1
// raider enemy - melee type. it moves down in a wave until the player is inside
// its detection radius, then it uses seek steering to fly at the player and dashes when close

using UnityEngine;

public class ChaserEnemy : EnemyBase
{
    [Header("Steering")]
    [SerializeField] private float chaseSpeed = 4.2f;
    [SerializeField] private float maxSteer = 7f; // how fast it turns
    [SerializeField] private float wanderAmplitude = 1.6f;

    [Header("Dash attack")]
    [SerializeField] private float dashRange = 2.2f;
    [SerializeField] private float dashMultiplier = 1.8f;

    private Vector2 velocity;
    private float wanderSeed;
    private bool isDashing;

    public override void Init(Transform playerTarget, BulletPool bulletPool)
    {
        base.Init(playerTarget, bulletPool);
        velocity = Vector2.down * speed;
        wanderSeed = Random.Range(0f, 10f);
    }

    protected override void Move()
    {
        float currentMax = isDashing ? chaseSpeed * dashMultiplier : chaseSpeed;
        Vector2 desired;

        // chase only if player is close and not behind it
        bool chasing = PlayerInDetectionRadius() && player.position.y < transform.position.y + 0.5f;
        if (chasing)
        {
            // seek: steering = desired velocity - current velocity
            desired = ((Vector2)(player.position - transform.position)).normalized * currentMax;
        }
        else
        {
            // not chasing, just go down in a wave
            float sway = Mathf.Sin(Time.time * 2.2f + wanderSeed) * wanderAmplitude;
            desired = new Vector2(sway, -speed);
        }

        Vector2 steering = Vector2.ClampMagnitude(desired - velocity, maxSteer * Time.deltaTime);
        velocity = Vector2.ClampMagnitude(velocity + steering, currentMax);
        transform.position += (Vector3)(velocity * Time.deltaTime);

        // rotate to where its going
        if (velocity.sqrMagnitude > 0.01f) transform.up = -velocity;
    }

    // melee attack - dash when really close
    protected override void Attack()
    {
        if (isDashing || !PlayerIsAlive()) return;
        if (Vector2.Distance(transform.position, player.position) <= dashRange)
        {
            isDashing = true;
            spriteRenderer.color = new Color(1f, 0.8f, 0.6f); // change color so the player knows
        }
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, dashRange);
    }
}
