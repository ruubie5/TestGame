using UnityEngine;
using System;
using System.Collections.Generic; 		//Allows us to use Lists.
using Random = UnityEngine.Random;      //Tells Random to use the Unity Engine random number generator.
using UnityEngine.UI;

namespace Completed
	
{

    public class BoardManager : MonoBehaviour
    {
        private bool exitIsSpawned;     //Keeps track if the exit door has been spawned

        // Using Serializable allows us to embed a class with sub properties in the inspector.
        [Serializable]
        public class Count
        {
            public int minimum;             //Minimum value for our Count class.
            public int maximum;             //Maximum value for our Count class.          


            //Assignment constructor.
            public Count(int min, int max)
            {
                minimum = min;
                maximum = max;
            }
        }

        //For the door positions
        [Serializable]
        public class DoorPositioning
        {
            public int xPos_1;
            public int xPos_columns;
            public int yPos_1;
            public int yPos_rows;

            public DoorPositioning(int x, int columns, int y, int rows)
            {
                xPos_1 = x;
                xPos_columns = columns;
                yPos_1 = y;
                yPos_rows = rows;
            }
        }

        public int maxRooms;
        public int roomCounter;
        public int totalItems = 0;
        public int columns = 12;                                        //Number of columns in our game board.
        public int rows = 12;                                           //Number of rows in our game board.
        public int roomSizeMultiplier = 2;
        public int maxRoomSize = 24;
        public int crowdedness = 2;
        public int resourcemulitplier = 4;
        public int level;
        public Count wallCount = new Count(5, 9);                       //Lower and upper limit for our random number of walls per level.
        public Count foodCount = new Count(1, 5);                       //Lower and upper limit for our random number of food items per level.
        public Count trapCount = new Count(2, 5);
        public Count exitCount = new Count(1, 1);
        public GameObject[] exit;                                         //Prefab to spawn for exit.
        public GameObject door;
        public GameObject[] weapons; 
        public GameObject[] chest;
        public GameObject[] floorTiles;                                 //Array of floor prefabs.
        public GameObject[] wallTiles;                                  //Array of wall prefabs.
        public GameObject[] foodTiles;                                  //Array of food prefabs.
        public GameObject[] enemyTiles;                                 //Array of enemy prefabs.
        public GameObject[] outerWallTiles;								//Array of outer tile prefabs.
        public GameObject[] trap;
        private Vector2 roomOrigin;

        private Transform boardHolder;                                  //A variable to store a reference to the transform of our Board object.

        public int doorsOpen;                                           //keeps track of how many doors have been opened in total; 
        private GameObject pickUpText;

        //Lists for keeping track of everything more easily
        public List<Vector3> gridPositions = new List<Vector3>();	        //A list of possible locations to place tiles.
        public List<Vector3> cantplacePositions = new List<Vector3>();          //A list where we're not allowed to place doors etc.
        public List<GameObject> cantplaceObjects = new List<GameObject>();      //list of outer walls already placed
        public List<DoorScript> doors = new List<DoorScript>();                 //List of all the doors in the scene

        private void Start()
        {
            maxRooms = Random.Range(20, 30);
            pickUpText = GameObject.Find("PickUpText");

            //Amount of walls to spawn in a room
            wallCount.minimum *= (roomSizeMultiplier * crowdedness);
            wallCount.maximum *= (roomSizeMultiplier * crowdedness);

            //Amount of food to spawn in a room
            foodCount.minimum *= (roomSizeMultiplier * resourcemulitplier);
            foodCount.maximum *= (roomSizeMultiplier * resourcemulitplier);
        }

        //Clears our list gridPositions and prepares it to generate a new board.
        void InitialiseList()
        {
            //Clear our list gridPositions.
            gridPositions.Clear();

            //Loop through x axis (columns).
            for (int x = 1; x < columns - 1; x++)
            {
                //Within each column, loop through y axis (rows).
                for (int y = 1; y < rows - 1; y++)
                {
                    //At each index add a new Vector3 to our list with the x and y coordinates of that position.
                    gridPositions.Add(new Vector3(x, y, 0f));
                }
            }
        }

        //Sets up the outer walls and floor (background) of the game board.
        void BoardSetup(int level)
        {
            columns *= roomSizeMultiplier;
            rows *= roomSizeMultiplier;           

            //Maxsize
            if (columns > maxRoomSize)
            {
                columns = maxRoomSize;
            }

            if (rows > maxRoomSize)
            {
                rows = maxRoomSize;
            }

            //Instantiate Board and set boardHolder to its transform.
            boardHolder = new GameObject("Board").transform;
            int doorAmount = Random.Range(4, 4);
            DoorPositioning doorPos = new DoorPositioning(Random.Range(0, columns), Random.Range(0, columns), Random.Range(0, rows), Random.Range(0, rows));

			//Loop along x axis, starting from -1 (to fill corner) with floor or outerwall edge tiles.
			for(int x = -1; x < columns + 1; x++)
			{
				//Loop along y axis, starting from -1 to place floor or outerwall tiles.
				for(int y = -1; y < rows + 1; y++)
				{
					//Choose a random tile from our array of floor tile prefabs and prepare to instantiate it.
					GameObject toInstantiate = floorTiles[Random.Range (0,floorTiles.Length)];

                    //Check if we current position is at board edge, if so choose a random outer wall prefab from our array of outer wall tiles.
                    if (x == -1 || x == columns || y == -1 || y == rows)
                    {
                        cantplacePositions.Add(new Vector3(x, y, 0));

                        //Placing doors or outer walls
                        if (doorAmount > 0 && x != -1 && x != columns || doorAmount > 0 && y != -1 && y != rows)
                        {
                            if (y == -1 && x == doorPos.xPos_1 || y == rows && x == doorPos.xPos_columns 
                                || x == -1 && y == doorPos.yPos_1 || x == columns && y == doorPos.yPos_rows)
                            {
                                toInstantiate = door;
                                door.GetComponent<DoorScript>().roomOrigin = roomOrigin;
                                door.GetComponent<DoorScript>().openNew = true;
                                doors.Add(toInstantiate.GetComponent<DoorScript>());
                                doorAmount--;                          
                            }
                            else
                            {
                                toInstantiate = outerWallTiles[Random.Range(0, outerWallTiles.Length)];
                            }
                        }
                        else
                        {
                            toInstantiate = outerWallTiles[Random.Range(0, outerWallTiles.Length)];
                        }
                    }
					
					//Instantiate the GameObject instance using sthe prefab chosen for toInstantiate at the Vector3 corresponding to current grid position in loop, cast it to GameObject.
					GameObject instance =
						Instantiate (toInstantiate, new Vector3 (x, y, 0f), Quaternion.identity) as GameObject;

                    cantplaceObjects.Add(instance);
                    totalItems++;

                    instance.name = instance.name + " - " + totalItems;

                    //Set the parent of our newly instantiated object instance to boardHolder, this is just organizational to avoid cluttering hierarchy.
                    instance.transform.SetParent (boardHolder);
				}
			}
		}
		
		
		//RandomPosition returns a random position from our list gridPositions.
		Vector3 RandomPosition ()
		{
			//Declare an integer randomIndex, set it's value to a random number between 0 and the count of items in our List gridPositions.
			int randomIndex = Random.Range (0, gridPositions.Count);
			
			//Declare a variable of type Vector3 called randomPosition, set it's value to the entry at randomIndex from our List gridPositions.
			Vector3 randomPosition = gridPositions[randomIndex];
			
			//Remove the entry at randomIndex from the list so that it can't be re-used.
			gridPositions.RemoveAt (randomIndex);
			
			//Return the randomly selected Vector3 position.
			return randomPosition;
		}
		
		
		//LayoutObjectAtRandom accepts an array of game objects to choose from along with a minimum and maximum range for the number of objects to create.
		void LayoutObjectAtRandom (GameObject[] tileArray, int minimum, int maximum)
		{
			//Choose a random number of objects to instantiate within the minimum and maximum limits
			int objectCount = Random.Range (minimum, maximum+1);
			
			//Instantiate objects until the randomly chosen limit objectCount is reached
			for(int i = 0; i < objectCount; i++)
			{
				//Choose a position for randomPosition by getting a random position from our list of available Vector3s stored in gridPosition
				Vector3 randomPosition = RandomPosition();

				
				//Choose a random tile from tileArray and assign it to tileChoice
				GameObject tileChoice = tileArray[Random.Range (0, tileArray.Length)];
				
				//Instantiate tileChoice at the position returned by RandomPosition with no change in rotation
				Instantiate(tileChoice, randomPosition, Quaternion.identity);
			}
		}

        //SetupScene initializes our level and calls the previous functions to lay out the game board
        public void SetupScene(int level, bool newRoom)
        {
            if (newRoom == false)
            {
                //Creates the outer walls and floor.
                BoardSetup(level);

                //Reset our list of gridpositions.
                InitialiseList();
            }

            //Instantiate a random number of wall tiles based on minimum and maximum, at randomized positions.
            LayoutObjectAtRandom(wallTiles, wallCount.minimum, wallCount.maximum);

            //Instantiate a random number of food tiles based on minimum and maximum, at randomized positions.
            LayoutObjectAtRandom(foodTiles, foodCount.minimum, foodCount.maximum);

            //Makes a random number of traps
            LayoutObjectAtRandom(trap, trapCount.minimum, trapCount.maximum);

            //Spawns weapons in the room
            LayoutObjectAtRandom(weapons, 0, 2);

            //Spawns chests on occasion
            int maybe = Random.Range(0, 11);

            if (maybe >= 6)
            {
                LayoutObjectAtRandom(chest, 1, 2); 
            }

            //Determine number of enemies based on current level number, based on a logarithmic progression
            int enemyCount = (int)Mathf.Log(level + doorsOpen, 2f);

            //Instantiate a random number of enemies based on minimum and maximum, at randomized positions.
            LayoutObjectAtRandom(enemyTiles, enemyCount, enemyCount);


            if (Random.Range(0, 11) == 1 && exitIsSpawned == false && newRoom == true || roomCounter >= maxRooms && exitIsSpawned == false && newRoom == true)
            {
                LayoutObjectAtRandom(exit, exitCount.minimum, exitCount.maximum);
                pickUpText.GetComponent<Text>().text = "Your eyes sense daylight, the exit must be nearby!";
                exitIsSpawned = true;
                
            }
        }

        //New room
        public void CreateRoom (int xPos, int yPos, Vector2 doorPos) 
        {            
            gridPositions.Clear();
            int doorAmount = Random.Range(4, 4);
            //Update 
            DoorPositioning newDoorPos = new DoorPositioning(Random.Range(xPos, xPos + columns), Random.Range(xPos, xPos + columns), Random.Range(yPos, yPos + rows), Random.Range(yPos, yPos + rows));

            //Loop along x axis, starting from -1 (to fill corner) with floor or outerwall edge tiles.
            for (float x = xPos - 1.0f; x < xPos + columns + 1.0f; x++)
            {
                //Loop along y axis, starting from -1 to place floor or outerwall tiles.
                for (float y = yPos - 1.0f; y < yPos + rows + 1.0f; y++)
                {
                    //Choose a random tile from our array of floor tile prefabs and prepare to instantiate it.
                    GameObject toInstantiate = floorTiles[Random.Range(0, floorTiles.Length)];

                    //Check if we current position is at board edge, if so choose a random outer wall prefab from our array of outer wall tiles.
                    if (x == xPos - 1.0f || x == xPos + columns || y == yPos - 1.0f || y == yPos + rows)
                    {
                        if (doorAmount > 0 && x != xPos - 1 && x != xPos + columns || doorAmount > 0 && y != yPos - 1 && y != yPos + rows)
                        {
                            Vector3 cantplacehere = new Vector3(x, y, 0);

                            //If the door can make a new room or if it shouldn't 
                            if (!cantplacePositions.Contains(cantplacehere))
                            {
                                door.GetComponent<DoorScript>().openNew = true;
                            }
                            else if (cantplacePositions.Contains(cantplacehere))
                            {
                                door.GetComponent<DoorScript>().openNew = false;
                            }

                            if (y == yPos - 1 && x == newDoorPos.xPos_1 || y == yPos + rows && x == newDoorPos.xPos_columns
                                || x == xPos - 1 && y == newDoorPos.yPos_1 || x == xPos + columns && y == newDoorPos.yPos_rows)
                            {
                                roomCounter++;

                                if (roomCounter <= maxRooms)
                                {
                                    toInstantiate = door;
                                    door.GetComponent<DoorScript>().roomOrigin = new Vector2(xPos, yPos);
                                    doors.Add(toInstantiate.GetComponent<DoorScript>());
                                    doorAmount--;
                                }
                                else
                                {
                                    toInstantiate = outerWallTiles[Random.Range(0, outerWallTiles.Length)];
                                }
                            }
                            else
                            {
                                toInstantiate = outerWallTiles[Random.Range(0, outerWallTiles.Length)];
                            }
                        }
                        else
                        {
                            toInstantiate = outerWallTiles[Random.Range(0, outerWallTiles.Length)];
                        }

                        if (!cantplacePositions.Contains(new Vector3(x, y, 0)))
                        {
                            cantplacePositions.Add(new Vector3(x, y, 0));
                        }
                    }
                    else
                    {
                        gridPositions.Add(new Vector3(x, y, 0f));
                    }

                    if (x == doorPos.x && y == doorPos.y)
                    {
                        toInstantiate = floorTiles[Random.Range(0, floorTiles.Length)];
                    }

                    //Instantiate the GameObject instance using sthe prefab chosen for toInstantiate at the Vector3 corresponding to current grid position in loop, cast it to GameObject.
                    GameObject instance =
                        Instantiate(toInstantiate, new Vector3(x, y, 0f), Quaternion.identity) as GameObject;

                    cantplaceObjects.Add(instance);
                    totalItems++;

                    instance.name = instance.name + " - " + totalItems;

                    //Set the parent of our newly instantiated object instance to boardHolder, this is just organizational to avoid cluttering hierarchy.
                    instance.transform.SetParent(boardHolder);
                }
            }

            SetupScene(level, true);
            //Add another room to network room counter so not every room will have a door eventually
        }
    }
}
