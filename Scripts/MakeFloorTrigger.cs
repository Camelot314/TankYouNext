using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MakeFloorTrigger : MonoBehaviour
{
    #region Instance Varaibles
    [SerializeField] private bool debug;
    [SerializeField] private Material[] colors;
    private Renderer rend;
    
    private bool disabled;                                  // Keeps track whether to enable this script. If it has already used
                                                            //      then it will become disabled to prevent 2 floors from being placed in same
                                                            //      location. 
    #endregion

    #region Unity Methods
    // Start is called before the first frame update
    void Start()
    {
        if (debug)
        {
            rend = GetComponent<Renderer>();
            rend.enabled = true;
            rend.sharedMaterial = colors[1];
        }
        
        
    }

    /// <summary>
    /// This method is called when an object passes through the floor trigger gameObject.
    /// This will cause the extender class to make a new floor. 
    /// </summary>
    /// <param name="other">Collider of the object that passes through the trigger.</param>
    private void OnTriggerEnter(Collider other)
    {
        
        if (other.CompareTag("FloorAction") && !disabled)
        {
            Level.GetInsance().ExtendFloor();
            disabled = true;
            if (debug)
            {
                rend.sharedMaterial = colors[2];
            }
        }
    }
    #endregion
}
