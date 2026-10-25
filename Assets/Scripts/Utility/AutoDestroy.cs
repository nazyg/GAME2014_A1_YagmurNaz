// AutoDestroy.cs
// Naz Yagmur - GAME2014 Assignment 1
// destroys the object after some time (used for explosions)

using UnityEngine;

public class AutoDestroy : MonoBehaviour
{
    [SerializeField] private float lifetime = 0.5f;

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }
}
