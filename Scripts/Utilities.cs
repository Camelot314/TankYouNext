using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class Utilities
{
    #region Static Variables.
    private const float ERROR_MARGIN = 0.0001f;
    #endregion

    #region Static Methods
    /// <summary>
    /// Will return true if the provided floats are within 0.0001 of each other.
    /// </summary>
    /// <param name="a">First float</param>
    /// <param name="b">Second float</param>
    /// <returns>true if they are essentially equal</returns>
    public static bool Equals(float a, float b)
    {
        return Math.Abs(a - b) < ERROR_MARGIN;
    }

    /// <summary>
    /// Will return true if the 2 floats are within the given range. 
    /// </summary>
    /// <param name="a">first float</param>
    /// <param name="b">second float</param>
    /// <param name="range">range between the 2 values</param>
    /// <returns>true if the 2 values are within the range. </returns>
    public static bool Within(float a, float b, float range)
    {
        return Math.Abs(a - b) < range;
    }


    /// <summary>
    /// This method checks a component array and will return true if the array contains
    /// an object of the tag specified. Otherwise it will return false. 
    /// </summary>
    /// <param name="array">array of componenets to check from</param>
    /// <param name="tag">tag to compare to</param>
    /// <returns>true if the tag is in one of the componenets in the array</returns>
    public static bool ContainsTag(Component[] array, string tag)
    {
        foreach (Component component in array)
        {
            if (component.CompareTag(tag))
            {
                return true;
            }
        }
        return false;
    }
    #endregion
}
