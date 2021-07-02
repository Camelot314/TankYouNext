using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    #region Public Methods
    /// <summary>
    /// Quits the game when called.
    /// </summary>
    public void Quit()
    {
        if (!Level.IsWebMode())
        {
            Debug.Log("Quitting");
            Application.Quit();
        }
    }

    /// <summary>
    /// Starts the game by loading the next scene in the queue. The queue is 
    /// established in the build settings.
    /// </summary>
    public void Play()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
    #endregion

    #region Unity Methods
    /// <summary>
    /// Makes the cursor visible.
    /// </summary>
    private void Start()
    {
        Cursor.visible = true;
    }
    #endregion
}
