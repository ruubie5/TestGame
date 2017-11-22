using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChestScript : MonoBehaviour {

    public GameObject[] possibleLoot;
    public Sprite openedImage;
    public AudioClip openSound;

    private bool canOpen = true;

    public void open()
    {
        if (canOpen == true)
        {
            canOpen = false;
            AudioSource.PlayClipAtPoint(openSound, transform.position);
            GetComponent<SpriteRenderer>().sprite = openedImage;
            GetComponent<BoxCollider2D>().offset = new Vector2(GetComponent<BoxCollider2D>().offset.x, 0.42f);

            int dropAmount = Random.Range(2, 5);

            for (int i = dropAmount; i > 0; i--)
            {
                Instantiate(possibleLoot[Random.Range(0, possibleLoot.Length)], transform.position - Vector3.up, Quaternion.identity);
            }
        }
    }
}
