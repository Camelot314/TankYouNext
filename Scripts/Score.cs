using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Score : MonoBehaviour
{
    #region InstanceMethods
    [SerializeField] private CameraScript cameraScript; // Reference to the main camera script (done in inspector)
    [SerializeField] private int scoreDivider = 1;      // The score given by the camera is milliseconds of gamePlay
                                                        //      This is the factor that the time will be divided by to get the score.

    private int incrementsPerSecond;                    // Increments in a second. This is used to create the boosted score when the camera is faster
    private TextMeshProUGUI textMesh;                   // Reference to the textMesh to update the visible score.
    private long millisecondsOfGamePlay, lastIncrement; // The amount of milliseconds of gameplay that has passed. The last time there was an increment
                                                        //      This is used when the camera speed is changing. 
    

    private long score, addedScore = 0;                 // This is the current score and addedScore is the amount of
                                                        //      bonus score added as the camera is speeding up.
    #endregion

    #region Public Methods

    /// <summary>
    /// Getter for the score.
    /// </summary>
    /// <returns>long that is the current score in the HUD</returns>
    public long GetScore()
    {
        return score;
    }
    #endregion

    #region Unity Methods

    /// <summary>
    ///  Makes sure the score divider is always one or above. 
    /// </summary>
    private void OnValidate()
    {
        if (scoreDivider < 1)
        {
            scoreDivider = 1;
        }
    }


    /// <summary>
    /// Sets up the reference to the textMesh as well as the incrementsPerSecond. 
    /// </summary>
    private void Start()
    {
        textMesh = GetComponent<TextMeshProUGUI>();
        incrementsPerSecond = 1000 / scoreDivider;
        lastIncrement = cameraScript.GetScore();
    }

    /// <summary>
    ///  Will update the current score to the milliseconds of gamePlay / score divider. 
    /// </summary>
    private void Update()
    {
        millisecondsOfGamePlay = cameraScript.GetScore();
        score = CalcScore();
        textMesh.text = "Score: " + score;
    }
    #endregion

    #region Private Methods

    /// <summary>
    /// If it has been at least one increment (determined by the amount of milliseconds per incrment)
    /// Then the added score will be adjusted and the returned score will be the milliseconds of gameplay
    /// + the addedScore for a faster camera. 
    /// </summary>
    /// <returns>The total score both added and of gamePlay</returns>
    private long CalcScore()
    {
        
        if (millisecondsOfGamePlay - lastIncrement > 1000 / incrementsPerSecond)
        {
            addedScore += (int) (cameraScript.GetBoostedAmount() * 10);
            lastIncrement = millisecondsOfGamePlay;
        }
        return (millisecondsOfGamePlay / scoreDivider) + addedScore;
    }
    #endregion
}
