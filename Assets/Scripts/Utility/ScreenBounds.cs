// ScreenBounds.cs
// Naz Yagmur - GAME2014 Assignment 1
// helper to get the edges of the screen in world units (so it works on different phone sizes)

using UnityEngine;

public static class ScreenBounds
{
    // screen area in world units
    public static Rect World
    {
        get
        {
            Camera cam = Camera.main;
            if (cam == null) return new Rect(-4.5f, -8f, 9f, 16f);
            float halfH = cam.orthographicSize;
            float halfW = halfH * cam.aspect;
            Vector3 c = cam.transform.position;
            return new Rect(c.x - halfW, c.y - halfH, halfW * 2f, halfH * 2f);
        }
    }

    // is the point on screen
    public static bool IsInside(Vector3 point, float margin = 0f)
    {
        Rect r = World;
        return point.x > r.xMin - margin && point.x < r.xMax + margin &&
               point.y > r.yMin - margin && point.y < r.yMax + margin;
    }

    // keep position inside the screen
    public static Vector3 Clamp(Vector3 p, float padX, float padY)
    {
        Rect r = World;
        p.x = Mathf.Clamp(p.x, r.xMin + padX, r.xMax - padX);
        p.y = Mathf.Clamp(p.y, r.yMin + padY, r.yMax - padY);
        return p;
    }
}
