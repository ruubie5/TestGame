using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Completed;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour {

    public Color defaultColor;
    public Color gotHurt;
    public Color justAte;
    public float restartLevelDelay = 1f;        //Delay time in seconds to restart level.
    public int pointsPerFood = 10;              //Number of points to add to player food points when picking up a food object.
    public int pointsPerSoda = 20;              //Number of points to add to player food points when picking up a soda object.
    public int wallDamage = 1;                  //How much damage a player does to a wall when chopping it.
    public AudioClip attackSound;
    public AudioClip footstep;
    public AudioClip equipSound;
    public AudioClip dropSound;
    public AudioClip moveSound1;                //1 of 2 Audio clips to play when player moves.
    public AudioClip moveSound2;                //2 of 2 Audio clips to play when player moves.
    public AudioClip eatSound1;                 //1 of 2 Audio clips to play when player collects a food object.
    public AudioClip eatSound2;                 //2 of 2 Audio clips to play when player collects a food object.
    public AudioClip drinkSound1;               //1 of 2 Audio clips to play when player collects a soda object.
    public AudioClip drinkSound2;               //2 of 2 Audio clips to play when player collects a soda object.
    public AudioClip gameOverSound;             //Audio clip to play when player dies.

    public Animator animator;                  //Used to store a reference to the Player's animator component.
    private SpriteRenderer spriteRenderer;      //Used to store a reference to the Player's spriterenderer component.
    public float food = 100;                           //Used to store player food points total during level.

    public int isFacingDir;
    private Vector2 rayDir;

    private RaycastHit2D attackRayHit;          //used to store the raycasthit of the attack.
    private IEnumerator coroutine;

    private RectTransform rectTrans;

    private Vector2 hitDir;
    private RaycastHit2D attackHit;
    private RaycastHit2D frontHit;
    private RaycastHit2D weaponHit;
    private Vector2 rayPos;
    private Vector2 frontRayPos;

    private bool isMoving;                      //Stores if the player is moving or not

    private GameObject foodObject;
    private Text foodText;
    public GameObject pickUpText;
    public Slider cooldownSlider;
    public Slider durabilitySlider;
    private bool recharging;

    public float movementSpeed;

    public WeaponScript equippedWeapon;
    public Transform weaponPos_1;
    public Transform weaponPos_2;

    public int rows;
    public int columns;

    private int usedSodas = 0;
    private int usedFood = 0;
    private int zombiesKilled = 0;
    private int doorsOpened = 0;
    private int chestsOpened = 0;
    private int trapsTriggered = 0;
    private float timePlayed = 0;
    private float move = 0;


    // Use this for initialization
    void Start()
    {
        animator = this.GetComponent<Animator>();
        spriteRenderer = this.GetComponent<SpriteRenderer>();
        rectTrans = this.GetComponent<RectTransform>();

        if (SceneManager.GetActiveScene().name != "IntroScene")
        {
            foodObject = GameObject.Find("FoodText");
            foodText = foodObject.GetComponent<Text>();
            columns = FindObjectOfType<BoardManager>().columns;
            rows = FindObjectOfType<BoardManager>().rows;
        }
    }
    // Update is called once per frame

    private void Update()
    {
        frontHit = Physics2D.Raycast(this.transform.position, rayDir, 1f);

        if (frontHit.collider != null)
        {
            if (frontHit.collider.GetComponent<HighlightObject>() != null)
            {
                frontHit.collider.GetComponent<HighlightObject>().isActive = true;
            }
        }
    }

    void FixedUpdate()
    {

        if (recharging == true)
        {
            cooldownSlider.value += Time.deltaTime;

            if (cooldownSlider.value == cooldownSlider.maxValue)
            {
                recharging = false;
            }
        }

        if (food > 100)
        {
            food = 100;
        }

        if (SceneManager.GetActiveScene().name != "IntroScene")
        {
            foodText.text = " :" + Mathf.RoundToInt(food).ToString();
        }
        else
        {
            food = 90;
        }

        int horizontal = 0;     //Used to store the horizontal move direction.
        int vertical = 0;       //Used to store the vertical move direction.

        //Get input from the input manager, round it to an integer and store in horizontal to set x axis move direction
        horizontal = (int)(Input.GetAxisRaw("Horizontal"));

        //Get input from the input manager, round it to an integer and store in vertical to set y axis move direction
        vertical = (int)(Input.GetAxisRaw("Vertical"));

        //Check if moving horizontally, if so set vertical to zero.
        if (horizontal != 0)
        {
            vertical = 0;
        }

        //Check whether the player is moving or not
        if (horizontal != 0 || vertical != 0)
        {
            isMoving = true;
        }
        else
        {
            isMoving = false;
        }

        //If the player is moving the food of the player should go down
        if (isMoving)
        {
            movementSpeed = 2;
            food -= Time.deltaTime;
            move += Time.deltaTime;

            if (move >= 0.6f)
            {
                AudioSource.PlayClipAtPoint(footstep, transform.position);
                move = 0;
            }
        }

        //Allows the player to run at the cost of a lot of food
        if (Input.GetKey(KeyCode.LeftShift) && isMoving)
        {
            movementSpeed = 5;
            food -= Time.deltaTime * 7;
            move += Time.deltaTime;

            if (move >= 1.1f)
            {
                AudioSource.PlayClipAtPoint(footstep, transform.position);
                move = 0;
            }
        }

        if (Input.GetKey(KeyCode.Q) && equippedWeapon != null)
        {
            AudioSource.PlayClipAtPoint(dropSound, transform.position);
            Destroy(equippedWeapon.gameObject);
            equippedWeapon = null;
            durabilitySlider.value = durabilitySlider.minValue;
            cooldownSlider.value = cooldownSlider.minValue;
            pickUpText.GetComponent<Text>().text = "You dropped your weapon!";
        }

        //Set the sprite flip to facing right.
        if (horizontal == 1)
        {
            if (equippedWeapon != null)
            {
                equippedWeapon.transform.SetParent(weaponPos_2.transform);
                equippedWeapon.transform.position = weaponPos_2.transform.position;
                equippedWeapon.gameObject.GetComponent<SpriteRenderer>().flipX = false;

                if (equippedWeapon.canAttack == false)
                {
                    equippedWeapon.transform.eulerAngles = new Vector3(0.0f, 0.0f, -45.0f);
                }
            }

            isFacingDir = 1;
            flpSprite(isFacingDir);
            rayDir = Vector2.right;

            this.transform.Translate(1 * Time.deltaTime * movementSpeed, 0, 0);
        }



        //Set the sprite fip to facing left;
        if (horizontal == -1)
        {
            if (equippedWeapon != null)
            {
                equippedWeapon.transform.SetParent(weaponPos_1.transform);
                equippedWeapon.transform.position = weaponPos_1.transform.position;
                equippedWeapon.gameObject.GetComponent<SpriteRenderer>().flipX = true;

                if (equippedWeapon.canAttack == false)
                {
                    equippedWeapon.transform.eulerAngles = new Vector3(0.0f, 0.0f, 45.0f);
                }
            }

            isFacingDir = 4;
            flpSprite(isFacingDir);
            rayDir = Vector2.left;

            this.transform.Translate(-1 * Time.deltaTime * movementSpeed, 0, 0);
        }

        //Move up
        if (vertical == 1)
        {
            isFacingDir = 2;
            flpSprite(isFacingDir);
            rayDir = Vector2.up;


            this.transform.Translate(0, 1 * Time.deltaTime * movementSpeed, 0);
        }

        //Move downs
        if (vertical == -1)
        {
            isFacingDir = 3;
            flpSprite(isFacingDir);
            rayDir = Vector2.down;

            this.transform.Translate(0, -1 * Time.deltaTime * movementSpeed, 0);
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            food -= 2;
            animator.SetTrigger("playerChop");
            rayPos = this.transform.position;

            //right
            if (isFacingDir == 1)
            {
                hitDir = Vector2.right;
                rayPos.x -= .5f;
            }

            //up
            else if (isFacingDir == 2)
            {
                hitDir = Vector2.up;
                rayPos.y -= .5f;
            }

            //down
            else if (isFacingDir == 3)
            {
                hitDir = Vector2.down;
                rayPos.y += .5f;
            }

            //left 
            else if (isFacingDir == 4)
            {
                hitDir = Vector2.left;
                rayPos.x += .5f;
            }

            LayerMask mask = 1 << 9;
            attackHit = Physics2D.Raycast(rayPos, hitDir, 1.5f, ~mask);

            //Attacking with a weapon
            if (equippedWeapon != null && equippedWeapon.canAttack == true)
            {
                if (spriteRenderer.flipX == false)
                {
                    equippedWeapon.gameObject.transform.localEulerAngles = new Vector3(0.0f, 0.0f, -45.0f);
                }
                else if (spriteRenderer.flipX == true)
                {
                    equippedWeapon.gameObject.transform.localEulerAngles = new Vector3(0.0f, 0.0f, 45.0f);
                }

                AudioSource.PlayClipAtPoint(attackSound, transform.position);
                weaponHit = Physics2D.Raycast(rayPos, hitDir, equippedWeapon.weaponRange, ~mask);
                equippedWeapon.justAttacked();

                if (weaponHit.collider != null)
                {
                    if (weaponHit.collider.tag == "Enemy")
                    {
                        ZombieAI foe = weaponHit.collider.gameObject.GetComponent<ZombieAI>();
                        AudioSource.PlayClipAtPoint(foe.AIPainSound, transform.position);
                        foe.health -= equippedWeapon.damage;
                        foe.GetComponent<Rigidbody2D>().AddForce((foe.transform.position - transform.position) * 250 * equippedWeapon.knockBack);
                        foe.Invoke("resetKnockback", 0.1f);
                        equippedWeapon.durability--;
                        durabilitySlider.value--;
                        cooldownSlider.value = cooldownSlider.minValue;
                        recharging = true;

                        if (equippedWeapon.durability == 0)
                        {
                            AudioSource.PlayClipAtPoint(dropSound, transform.position);
                            Destroy(equippedWeapon.gameObject);
                            recharging = false;
                            equippedWeapon = null;
                        }
                    }
                }
            }

            if (attackHit.collider != null)
            {
                //Intro stuff
                if (attackHit.collider.tag == "StartGame")
                {
                    SceneManager.LoadScene("_Complete-Game");
                }

                //Destroying walls
                if (attackHit.collider.tag == "Wall")
                {
                    attackHit.collider.gameObject.GetComponent<Wall>().DamageWall(wallDamage);
                }

                //Opening chests
                if (attackHit.collider.tag == "Chest" && isFacingDir == 2)
                {
                    attackHit.collider.gameObject.GetComponent<ChestScript>().open();
                    chestsOpened++;
                }

                //Picking up and equipping weapons
                if (attackHit.collider.tag == "Weapon")
                {
                    if (equippedWeapon == null)
                    {
                        AudioSource.PlayClipAtPoint(equipSound, transform.position);
                        equippedWeapon = attackHit.collider.gameObject.GetComponent<WeaponScript>();
                        equippedWeapon.GetComponent<BoxCollider2D>().enabled = false;
                        durabilitySlider.maxValue = equippedWeapon.durability;
                        durabilitySlider.value = equippedWeapon.durability;
                        cooldownSlider.maxValue = equippedWeapon.cooldown;
                        cooldownSlider.value = equippedWeapon.cooldown;

                        if (this.spriteRenderer.flipX == true)
                        {
                            equippedWeapon.gameObject.transform.SetParent(weaponPos_1);
                            equippedWeapon.gameObject.transform.position = weaponPos_1.position;
                            equippedWeapon.gameObject.GetComponent<SpriteRenderer>().flipX = true;
                            equippedWeapon.gameObject.GetComponent<SpriteRenderer>().sortingLayerID = gameObject.GetComponent<SpriteRenderer>().sortingLayerID;
                        }
                        else if (this.spriteRenderer.flipX == false)
                        {
                            equippedWeapon.transform.SetParent(weaponPos_2);
                            equippedWeapon.transform.position = weaponPos_2.position;
                            equippedWeapon.gameObject.GetComponent<SpriteRenderer>().flipX = false;
                            equippedWeapon.gameObject.GetComponent<SpriteRenderer>().sortingLayerID = gameObject.GetComponent<SpriteRenderer>().sortingLayerID;
                        }

                        pickUpText.GetComponent<Text>().text = "You picked up the " + attackHit.collider.gameObject.GetComponent<WeaponScript>().weaponName;
                    }

                }

                //Picking up food
                else if (attackHit.collider.tag == "Food")
                {
                    StartCoroutine(iAte(pointsPerFood, attackHit.collider.gameObject, "food"));
                }

                //Picking up soda
                else if (attackHit.collider.tag == "Soda")
                {
                    StartCoroutine(iAte(pointsPerSoda, attackHit.collider.gameObject, "soda"));
                }

                //Opening a new room
                else if (attackHit.collider.tag == "Door")
                {
                    AudioSource.PlayClipAtPoint(attackHit.collider.GetComponent<DoorScript>().openSound, transform.position);
                    Vector2 pos = transform.position - attackHit.transform.position;
                    FindObjectOfType<BoardManager>().doorsOpen++;
                    pickUpText.GetComponent<Text>().text = "You opened a door!";
                    doorsOpened++;

                    if (attackHit.collider.GetComponent<DoorScript>().openNew == true)
                    {

                        if (pos.x < 0 && isFacingDir == 1)
                        {
                            FindObjectOfType<BoardManager>().CreateRoom((int)attackHit.collider.GetComponent<DoorScript>().roomOrigin.x + columns + 1, (int)attackHit.collider.GetComponent<DoorScript>().roomOrigin.y, attackHit.collider.transform.position);
                        }
                        else if (pos.x > 0 && isFacingDir == 4)
                        {
                            FindObjectOfType<BoardManager>().CreateRoom((int)attackHit.collider.GetComponent<DoorScript>().roomOrigin.x - columns - 1, (int)attackHit.collider.GetComponent<DoorScript>().roomOrigin.y, attackHit.collider.transform.position);
                        }

                        if (pos.y < 0 && isFacingDir == 2)
                        {
                            FindObjectOfType<BoardManager>().CreateRoom((int)attackHit.collider.GetComponent<DoorScript>().roomOrigin.x, (int)attackHit.collider.GetComponent<DoorScript>().roomOrigin.y + rows + 1, attackHit.collider.transform.position);
                        }
                        else if (pos.y > 0 && isFacingDir == 3)
                        {
                            FindObjectOfType<BoardManager>().CreateRoom((int)attackHit.collider.GetComponent<DoorScript>().roomOrigin.x, (int)attackHit.collider.GetComponent<DoorScript>().roomOrigin.y - rows - 1, attackHit.collider.transform.position);
                        }

                        FindObjectOfType<BoardManager>().cantplaceObjects.Remove(attackHit.collider.gameObject);
                        Destroy(attackHit.collider.gameObject);
                    }
                    else
                    {
                        Instantiate(FindObjectOfType<BoardManager>().floorTiles[UnityEngine.Random.Range(0, FindObjectOfType<BoardManager>().floorTiles.Length)], attackHit.transform.position, Quaternion.identity);
                        FindObjectOfType<BoardManager>().cantplaceObjects.Remove(attackHit.collider.gameObject);
                        Destroy(attackHit.collider.gameObject);
                    }
                }

                //Opening the exit
                else if (attackHit.collider.tag == "Exit")
                {
                    Invoke("exitGame", 5);
                    foodText.text = "Well done you've escaped this wretched place!";
                    enabled = false;
                }

            }
            else
            {
                AudioSource.PlayClipAtPoint(attackSound, transform.position);
            }

            Invoke("removeText", 1.0f);
        }

        if (food <= 0)
        {
            FindObjectOfType<GameManager>().GameOver();

            food = Mathf.Infinity;
        }
    }

    private IEnumerator iAte(int foodToAdd, GameObject toSetInactive, string whichPickedUp)
    {
        if (SceneManager.GetActiveScene().name != "IntroScene")
        {
            food += foodToAdd;
            SoundManager.instance.RandomizeSfx(drinkSound1, drinkSound2);
            animator.SetTrigger("playerEat");
            toSetInactive.SetActive(false);
            pickUpText.GetComponent<Text>().text = "You picked up some " + whichPickedUp + "!";
            foodText.GetComponent<Text>().color = justAte;
            yield return new WaitForSeconds(1.0f);
            foodText.GetComponent<Text>().color = defaultColor;
        }
        else
        {
            food += foodToAdd;
            animator.SetTrigger("playerEat");
            toSetInactive.SetActive(false);
        }
    }

    public IEnumerator hurtMe ()
    {
        if (SceneManager.GetActiveScene().name != "IntroScene")
        {
            foodText.GetComponent<Text>().color = gotHurt;
            yield return new WaitForSeconds(1.0f);
            foodText.GetComponent<Text>().color = defaultColor;
        }
    }   

    void flpSprite(int facing)
    {
        if (facing == 1)
        {
            this.spriteRenderer.flipX = false;
        }

        else if (facing == 4)
        {
            this.spriteRenderer.flipX = true;
        }
    }
    
    //This currently causes bugs to happen on the next levels. 
    void nextLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex, LoadSceneMode.Single);
    }  

    void exitGame()
    {
        Application.Quit();
    }

    void removeText()
    {
        pickUpText.GetComponent<Text>().text = "";
    }

    public void removeKnockback ()
    {
        GetComponent<Rigidbody2D>().velocity = Vector2.zero;
    }
}