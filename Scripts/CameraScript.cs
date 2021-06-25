using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CameraScript : MonoBehaviour
{
    #region Variables
    #region Static Variables

    private const long MILLISECONDS_IN_SECOND = 1000;           // Milliseconds in a second for calculations.
    private const float MAX_CAMERA_SPEED = 0.175f;              // Max allowable camera speed to prevent weird behaviors.
    private static bool forcedPause, pauseOnDeathStatic;        // static bools to check if camera should pause on death or forced. 
                                                                //      This allows objects without a camera reference to pause the camera.
    
    private static int difficultyLevel = 2;                     // Static reference to the difficulty level that will change.
    #endregion

    #region Instance Variables

    [Header("Following Settings")]
    [SerializeField] private GameObject player;                                 // The transform of the player that the camera is following.
    [SerializeField] private AnimationCurve offsetCurve;                        // The animation curve which dictates the camera offset as the player changes height. 
    [SerializeField] private float forwardFollowingSpeed, maxJumpHeight;        // The floats for the speed at which the player has to go for the camera to follow it forward
                                                                                //      as well as the maximum height at which the camera will no longer rise with the player.

    [SerializeField] [Range(0f, 10f)] private float initialOffset;              // The initial y offset of the camera.
    [SerializeField] [Range(0.01f, 1f)] private float smoothSpeed = 0.125f;     // The smoothing value that dictates how fast the camera will follow the player in the vertical
                                                                                //      direction


    [Header("Scrolling Settings")]
    [SerializeField] private bool scroll = false;                                       // Dictates if the camera scrolls. 
    [SerializeField ] private bool  pauseOnDeath = true, increaseScrollSpeed = false,
        debuggingMode;                                                                  // Dictates if the camera will pause when the player dies and if the speed will increase
                                                                                        //      as the game goes on. If debugging Mode is true
                                                                                        //      then the difficulty will that set by the inspector.

    [SerializeField] [Range(0, 3)] private int difficulty = difficultyLevel;            // Difficulty level that adjusts all the other scrolling values.
    [SerializeField] [Range(0f, .175f)] private float scrollingSpeed = 0.01f;           // The camera scrolling speed.
    [Range(1, 120)] [SerializeField] private int secondsPerIncrement = 1;               // After how many seconds the camera will increase the scrolling speed.
    [Range(0f, 1f)] [SerializeField] private float percentIncrease;                     // The percent the speed increases. The range is to prevent objects from teleproting into
                                                                                        //      each other.


    private long start, currTime, prePausedTime, lastIncrement;         // longs for keeping track of milliseconds. Start is when the game starts
                                                                        //      currTime is the last millisecond count when the game was not paused.
                                                                        //      prePausedTime is the amount of milliseconds that passed before the game was paused
                                                                        //      lastIncrement was the last time the camera speed was incremented. 

    private float playerCameraDifference, boostedAmount, originalSpeed; // The speed difference between the camera and player. To make sure that as
                                                                        //      the camera speed increases so does the player speed. The boosted amount
                                                                        //      is the percent faster the camera is going from its original speed.
                                                                        //      The original speed is the amount the originalSpeed was when the game started.

    private bool pause;                                                 // Bool to keep track of whether the camera and game is paused. 
    private Transform target;                                           // Transform of the target (this will be the player) to follow.
    private Rigidbody targetBody;                                       // RigidBody of the target. 
    private Player playerClass;                                         // Reference to the instance of the player script.
    private Vector3 velocity = Vector3.zero;                            // Vector3 that will be used to change follow the player vertically.
    #endregion

    #endregion

    #region Static Methods
    /// <summary>
    /// This is a static method that will set the forced paused variable. This variable 
    /// is used to override the current paused state of the camera. It is static so that
    /// other classes can call this method and pause the camera without a reference to the
    /// object. This is for any prefabs that kill the player and then pause the camera because
    /// it is game over. It is difficult to get a camera reference to a prefab as the camera itself
    /// is not a prefab. This seemed like the easiest solution. Finally the boolean will only be 
    /// changed if the pauseOnDeath static variable is true. This value can be changed in the inspector. 
    /// </summary>
    /// <param name="forcedPaused">boolean that the value will be changed to.</param>
    public static void ForcePaused(bool forcedPaused)
    {
        if (pauseOnDeathStatic)
        {
            CameraScript.forcedPause = forcedPaused;
        }
    }


    /// <summary>
    /// this will set the static difficulty level of the game to the difficulty
    /// provided (bounded between 0 and 3).
    /// </summary>
    /// <param name="difficulty">The level of difficulty.</param>
    public static void ChangeDifficulty(int difficulty)
    {
        difficultyLevel = ValidateDifficulty(difficulty);
    }
    /// <summary>
    /// Returns the the static difficulty level variable. 
    /// </summary>
    /// <returns>Int from 0 to 3</returns>
    public static int GetDifficulty()
    {
        return difficultyLevel;
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// This will set the paused boolean. If the camera is paused then
    /// There will be no more scrolling. If pause on death is true (this
    /// is a value that set in the inspector) then the method will change
    /// paused to the corresponding parameter. Otherwise it will ignore the
    /// parameter and never pause. This method also resets the player walking speed.
    /// </summary>
    /// <param name="toPause">bool to change the puased variable to</param>
    public void Pause(bool toPause)
    {
        if (pauseOnDeath)
        {
            pause = toPause;
            forcedPause = toPause;
            prePausedTime = GetScore();
            start = currTime;
        }

    }

    /// <summary>
    /// Returns true if the camera is paused. 
    /// </summary>
    /// <returns>bool true if paused.</returns>
    public bool IsPaused()
    {
        return pause || forcedPause;
    }

    /// <summary>
    /// Returns the score which is the number of milliseconds between the currentTime
    /// which is updated every time fixed update is called and the game is not paused as
    /// well as another long which is the amount of milliseconds that passed before the
    /// game was paused. 
    /// </summary>
    /// <returns>Long the Milliseconds of gamePlay.</returns>
    public long GetScore()
    {
        return currTime - start + prePausedTime;
    }

    /// <summary>
    /// Gets the percent increase in the boosted amount.
    /// </summary>
    /// <returns>A float from 0 to infinity.</returns>
    public float GetBoostedAmount()
    {
        return boostedAmount;
    }
    #endregion

    #region Unity Methods

    /// <summary>
    /// Kind of a jank solution. I want to have a boolean that keeps track of whether the camera will
    /// pause on a player death. I also want to be able to change this boolean from the inspector. The
    /// issue is that only nonstatic variables are visible in the inspector but I want the pauseOnDeath
    /// bool to be static so I can use it in a static class. This method then makes the static bool
    /// equal to the nonstatic bool every time it is changed in the inpsector. 
    /// </summary>
    private void OnValidate()
    {
        
        if (forwardFollowingSpeed < 0)
        {
            forwardFollowingSpeed = 0;
        }

        if (maxJumpHeight <= 0)
        {
            maxJumpHeight = 0.01f;
        }

       
    }

    /// <summary>
    /// This method sets the time that the camera was loaded in. This time
    /// will be updated and used to increment the speed of the camera in the 
    /// fixed update method. This also gets the intial ratio of the camera 
    /// speed to the player walking speed. As the camera increases speed this 
    /// ratio will be maintained. 
    /// </summary>
    private void Start()
    {
        pauseOnDeathStatic = pauseOnDeath;
        Vector3 currentPosition = transform.position;
        currentPosition.y = initialOffset;
        transform.position = currentPosition;

        if (debuggingMode)
        {
            difficultyLevel = difficulty;
            SetDifficulties();
        }
        if (!debuggingMode)
        {
            difficulty = difficultyLevel;
        } else
        {
            Debug.LogError("Camera in debugging mode");
        }
        SetDifficulties();
        originalSpeed = scrollingSpeed;
        target = player.GetComponent<Transform>();
        targetBody = player.GetComponent<Rigidbody>();
        playerClass = target.gameObject.GetComponent<Player>();
        start = currTime = lastIncrement = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        playerCameraDifference = playerClass.GetWalkingSpeed() - scrollingSpeed * 100;
        
    }

    /// <summary>
    /// If the paused boolean is false then it will move the camera to the right the fixed amount 
    /// that is provided in the inspectory. This will also move the prefab killer because it is 
    /// a child of the camera. 
    /// </summary>
    private void FixedUpdate()
    {
        if (increaseScrollSpeed)
        {
            long currentTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            long difference = currentTime - lastIncrement;
            if (difference > MILLISECONDS_IN_SECOND * secondsPerIncrement)
            {
                lastIncrement = currentTime;
                IncrementSpeed(percentIncrease);
                smoothSpeed *= (percentIncrease + 1); 
            }
        }

        if (scroll && (!pause && !forcedPause))
        {
            Vector3 lastPos = transform.position;
            transform.position = new Vector3(lastPos.x + scrollingSpeed, lastPos.y, lastPos.z);

        } else
        {
            playerClass.ResetWalkingSpeed();
        }
    }

    /// <summary>
    /// At the end of the updatae, after the player has made movement info it will call this method. 
    /// This will move the camera in the vertical axis. It will follow the player using the follow
    /// speed specified in the inspector. There is an additional offset that is in the inpsector. 
    /// If the player hieght is less than the max jump height then the camera will follow the player.
    /// The follow will be using the normalized value of the player height and max jump height. 
    /// Additionally if the player height is greater than the max jump height the new camera height
    /// will be the old camera height (no change). This is to keep the floor visible for composition 
    /// reasons. 
    /// </summary>
    private void LateUpdate()
    {
        if (!pause)
        {
            Vector3 lastPos = transform.position, newPosition;
            float targetHeight = target.position.y, offset, yTarget, xTarget;

            offset = initialOffset * offsetCurve.Evaluate(targetHeight / maxJumpHeight);
            yTarget = targetHeight < maxJumpHeight ? targetHeight + offset : lastPos.y;


           /*
            *  target.position.x must be greater than the last x position to 
            *  prevent camera going backwards. It will only follow if the difficulty level is 1 or less.
            *  Otherwise it will not follow (because 2 and above the camera speed increases so it feels
            *  like player cant move if follow is on). 
            */
            if (targetBody.velocity.x > forwardFollowingSpeed 
                && target.position.x > lastPos.x && difficultyLevel < 2) 
            {
                xTarget = target.position.x;
            } else
            {
                xTarget = lastPos.x;
            }

            newPosition = new Vector3(xTarget, yTarget, lastPos.z); ;
            transform.position = Vector3.SmoothDamp(transform.position, newPosition, ref velocity, smoothSpeed);
            currTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        }

    }
    #endregion

    #region Private Methods

    /// <summary>
    /// This method increases the camera speed. The paremeter is a float 
    /// between 0 and 1 and the scroll speed increments by 1 + percent. This 
    /// method will not increase the scrolling speed past the specified max camera speed.
    /// </summary>
    /// <param name="percent">float the amount the speed increases. A value from 0 to 1</param>
    private void IncrementSpeed(float percent)
    {
        
        float newSpeed = scrollingSpeed * (1 + percent);
        if (newSpeed > MAX_CAMERA_SPEED)
        {
            return;
        }
        scrollingSpeed = newSpeed;
        float newPlayerSpeed = newSpeed * 100 + playerCameraDifference;
        playerClass.SetWalkingspeed(newPlayerSpeed);
        boostedAmount = (newSpeed - originalSpeed) / originalSpeed;
    }

    /// <summary>
    /// Method that validates the difficulty parameter. It will return a value between 0 and 3.
    /// If the input is above 3 or below 0 it will return the closest bounding number.
    /// </summary>
    /// <param name="difficulty">integer to check</param>
    /// <returns>valide int between 0 and 3</returns>
    private static int ValidateDifficulty(int difficulty)
    {
        if (difficulty > 3)
        {
            return 3;
        }
        if (difficulty < 0)
        {
            return 0;
        }
        return difficulty;
    }

    /// <summary>
    /// This method sets up the difficulties based on the static difficulty level. 
    /// If the level is 0 then the increase scroll speed will be false and the scrolling speed will
    /// be 0.028. For every other case it will be as written in the code. 
    /// </summary>
    private void SetDifficulties()
    {
        switch(difficultyLevel)
        {
            case 0:
                increaseScrollSpeed = false;
                scrollingSpeed = 0.04f;
                break;
            case 1:
                increaseScrollSpeed = false;
                scrollingSpeed = 0.07f;
                break;
            case 2:
                increaseScrollSpeed = true;
                scrollingSpeed = 0.04f;
                percentIncrease = 0.2f;
                secondsPerIncrement = 6;
                break;
            default:
                increaseScrollSpeed = true;
                scrollingSpeed = 0.04f;
                percentIncrease = 0.2f;
                secondsPerIncrement = 4;
                break;
        }
    }
    #endregion

}
