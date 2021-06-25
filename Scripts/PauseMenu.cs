using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    #region Instance Vairables
    [SerializeField] private Level level;
    #endregion

    #region Public Methods
    /// <summary>
    /// Quits the game when called.
    /// </summary>
    public void Quit()
    {
        Debug.Log("Quitting");
        Application.Quit();
    }

    /// <summary>
    /// Starts the game by loading the next scene in the queue. The queue is 
    /// established in the build settings.
    /// </summary>
    public void Play()
    {
        level.Pause(false);
    }

    /// <summary>
    /// Rebuilds the game scene so it can restart the game when play again is called.
    /// </summary>
    public void PlayAgain()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        CameraScript.ForcePaused(false);
        Enemy.Freeze(false);
    }

    /// <summary>
    /// This is called by the main menu button and it will load in the main menu scene.
    /// This will remove all data from the current ongoing game. 
    /// </summary>
    public void MainMenu()
    {
        SceneManager.LoadScene(0);
        CameraScript.ForcePaused(false);
        Enemy.Freeze(false);
    }
    #endregion
}
