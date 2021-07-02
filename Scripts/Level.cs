using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Level : MonoBehaviour
{
    #region Custome Private classes
    [System.Serializable]
    private class PrefabInfo
    {
        public GameObject prefab;                       // Game object that is the prefab.
        public Vector3 positionOffsets;                 // The positon offsets that will be used when placing the generated preFAb.
        [Range(1, 5)]
        public int floorSize = 1;                       // Floorsize. The amount of space the object takes up. This is to prevent multiple
                                                        //      obstacles from overlapping.
    }
    #endregion

    #region Static variables
    private static bool webMode = false;                 // If it is in webMode then it will not make the cursor Invisible or lock the cursor.
    private static bool setEnd;                         // Static boolean that keeps track of if the game should end (used by the enemyClass
                                                        //      which cannot get a direct access to the level instance).
    private static Level instance; 
    #endregion

    #region Instance varables
    [Header("Debugging Settings")]
    [SerializeField] private bool debugRandom;                          // If this bool is true then it will use a seeded Random object to 
                                                                        //      make debugging easier. 

    [SerializeField] private int seedValue;                             // Seed for the seeded random class
    [SerializeField] private bool disableObstacleCreation, testPrefab;  // If disableObstacleCreation is true then there will be no obstacles 
                                                                        //      generated in runtime. Only new floors. If testPrefab is true
                                                                        //      Then only that specific prefab will be created in runtime.
                                                                        
    [SerializeField] PrefabInfo testObj;                                // The object that will be generated every time when testPrefab is true.


    [Header("Camera")]
    [SerializeField] private GameObject cameraObj;                      // Reference to the camera object. 
    [SerializeField] private AudioSource audioSource;                   // Reference to the audio source so it can be muted on pause. 

    [Header("Pausing Settings")]
    [SerializeField] private GameObject messageCanvas;                  // Reference to the message canvas gameObject. 
    [SerializeField] private GameObject pauseMenu;                      // Reference to the pause menu gameObject. 
    [SerializeField] private GameObject gameOverMenu;                   // Reference to the gameOver menu gameObject. 
    [SerializeField] private Player player;                             // Reference to the script of the player gameObject.
    

    [Header("Object Generator Settings")]
    [SerializeField] private GameObject floor;                          // Reference to the prefab floor that it will use to make future floors.
    [SerializeField] private GameObject currFurthestFloor;              // Reference to the current floor. At first it is the floor in the scene
                                                                        //      then it will b ethe current farthest floor. 

    [Tooltip("This is the list of objects that will be used by the random" +
        "generate object function as the game is progressing")]
    [SerializeField] private PrefabInfo[] obstaclesList;                // List of objects that will be used to generate obstacles in runtime. 

    private System.Random random;                                       // Random object that is used to determine which objects are made in runtime.
    private Vector3 floorDimensions;                                    // The dimensions of the floor so it can correctly place them. 
    private Transform currFurthestTrans;                                // The transform of the current farthest floor. 
    private CameraScript cameraScript;                                  // Reference to the instance of the script object of the camera. 
    private bool atLeast2Floors, paused, gameOver = false;              // Will be true if there are at least 2 floors in the game. Paused is true
                                                                        //      if the game state is paused. Game over keeps track of if the game is over
                                                                        //      escapeDisabled prevents the game from pausing multiple time on one escape press.

    private int objectSkipsLeft;                                        // This is used to prevent obstacle generation for a period of time if the size is
                                                                        //      larger than the normal size of 1. 
    #endregion

    #region Static Methods
    /// <summary>
    /// Static method used by the enemy prefab to end the game. This is used
    /// because the enemy and spike prefab cannot get a reference to 
    /// the level instance. 
    /// </summary>
    /// <param name="setEnd">True if ending game false otherwise</param>
    public static void SetEnd(bool setEnd)
    {
        Level.setEnd = setEnd;
    }

    /// <summary>
    /// Gets the instance of the level class.
    /// </summary>
    /// <returns>instance of Level.</returns>
    public static Level GetInsance()
    {
        if (instance != null)
        {
            return instance;
        }
        return null;
    }

    /// <summary>
    /// Returns true if the game is in webmode;
    /// </summary>
    /// <returns>True if it is webmode</returns>
    public static bool IsWebMode()
    {
        return webMode;
    }
    #endregion

    #region Public Methods

    /// <summary>
    /// Should bring up the pause menu. Pause the audio, the camera and freeze the player.
    /// </summary>
    /// <param name="toPause"></param>
    public void Pause(bool toPause)
    {
        FreezeEverything(toPause, true);
    }

    /// <summary>
    /// Ends the game and brings up the game over menu.
    /// </summary>
    public void EndGame()
    {
        gameOver = true;
        FreezeEverything(true, false);
    }

    /// <summary>
    /// This method will return true if there are currently two floors
    /// present in the game sene. 
    /// </summary>
    /// <returns>true if there are two floors present</returns>
    public bool HasTwoFloors()
    {
        return atLeast2Floors;
    }

    /// <summary>
    ///  This method is used by the floor extender class and the prefab
    /// killer class. This will set the bool for whether or not there are
    /// two floors present.
    /// </summary>
    /// <param name="twoFloors">The new twoFloors boolean that will be set</param>
    /// <returns>the current value of the boolean after setting</returns>    
    public bool SetTwoFloors(bool twoFloors)
    {
        atLeast2Floors = twoFloors;
        return atLeast2Floors;
    }

    /// <summary> 
    ///This will return true if the camera is paused and the movement has
    /// stopped. 
    /// </summary>
    /// <returns>a boolean that is true if the camera is paused</returns>
    public bool IsPaused()
    {
        return cameraScript.IsPaused();
    }

    /// <summary>
    /// This will stop the camera and boundaries and everything from moving. 
    /// This method will be called when the game is paused or the game is over
    /// which is determined by the player class. 
    /// </summary>
    /// <param name="pause">the boolean to set for the camera paused</param>
    public void PauseCamera(bool pause)
    {
        cameraScript.Pause(pause);
    }

    /// <summary>
    /// Method that is called that will make a new floor object that is exactly 
    /// adjacent to the current floor object. It uses the current floor objects
    /// transform to do this. This will also update the level class so it knows
    /// that level contains two floors. (This is to make sure that the prefab killer
    /// doesn't destroy the floor while the player is still on it). It makes the new
    /// floor a child of the empty level object. 
    /// </summary>
    public void ExtendFloor()
    {
        float xLocation = currFurthestTrans.position.x + floorDimensions.x;
        Vector3 newPos = new Vector3(xLocation, currFurthestTrans.position.y, currFurthestTrans.position.z);
        currFurthestFloor = Instantiate(floor, newPos, new Quaternion(0, 0, 0, 1));
        currFurthestTrans = currFurthestFloor.transform;
        currFurthestFloor.transform.SetParent(gameObject.transform);

        SetTwoFloors(true);
    }

    /// <summary>
    /// Makes and adds a new random object at the specified position using the list of objects
    /// and a random value. The objects will be offset by the specified offset associated
    /// with the instance in the array. Additionally the object will only be made if the variable
    /// object skips is at 0. Otherwise nothing will happen. This is to allow for the future creation
    /// of larger obstacles that span more than one portion of the floor space. 
    /// </summary>
    /// <param name="position">Vector3 corresponding to the position the object will be placed</param>
    public void MakeObject(Vector3 position)
    {
        if (disableObstacleCreation)
        {
            return;
        }
        if (position == null)
        {
            Debug.LogError("null position input into level make object");
            return;
        }
        if (objectSkipsLeft > 0)
        {
            objectSkipsLeft--;
            return;
        }
        int objectKey = random.Next(obstaclesList.Length);
        if (obstaclesList[objectKey] == null)
        {
            Debug.LogError("null reference in object list at position " + objectKey);
            return;
        }
        PrefabInfo objectInfo = obstaclesList[objectKey];
        if (testPrefab)
        {
            objectInfo = testObj;
        }

        objectSkipsLeft = objectInfo.floorSize - 1;
        Vector3 finalPosition = objectInfo.positionOffsets + position;
        GameObject toInstantiate = objectInfo.prefab;

        GameObject created = Instantiate(toInstantiate, finalPosition, new Quaternion(0, 0, 0, 1));
        created.transform.SetParent(gameObject.transform);
    }
    #endregion

    #region Unity Methods

    // Start is called before the first frame update
    /// <summary>
    /// This method is called first before anyting. This will load up the refernces to 
    /// the camera scrips and the necessary transforms and dimensions. 
    /// </summary>
    private void Awake()
    {
        setEnd = false;
        if (instance == null)
        {
            instance = this;
        }
        cameraScript = cameraObj.GetComponent<CameraScript>();
        atLeast2Floors = false;
        floorDimensions = floor.transform.localScale;
        currFurthestTrans = currFurthestFloor.transform;
        if (debugRandom)
        {
            random = new System.Random(seedValue);
        }
        else
        {
            random = new System.Random();
        }
        /*
        Floor currentFloor = currFurthestFloor.GetComponent<Floor>();
        
        
        
        
        
        
        if (currentFloor == null)
        {
            Debug.LogError("null floor class reference");
        } 
        else
        {
            currentFloor.SetLevelInChildren(this);
        }
        */
    }

    /// <summary>
    /// Makes the cursor invisible when the game starts. 
    /// </summary>
    private void Start()
    {
        Cursor.visible = false;
        FreezeEverything(false, true);
        if (debugRandom || testPrefab || disableObstacleCreation || webMode)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    /// <summary>
    /// Will check if the escape key has been pressed. If the escape key is pressed
    /// then it will bring up the pause menu. If the game is currently paused
    /// pressing the escape key removes the pause menu and plays the game.
    /// You should not be able to open the puase menu when
    /// the game is over.
    /// </summary>
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !gameOver)
        {
            Pause(!paused);
        }

        if (setEnd && !gameOver)
        {
            EndGame();
            setEnd = false;
        }
    }
    #endregion


    #region Private Methods

    /// <summary>
    /// Method that is called when a menu is displayed. It pauses
    /// the camera, the audio source, freezes the enemy and the 
    /// player in place. It also enables the message canvas and the 
    /// appropriate menu as well as enabling or disabling the cursor. 
    /// </summary>
    /// <param name="toFreeze">true if the game is to be frozen</param>
    /// <param name="isMenu">true if is a pause screen false if gameOver screen</param>
    private void FreezeEverything(bool toFreeze, bool isMenu)
    {
        paused = toFreeze;
        Enemy.Freeze(toFreeze);
        messageCanvas.SetActive(toFreeze);
        cameraScript.Pause(toFreeze);
        player.FreezePlayer(toFreeze);
        if (toFreeze)
        {
            audioSource.Pause();
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            audioSource.Play();
            if (!webMode)
            {
                Cursor.visible = toFreeze;
                Cursor.lockState = CursorLockMode.Locked;
            } else
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
            
        }
        if (isMenu)
        {
            pauseMenu.SetActive(toFreeze);
        }
        else
        {
            gameOverMenu.SetActive(true);
        }

    }
    #endregion
}


