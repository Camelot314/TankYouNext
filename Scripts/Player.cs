using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    #region Instance Variables
    [Header("Player Controls")]
    [SerializeField] private float jumpAmount, superJumpAmount;                 // The force amount that the player will jump regularly and on a superJump.
    [SerializeField] [Range(0.0001f, 20f)] private float walkingSpeed = 5f;     // The walking speed of the player. This is bounded to prevent weird edge cases.
    [SerializeField] private GameObject levelGameObj;                           // Reference to the entire level game object. 

    [Header("Collision Detection")]
    [SerializeField] private LayerMask playerMask;                              // Layer mask used by the Physics.Overlapped methods. This will not include the player layer.
                                                                                //      or the ignore collision layer.

    [SerializeField] private Transform feet, front, back, backBarier;           // Transforms for the feet, front, back, and backBarier locations. 
    [SerializeField] private Vector3 sideBoxSize, feetBoxSize;                  // Vector3 for the size of the side box and the feetHitBox for the physics.Overlapped


    [Header("Debugging Jumping")]
    [SerializeField] private bool debug;
    [SerializeField] private Material[] colors;
    [SerializeField] private GameObject feetObject;
    private Renderer feetRenderer;
    

    private Level level;                                                // Reference to the instance of the script attached to the level gameObject. 
    private Rigidbody body;                                             // RigidBody of the player attatched to this script.
    private bool jumpPressed;                                           // Bool that keeps track of whether the jump key was pressed (to prevent sticky keys).

    private float horizontalMovment, originalWalingSpeed;               // Float to keep track of how much to move horizontally (from input.GetAxis). Additional float
                                                                        //      to keep track of the original player speed so that the player doesn't go 
                                                                        //      crazy when the game is paused or over. 

    private Vector3 sideBoxHalfSize, feetBoxHalfSize;                   // Size of the hitbox for the side and feet.
   
    private Quaternion orientation = new Quaternion(0, 0, 0, 1);        // Quarernion for the orientation used by Physics.OverlappedBox().
    #endregion

    #region Unity Methods
    /// <summary>
    /// Validates the jump amount parameter.
    /// </summary>
    private void OnValidate()
    {
        if (jumpAmount < 0)
        {
            jumpAmount = 0;
        }

    }
 
    // Start is called before the first frame update
    private void Awake()
    {
        sideBoxHalfSize = sideBoxSize / 2;
        feetBoxHalfSize = feetBoxSize / 2;
        body = gameObject.GetComponent<Rigidbody>();
        level = levelGameObj.GetComponent<Level>();
        originalWalingSpeed = walkingSpeed;
    }

    private void Start()
    {
        if (debug)
        {
            feetRenderer = feetObject.GetComponent<Renderer>();
            feetRenderer.enabled = true;
            feetRenderer.sharedMaterial = colors[0];
        }
    }

    /// <summary>
    /// Update is called once per frame
    /// the horizontal movement is directly off the input.GetAxis. 
    /// the jump is either true or false depending on whether the input axis is close enough
    /// to 1. It will then return true. This info is used by fixed update that checks 
    /// if the player is floating ( then it will make the jumped press false if not then it will
    /// jump and make jumpPressed false).
    /// </summary>
    void Update()
    {
        jumpPressed = Utilities.Equals(1, Input.GetAxis("Jump"));
        horizontalMovment = Input.GetAxis("Horizontal");
    }


    /// <summary>
    /// The floating bool keeps track of whether or no the player is currentlly in the air.
    /// It does this by checking an overalapped sphere at the feet of the player and uses
    /// the player mask to determine if it has any overlapped.
    /// The hit front and hit back are done in the same way using an overlapped box. 
    /// </summary>
    private void FixedUpdate()
    {
        Collider[] groundArray, frontArray, backArray;

        groundArray = Physics.OverlapBox(feet.position, feetBoxHalfSize, orientation, playerMask);
        frontArray = Physics.OverlapBox(front.position, sideBoxHalfSize, orientation, playerMask);
        backArray = Physics.OverlapBox(back.position, sideBoxHalfSize, orientation, playerMask);

        bool floating = groundArray.Length == 0, doneJumping = Utilities.Equals(body.velocity.y, 0);
        bool hitFront = frontArray.Length != 0, hitBack = backArray.Length != 0;

        body.velocity = new Vector3(horizontalMovment * walkingSpeed,
           body.velocity.y, 0);

        if (debug)
        {
            if (floating)
            {
                feetRenderer.sharedMaterial = colors[1];
            } else
            {
                feetRenderer.sharedMaterial = colors[2];
            }
        }


        if (!floating && Utilities.ContainsTag(groundArray, "Enemy"))
        {
            SuperJump(groundArray);
        } else if (!floating && jumpPressed && doneJumping)
        {
            body.AddForce(Vector3.up * jumpAmount, ForceMode.Force);
            jumpPressed = false;
        }

        if (hitFront)
        {
            HitFront();
        }

        if (hitBack)
        {
            HitBack();
           hitBack = Utilities.ContainsTag(backArray, "CameraBound");
        }
        if (hitFront && hitBack)
        {
            level.EndGame();
            level.PauseCamera(true);
        }
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Will make the player kinematic so it will freeze when the 
    /// pause and game over menus are displayed. 
    /// </summary>
    /// <param name="freeze">player frozen if true.</param>
    public void FreezePlayer(bool freeze)
    {
        body.isKinematic = freeze;
    }


    /// <summary>
    /// This method returns the player walking speed. 
    /// </summary>
    /// <returns>float the player walking speed</returns>
    public float GetWalkingSpeed()
    {
        return walkingSpeed;
    }

    /// <summary>
    /// This method sets the walking speed for the player.
    /// If the parameter walking speed is 0 or less nothing will happen. 
    /// </summary>
    /// <param name="walkingSpeed">float the speed to set. </param>
    public void SetWalkingspeed(float walkingSpeed)
    {
        if (walkingSpeed > 0)
        {
            this.walkingSpeed = walkingSpeed;
        }
    }

    /// <summary>
    /// Resets the player walking speed to the value that it was when the game started.
    /// </summary>
    public void ResetWalkingSpeed()
    {
        walkingSpeed = originalWalingSpeed;
    }

    #endregion

    #region Private Methdos
    /// <summary>
    /// Method that is called when something hits the front of the player. 
    /// </summary>
    private void HitFront()
    {

    }

    /// <summary>
    /// Method that is called when something hits the back of the player.
    /// </summary>
    private void HitBack()
    {
    
    }

    /// <summary>
    /// Super jump method. This is called when the player lands on an enemy
    /// and is granted a super jump. After jumping on the enemy the object
    /// is destroyed. This method loops through an array of colliders to find
    /// the one with the enemy. This is an additional loop that is on top of 
    /// the contains tag aray. 
    /// </summary>
    /// <param name="array">array of colliders</param>
    private void SuperJump(Collider[] array)
    {
        foreach (Collider collider in array)
        {
            if (collider.CompareTag("Enemy"))
            { 
                Destroy(collider.gameObject);
            }
        }
        body.AddForce(Vector3.up * superJumpAmount, ForceMode.Force);
    }
    #endregion
}
