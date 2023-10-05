using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Prop : MonoBehaviour {

    public Vector3Int position;
    public Sprite sprite;
    public Sprite shadow;

    [Header("Bottom Shadow Settings")]
    public Vector2 size;
    public Sprite shape;
    public Color color;

    void Start() {
        GetComponent<SpriteRenderer>().sprite = sprite;
        if (shadow != null) {
            GameObject shadowObject = new GameObject("Shadow");
            shadowObject.transform.SetParent(transform);
            shadowObject.transform.position = transform.position;
            shadowObject.AddComponent<SpriteRenderer>().sprite = shadow;
            shadowObject.GetComponent<SpriteRenderer>().spriteSortPoint = SpriteSortPoint.Pivot;
        }
        
        GameObject bottomShadow = new GameObject("BottomShadow");
        SpriteRenderer spriteRenderer = bottomShadow.AddComponent<SpriteRenderer>();
        spriteRenderer.color = color;
        spriteRenderer.sprite = shape;
        spriteRenderer.sortingLayerName = "Shadow";
        bottomShadow.transform.localScale = size;
        bottomShadow.transform.SetParent(transform);
        bottomShadow.transform.position = transform.position;
    }
}
