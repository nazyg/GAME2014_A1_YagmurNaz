// UIFloat.cs
// Naz Yagmur - GAME2014 Assignment 1
// moves a ui element back and forth

using UnityEngine;

public class UIFloat : MonoBehaviour
{
    [SerializeField] private Vector2 distance = new Vector2(0f, 20f);
    [SerializeField] private float speed = 2f;
    [SerializeField] private float phase = 0f;

    private RectTransform rect;
    private Vector2 start;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        start = rect.anchoredPosition;
    }

    private void Update()
    {
        rect.anchoredPosition = start + distance * Mathf.Sin(Time.unscaledTime * speed + phase);
    }
}
