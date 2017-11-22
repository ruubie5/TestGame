using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

namespace Completed
{
	using System.Collections.Generic;		//Allows us to use Lists. 
	using UnityEngine.UI;					//Allows us to use UI.
    using UnityEngine.Analytics;            //Allows us to use Unity Analytics.
	
	public class GameManager : MonoBehaviour
	{
        public GameObject player;
        public Sprite overlayImage;
		public float levelStartDelay = 2f;						//Time to wait before starting level, in seconds.
		public float turnDelay = 0.1f;							//Delay between each Player turn.
		public int playerFoodPoints = 100;						//Starting value for Player food points.
		public static GameManager instance = null;				//Static instance of GameManager which allows it to be accessed by any other script.
		[HideInInspector] public bool playersTurn = true;		//Boolean to check if it's players turn, hidden in inspector but public.
		
		
		private Text levelText;									//Text to display current level number.
		private GameObject levelImage;							//Image to block out level as levels are being set up, background for levelText.
        private GameObject levelButtons;
		private BoardManager boardScript;						//Store a reference to our BoardManager which will set up the level.
		private int level = 0;									//Current level number, expressed in game as "Day 1".
		private bool enemiesMoving;								//Boolean to check if enemies are moving.
		private bool doingSetup = true;                         //Boolean to check if we're setting up board, prevent Player from moving during setup.
        


        //Awake is always called before any Start functions
        void Awake()
		{
            if (SceneManager.GetActiveScene().name != "IntroScene")
            {
                player.SetActive(false);

                //Check if instance already exists
                if (instance == null)

                    //if not, set instance to this
                    instance = this;

                //If instance already exists and it's not this:
                else if (instance != this)

                    //Then destroy this. This enforces our singleton pattern, meaning there can only ever be one instance of a GameManager.
                    Destroy(gameObject);

                //Get a component reference to the attached BoardManager script
                boardScript = GetComponent<BoardManager>();
            }
			
		}

        //this is called only once, and the paramter tell it to be called only after the scene was loaded
        //(otherwise, our Scene Load callback would be called the very first load, and we don't want that)
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static public void CallbackInitialization()
        {
            //register the callback to be called everytime the scene is loaded
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        //This is called each time a scene is loaded.
        static private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            //instance.level++;
            instance.InitGame();
        }

		
		//Initializes the game for each level.
		void InitGame()
		{
            if (SceneManager.GetActiveScene().name == "_Complete-Game")
            {
                //While doingSetup is true the player can't move, prevent player from moving while title card is up.
                doingSetup = true;

                //Get a reference to our image LevelImage by finding it by name.
                levelImage = GameObject.Find("LevelImage");
                levelButtons = GameObject.Find("LevelButtons");

                //Get a reference to our text LevelText's text component by finding it by name and calling GetComponent.
                levelText = GameObject.Find("LevelText").GetComponent<Text>();

                //Set the text of levelText to the string "Day" and append the current level number.
                levelText.text = "You wake up in a strange area, while voices echo all around you";

                //Set levelImage to active blocking player's view of the game board during setup.
                levelImage.SetActive(true);
                levelButtons.SetActive(false);

                //Call the HideLevelImage function with a delay in seconds of levelStartDelay.
                Invoke("HideLevelImage", levelStartDelay);

                //Call the SetupScene function of the BoardManager script, pass it current level number.
                boardScript.SetupScene(level, false);
                boardScript.level = level;
            }
			
		}
		
		
		//Hides black image used between levels
		void HideLevelImage()
		{
            //Disable the levelImage gameObject.
            levelImage.GetComponent<Image>().sprite = overlayImage;
            levelImage.transform.SetParent(player.GetComponentInChildren<Canvas>().transform);
            levelText.text = "";
            levelButtons.SetActive(false);
            player.SetActive(true);
            GameObject.Find("FoodImage").transform.SetParent(null);
            GameObject.Find("FoodImage").transform.SetParent(player.GetComponentInChildren<Canvas>().transform);
            Camera.main.transform.SetParent(player.transform);

            //Set doingSetup to false allowing player to move again.
            doingSetup = false;
		}
		
		//GameOver is called when the player reaches 0 food points
		public void GameOver()
		{
			//Set levelText to display number of levels passed and game over message
			levelText.text = "You have perished..";

            //Enable black background image gameObject.
            Camera.main.transform.SetParent(null);
            Camera.main.transform.position = new Vector3(0.0f, 0.0f, -10.0f);
            player.SetActive(false);
            levelImage.GetComponent<Image>().sprite = null;
            levelImage.transform.SetParent(GameObject.Find("StartCanvas").transform);
            levelImage.GetComponent<RectTransform>().position = new Vector3(0.0f, 0.0f, 0.0f);
            levelButtons.transform.SetParent(null);
            levelButtons.transform.SetParent(GameObject.Find("StartCanvas").transform);
            levelButtons.transform.position = new Vector3(5.7f, -1.88f, 0.0f);
            levelButtons.SetActive(true);
        }
	}
}

