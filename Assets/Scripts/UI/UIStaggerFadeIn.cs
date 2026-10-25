// UIStaggerFadeIn.cs
// Naz Yagmur - GAME2014 Assignment 1
// animated instructions - shows the rows one by one with a fade and slide

using System.Collections;
using UnityEngine;

public class UIStaggerFadeIn : MonoBehaviour
{
    [SerializeField] private float delayBetween = 0.25f;
    [SerializeField] private float duration = 0.35f;
    [SerializeField] private float slideDistance = 120f;

    private IEnumerator Start()
    {
        // hide all rows first
        CanvasGroup[] rows = new CanvasGroup[transform.childCount];
        for (int i = 0; i < rows.Length; i++)
        {
            Transform child = transform.GetChild(i);
            rows[i] = child.GetComponent<CanvasGroup>();
            if (rows[i] == null) rows[i] = child.gameObject.AddComponent<CanvasGroup>();
            rows[i].alpha = 0f;
        }

        yield return new WaitForSeconds(0.3f);
        foreach (CanvasGroup row in rows)
        {
            StartCoroutine(Reveal(row));
            yield return new WaitForSeconds(delayBetween);
        }
    }

    private IEnumerator Reveal(CanvasGroup group)
    {
        RectTransform rect = (RectTransform)group.transform;
        Vector2 end = rect.anchoredPosition;
        Vector2 start = end + Vector2.right * slideDistance;
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float k = 1f - Mathf.Pow(1f - Mathf.Clamp01(t / duration), 3f); // ease out
            group.alpha = k;
            rect.anchoredPosition = Vector2.Lerp(start, end, k);
            yield return null;
        }
        group.alpha = 1f;
        rect.anchoredPosition = end;
    }
}
