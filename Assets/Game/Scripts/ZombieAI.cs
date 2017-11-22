using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ZombieAI : MonoBehaviour
{
    public float health = 100;
    public float food = 0;
    public int pointsPerFood = 10;              //Number of points to add to player food points when picking up a food object.
    public int pointsPerSoda = 20;				//Number of points to add to player food points when picking up a soda object.
    public int playerDamage;                            //The amount of food points to subtract from the player when attacking.
    public AudioClip attackSound;                      //First of two audio clips to play when attacking the player.
    public AudioClip painSound;
    public AudioClip AIPainSound;
    public AudioClip deathSound;
    public GameObject[] lootList;
    public GameObject skeletonHead;

    public float enemyViewDistance = 10f;
    public float enemyMoveSpeed = 0.1f;

    private CircleCollider2D circleCollider; 		            //The BoxCollider2D component attached to this object.

    private Animator animator;                          //Variable of type Animator to store a reference to the enemy's Animator component.
    private Transform target;                           //Transform to attempt to move toward each turn.
                                                        // Use this for initialization
    private Slider healthSlider;
    private Vector3 direction;
    private int steps;
    private int targetSteps;
    private bool targetCompleted = true;
    private Vector2 targetDir;
    private bool canAttack = true;

    void Start()
    {
        healthSlider = GetComponentInChildren<Slider>();

        //Get and store a reference to the attached Animator component.
        animator = GetComponent<Animator>();

        //Get a component reference to this object's BoxCollider2D
        circleCollider = GetComponent<CircleCollider2D>();

        //Find the Player GameObject using it's tag and store a reference to its transform component.
        target = GameObject.FindGameObjectWithTag("Player").transform;

    }

    // Update is called once per frame
    void Update()
    {
        healthSlider.value = health;

        //Looks if the player has food above 200 and a weapon if so the zombi its view distance will increase.
        if(target.GetComponent<PlayerMovement>().food > 200 && target.GetComponent<PlayerMovement>().equippedWeapon !=null)
        {
            enemyViewDistance = target.GetComponent<PlayerMovement>().food/10;
        }
        else
        {
            enemyViewDistance = 6;
        }
        if(health <= 0)
        {
            AudioSource.PlayClipAtPoint(deathSound, transform.position);
            int which = Random.Range(0, 11);
            Instantiate(skeletonHead, transform.position, Quaternion.identity);

            if (which >= 5)
            {
                Instantiate(lootList[Random.Range(0, lootList.Length)], transform.position, Quaternion.identity);
            }

            Destroy(gameObject);
        }

        target = GameObject.FindGameObjectWithTag("Player").transform;

        lookForPlayer();

        if (Vector3.Distance(this.transform.position, target.transform.position) >= 1)
        {
            MoveRandom();
        }
       
        this.transform.Translate(targetDir * Time.deltaTime);

        RaycastHit2D closeHit = Physics2D.Raycast(transform.position, targetDir, 0.5f);        
        
        if (closeHit.collider != null)
        {
           if (closeHit.collider.tag == "OuterWall" || closeHit.collider.tag == "Wall")
            {
                targetCompleted = true;
                MoveRandom();
            }
        }
    }
    void lookForPlayer()
    {
        direction = target.transform.position - this.transform.position;

        //Disable the boxCollider so that linecast doesn't hit this object's own collider.
        circleCollider.enabled = false;

        //Shoot a raycast towards the target to see if there is a clear path between them.
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, enemyViewDistance);

        //Re-enable boxCollider after linecast
        circleCollider.enabled = true;

        if (hit.collider != null)
        {
            if(hit.collider.tag == "Player")
            {
                if (Vector3.Distance (this.transform.position, target.transform.position) >= 1f)
                {
                    transform.position = Vector2.MoveTowards(transform.position, target.transform.position, enemyMoveSpeed * Time.deltaTime);
                }

                if (Vector3.Distance(this.transform.position,target.transform.position) <= 1f && canAttack)
                {
                    AudioSource.PlayClipAtPoint(attackSound, transform.position);
                    AudioSource.PlayClipAtPoint(painSound, transform.position);
                    hit.collider.gameObject.GetComponent<Rigidbody2D>().AddForce((hit.collider.gameObject.transform.position - transform.position) * 500);
                    hit.collider.gameObject.GetComponent<PlayerMovement>().Invoke("removeKnockback", 0.1f);
                    hit.collider.gameObject.GetComponent<PlayerMovement>().animator.SetTrigger("playerHit");
                    animator.SetTrigger("enemyAttack");
                    target.GetComponent<PlayerMovement>().food -= 15;
                    StartCoroutine(damageCD());
                    StartCoroutine(hit.collider.gameObject.GetComponent<PlayerMovement>().hurtMe());
                }
            }
            if(hit.collider.tag == "Food"|| hit.collider.tag == "Soda")
            {
                if (Vector3.Distance(this.transform.position, hit.collider.gameObject.transform.position) <= 1.5f)
                {
                    Destroy(hit.collider.gameObject);
                    food += pointsPerFood;
                    IncreaseSize();
                }               
            }
        }
    }
    public void resetKnockback()
    {
        GetComponent<Rigidbody2D>().velocity = Vector2.zero;
    }
    public void IncreaseSize()
    {
        this.transform.localScale += new Vector3(food / 1000f, 0.01f, 0f);
        enemyMoveSpeed -= food / 100;
    }
    private void MoveRandom()
    {
        if (targetCompleted == false)
        {
            steps++;
            if (steps >= targetSteps)
            {
                targetCompleted = true;
            }
        }

        if (targetCompleted == true)
        {
            targetSteps = Random.Range(20, 90);
            switch (Random.Range(0, 4))
            {
                case 0: targetDir = Vector2.up;
                    break;

                case 1:
                    targetDir = Vector2.right;
                    break;

                case 2:
                    targetDir = Vector2.down;
                    break;

                case 3:
                    targetDir = Vector2.left;
                    break;
            }
            steps = 0;

            targetCompleted = false;
            
        }
    }

    private IEnumerator damageCD()
    {
        canAttack = false;
        yield return new WaitForSeconds(.5f);
        canAttack = true;
    }
}
