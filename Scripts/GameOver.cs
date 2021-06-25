using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GameOver : MonoBehaviour
{

    #region Private Enum
    private enum State { Score, Difficulty }            // Enumerator to have same class do 2 things. 
    #endregion

    #region Instance Varables
    [SerializeField] private Score hud;                 // Reference to the HUD script so it can get the score.
    [SerializeField] private State state;               // The enumerator dictates what it will check when generating the screen.
    
    private TextMeshProUGUI textMesh;                   // Reference to the textMesh so it can update the text with the score
    private string displayText;                         // Text to display.
    #endregion

    #region Unity Methods
    // Start is called before the first frame update
    void Start()
    {
        textMesh = GetComponent<TextMeshProUGUI>();
        if (state == State.Score)
        {
            long score = hud.GetScore();
            displayText = "Score: " + score;
        } else
        {
           displayText = "Difficulty: " + CameraScript.GetDifficulty();
        }

        textMesh.text = displayText;
    }
    #endregion
}
