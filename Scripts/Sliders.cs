using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Sliders : MonoBehaviour
{
    #region Instance Variables. 
    [SerializeField] Slider volumeSlider, difficultySlider;     // Reference to the sliders
    #endregion

    #region Public Methods

    /// <summary>
    /// Method called when the volume slider is changed. It calls the audio
    /// Change volume method using the value from the volume slider.
    /// </summary>
    public void ChangeVolume()
    {
        Audio.ChangeVolume(volumeSlider.value);
    }

    public void ChangeDifficulty()
    {
        CameraScript.ChangeDifficulty((int) difficultySlider.value);
    }
    #endregion

    #region Unity Methods

    /// <summary>
    /// On start it makes the values of both the volume and difficulty sliders
    /// to the value of corresponding to static variables. 
    /// </summary>
    private void Start()
    {
        volumeSlider.value = Audio.StaticGetVolume();
        if (difficultySlider != null)
        {
            difficultySlider.value = CameraScript.GetDifficulty();
        }
    }

    #endregion
}
