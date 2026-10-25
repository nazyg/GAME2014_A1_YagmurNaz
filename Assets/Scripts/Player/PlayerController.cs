// PlayerController.cs
// Naz Yagmur - GAME2014 Assignment 1
// player movement with touch (new input system). you hold and drag to move the ship,
// it shoots while your finger is on the screen

using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float maxSpeed = 18f;
    [SerializeField] private float edgePaddingX = 0.6f;
    [SerializeField] private float edgePaddingY = 0.9f;

    [Header("Weapon")]
    [SerializeField] private BulletPool bulletPool;
    [SerializeField] private Transform muzzle;
    [SerializeField] private float fireRate = 7f; // shots per second
    [SerializeField] private AudioClip shootSfx;

    [Header("Damage")]
    [SerializeField] private float invincibleTime = 1.5f;
    [SerializeField] private AudioClip hitSfx;
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private AudioClip explosionSfx;

    private Camera cam;
    private SpriteRenderer spriteRenderer;
    private Vector3 targetPosition;
    private Vector3 dragOffset;
    private float fireTimer;
    private bool isInvincible;
    private bool isDead;

    private void Awake()
    {
        cam = Camera.main;
        spriteRenderer = GetComponent<SpriteRenderer>();
        targetPosition = transform.position;
    }

    private void Update()
    {
        if (isDead) return;

        bool touching = HandleTouch();

        // move to the target position
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, maxSpeed * Time.deltaTime);
        transform.position = ScreenBounds.Clamp(transform.position, edgePaddingX, edgePaddingY);

        if (touching) HandleShooting();
    }

    // touch input (mouse works too in editor). returns true if touching
    private bool HandleTouch()
    {
        Pointer pointer = Pointer.current;
        if (pointer == null || !pointer.press.isPressed) return false;

        Vector3 fingerWorld = cam.ScreenToWorldPoint(pointer.position.ReadValue());
        fingerWorld.z = 0f;

        // save the offset when touch starts so the finger doesnt cover the ship
        if (pointer.press.wasPressedThisFrame)
            dragOffset = transform.position - fingerWorld;

        targetPosition = ScreenBounds.Clamp(fingerWorld + dragOffset, edgePaddingX, edgePaddingY);
        return true;
    }

    private void HandleShooting()
    {
        fireTimer -= Time.deltaTime;
        if (fireTimer > 0f) return;
        fireTimer = 1f / fireRate;

        bulletPool.Get(muzzle.position, Vector2.up);
        AudioManager.Play(shootSfx, 0.35f);
    }

    // called when enemy bullet or enemy hits the player
    public void TakeHit()
    {
        if (isInvincible || isDead) return;
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;

        GameManager.Instance.LoseLife();

        if (GameManager.Instance.Lives <= 0)
        {
            Die();
            return;
        }
        AudioManager.Play(hitSfx);
        StartCoroutine(InvincibilityRoutine());
    }

    private IEnumerator InvincibilityRoutine()
    {
        isInvincible = true;
        float t = 0f;
        while (t < invincibleTime)
        {
            spriteRenderer.enabled = !spriteRenderer.enabled; // blink
            yield return new WaitForSeconds(0.08f);
            t += 0.08f;
        }
        spriteRenderer.enabled = true;
        isInvincible = false;
    }

    private void Die()
    {
        isDead = true;
        AudioManager.Play(explosionSfx);
        if (explosionPrefab != null)
        {
            GameObject fx = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            fx.transform.localScale = Vector3.one * 2f; // bigger explosion
        }
        gameObject.SetActive(false);
    }
}
