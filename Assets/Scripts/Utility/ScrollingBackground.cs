// ScrollingBackground.cs
// Naz Yagmur - GAME2014 Assignment 1
// moves 2 background tiles down and loops them so the space never ends

using UnityEngine;

public class ScrollingBackground : MonoBehaviour
{
    [SerializeField] private Transform[] tiles = new Transform[2];
    [SerializeField] private float speed = 1.2f;

    private float tileHeight;

    private void Start()
    {
        tileHeight = tiles[0].GetComponent<SpriteRenderer>().bounds.size.y;
    }

    private void Update()
    {
        foreach (Transform tile in tiles)
        {
            tile.position += Vector3.down * speed * Time.deltaTime;
            // when it goes too low move it back on top
            if (tile.localPosition.y <= -tileHeight)
                tile.localPosition += Vector3.up * tileHeight * 2f;
        }
    }
}
