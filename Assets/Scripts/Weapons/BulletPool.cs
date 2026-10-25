// BulletPool.cs
// Naz Yagmur - GAME2014 Assignment 1
// object pool (week 5). instead of Instantiate/Destroy every time we shoot,
// we make all the bullets at the start and just turn them on and off

using System.Collections.Generic;
using UnityEngine;

public class BulletPool : MonoBehaviour
{
    [SerializeField] private Bullet bulletPrefab;
    [SerializeField] private int initialSize = 30;
    [SerializeField] private bool canGrow = true; // make more if we run out

    // bullets that are not being used
    private readonly Queue<Bullet> available = new Queue<Bullet>();

    private void Awake()
    {
        // make all the bullets at the start
        for (int i = 0; i < initialSize; i++) CreateBullet();
    }

    private Bullet CreateBullet()
    {
        Bullet b = Instantiate(bulletPrefab, transform);
        b.gameObject.SetActive(false);
        available.Enqueue(b);
        return b;
    }

    // get a bullet from the pool
    public Bullet Get(Vector3 position, Vector2 direction)
    {
        if (available.Count == 0)
        {
            if (!canGrow) return null;
            CreateBullet();
        }
        Bullet bullet = available.Dequeue();
        bullet.Launch(this, position, direction);
        return bullet;
    }

    // put the bullet back
    public void Release(Bullet bullet)
    {
        if (!bullet.gameObject.activeSelf) return; // already back in pool
        bullet.gameObject.SetActive(false);
        available.Enqueue(bullet);
    }
}
