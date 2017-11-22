using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Completed;

public class TrapScript : MonoBehaviour {

    public List<Sprite> sprungSprites = new List<Sprite>();
    public AudioClip trapSound;
    public AudioClip painSound;

    private int which = 0;
    private bool canHurt = true;

    void OnTriggerEnter2D (Collider2D col)
    {
        if (col.name == "Player" && canHurt == true)
        {
            if (which < sprungSprites.Count)
            {
                GetComponent<SpriteRenderer>().sprite = sprungSprites[which];
                which++;
            }

            if (which == 1)
            {
                AudioSource.PlayClipAtPoint(trapSound, transform.position, 100f);
            }

            AudioSource.PlayClipAtPoint(painSound, transform.position, 100f);
            StartCoroutine(col.gameObject.GetComponent<PlayerMovement>().hurtMe());
            col.GetComponent<PlayerMovement>().food -= 15;

            canHurt = false;
            Invoke("canHurtAgain", 1.5f);
        }
    }

    void canHurtAgain ()
    {
        canHurt = true;
    }
}
