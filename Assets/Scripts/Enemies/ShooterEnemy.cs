// ShooterEnemy.cs
// Naz Yagmur - GAME2014 Assignment 1
// saucer enemy - ranged type. it stops near the top and moves left/right.
// it only shoots when it can see the player (line of sight with a raycast)

using UnityEngine;

public class ShooterEnemy : EnemyBase
{
    [Header("Movement")]
    [SerializeField] private float strafeSpeed = 2f;
    [SerializeField] private float stayDuration = 9f;

    [Header("Line of Sight")]
    [SerializeField] private float sightRange = 13f;
    [SerializeField] private float visionHalfAngle = 40f; // vision cone angle

    [Header("Weapon")]
    [SerializeField] private float fireCooldown = 1.6f;
    [SerializeField] private float bulletSpread = 6f; // so its not perfect aim
    [SerializeField] private AudioClip shootSfx;

    private float hoverY;
    private int strafeDir;
    private float timeOnStation;
    private float fireTimer;
    private bool hasLineOfSight;

    public override void Init(Transform playerTarget, BulletPool bulletPool)
    {
        base.Init(playerTarget, bulletPool);
        Rect r = ScreenBounds.World;
        hoverY = Random.Range(r.yMax - 5f, r.yMax - 2f);
        strafeDir = Random.value < 0.5f ? -1 : 1;
        fireTimer = Random.Range(0.6f, fireCooldown);
    }

    protected override void Move()
    {
        Vector3 pos = transform.position;
        Rect r = ScreenBounds.World;

        if (timeOnStation >= stayDuration)
        {
            pos.y -= speed * Time.deltaTime; // leave
        }
        else if (pos.y > hoverY)
        {
            pos.y = Mathf.MoveTowards(pos.y, hoverY, speed * Time.deltaTime); // fly in
        }
        else
        {
            timeOnStation += Time.deltaTime;
            pos.x += strafeDir * strafeSpeed * Time.deltaTime; // move sideways
            if (pos.x < r.xMin + 1f) strafeDir = 1;
            if (pos.x > r.xMax - 1f) strafeDir = -1;
        }
        transform.position = pos;
    }

    protected override void Attack()
    {
        fireTimer -= Time.deltaTime;
        hasLineOfSight = HasLineOfSight();
        if (fireTimer > 0f || !hasLineOfSight) return;
        if (!ScreenBounds.IsInside(transform.position, -0.5f)) return; // dont shoot from off screen

        fireTimer = fireCooldown;
        Vector2 aim = (player.position - transform.position).normalized;
        aim = Quaternion.Euler(0, 0, Random.Range(-bulletSpread, bulletSpread)) * aim;
        enemyBulletPool.Get(transform.position + Vector3.down * 0.6f, aim);
        AudioManager.Play(shootSfx, 0.5f);
    }

    private bool HasLineOfSight()
    {
        if (!PlayerIsAlive()) return false;

        Vector2 toPlayer = player.position - transform.position;
        if (toPlayer.magnitude > sightRange) return false;
        if (Vector2.Angle(Vector2.down, toPlayer) > visionHalfAngle) return false; // not in vision cone

        // raycast - first thing it hits has to be the player
        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, toPlayer.normalized, sightRange);
        foreach (RaycastHit2D hit in hits) // closest first
        {
            if (hit.collider.transform == transform) continue;
            if (hit.collider.GetComponent<Bullet>() != null) continue;
            if (hit.collider.GetComponent<Pickup>() != null) continue;
            return hit.collider.GetComponent<PlayerController>() != null; // if another enemy is in the way its false
        }
        return false;
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        if (player == null) return;
        Gizmos.color = hasLineOfSight ? Color.green : Color.gray;
        Gizmos.DrawLine(transform.position, player.position);
    }
}
