using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HighlightObject : MonoBehaviour {

    public bool isActive;
    private Sprite normalSprite;
    public Sprite highlightSprite;

    private SpriteRenderer spriteRenderer;


    private void Start()
    {        
        spriteRenderer = this.GetComponent<SpriteRenderer>();
        normalSprite = spriteRenderer.sprite;
    }

    void Update ()
    {       
        if (!isActive)
        {
            spriteRenderer.sprite = normalSprite;
        }


        if (isActive)
        {
            spriteRenderer.sprite = highlightSprite;
        }

        isActive = false;
    }
}
