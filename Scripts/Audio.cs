using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Audio : MonoBehaviour
{
    #region Variables
    #region Static Vairables
    private static float volume = 0.30f;            // Static volume amount
    private static Audio instance;                  // Instance of the audio object so that changes can
                                                    //      be made without an instance. 
    #endregion

    #region Instance Variables
    private AudioSource audioSource;                // Audio source unity object.

    #endregion
    #endregion

    #region Static Methods
    /// <summary>
    /// Static method that changes the volume. If the instance is non null
    /// then it will directly change the volume of the instance. If not then
    /// the volume will be set to the static volume on when the object is created. 
    /// </summary>
    /// <param name="volume">float the volume. Will be between 0 and 1</param>
    public static void ChangeVolume(float volume)
    {


        Audio.volume = VerifyVolume(volume);
        if (instance != null)
        {
            instance.Volume(Audio.volume);
        }

    }

    /// <summary>
    /// Returns the static volume varaible. The value is between 0 and 1. 
    /// </summary>
    /// <returns>Float volume between 0 and 1</returns>
    public static float StaticGetVolume()
    {
        return volume;
    }
    #endregion

    #region Unity Methods

    /// <summary>
    /// Sets the audio source instance variables. Sets the volume to the 
    /// static volume. 
    /// </summary>
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.volume = volume;
        instance = this;
        audioSource.volume = volume;
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Will set the new volume to the given volume. It will also change
    /// the static volume varaible. 
    /// </summary>
    /// <param name="newVolume">the new float for the volume.</param>
    /// <returns>the new set volume.</returns>
    public float Volume(float newVolume)
    {
        float verifiedVolume = VerifyVolume(newVolume);
        audioSource.volume = verifiedVolume;
        volume = verifiedVolume;
        return audioSource.volume;
    }

    /// <summary>
    /// Gets the volume of the audio source.
    /// </summary>
    /// <returns>float between 0 and 1</returns>
    public float GetVolume()
    {
        return audioSource.volume;
    }


    #endregion

    #region Private Methods

    /// <summary>
    /// Adjusts the input to a float between 0 and 1. Anything outside of 
    /// that range gets put to the nearest bound. 
    /// </summary>
    /// <param name="volume">input float</param>
    /// <returns>float between 0 and 1</returns>
    private static float VerifyVolume(float volume)
    {
        if (volume < 0)
        {
            return 0;
        }
        if (volume > 1)
        {
            return 1;
        }
        return volume;
    }
    #endregion
}
