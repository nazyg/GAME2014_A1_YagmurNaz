// UISpriteAnimator.cs
// Naz Yagmur - GAME2014 Assignment 1
// simple frame animation for ui images

using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class UISpriteAnimator : MonoBehaviour
{
    [SerializeField] private Sprite[] frames;
    [SerializeField] private float framesPerSecond = 8f;

    private Image image;

    private void Awake()
    {
        image = GetComponent<Image>();
    }

    private void Update()
    {
        if (frames == null || frames.Length == 0) return;
        int index = (int)(Time.unscaledTime * framesPerSecond) % frames.Length;
        image.sprite = frames[index];
    }
}
