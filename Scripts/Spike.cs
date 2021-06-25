using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spike : MonoBehaviour
{

    #region Unity Methods

    private void OnCollisionEnter(Collision collision)
    {
        
        if (collision.gameObject.CompareTag("Player"))
        {
            Level.SetEnd(true);
            CameraScript.ForcePaused(true);
        }
        
    }
    #endregion
}
