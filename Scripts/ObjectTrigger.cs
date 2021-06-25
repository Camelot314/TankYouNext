using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectTrigger : MonoBehaviour
{
    #region Instance Variables
    [SerializeField] private bool debug;
    [SerializeField] private Material[] colors;

    private Renderer rend;
    private Vector3 objectPosition;             // The position of the current objectTrigger that the script is attached to.
    #endregion

    #region Unity methods

    /// <summary>
    /// Initializes the instance of the transfrom to the transform of the gameObject
    /// associated with the class instance. 
    /// </summary>
    private void Awake()
    {
        objectPosition = gameObject.transform.position;
    }

    private void Start()
    {
        if (debug)
        {
            rend = GetComponent<Renderer>();
            rend.enabled = true;
            rend.sharedMaterial = colors[0];
        }
        
    }



    /// <summary>
    /// This method is called when the floor action object that is attached to the player hits it. 
    /// It will then called the level reference and make an object at the current trigger position. 
    /// Finally it will destroy the trigger. 
    /// </summary>
    /// <param name="other">Collider of the object that activates the trigger</param>
    private void OnTriggerEnter(Collider other)
    {
        if (debug)
        {
            rend.sharedMaterial = colors[1];
        }
        if (other.CompareTag("FloorAction"))
        {
            Level.GetInsance().MakeObject(objectPosition);
            Destroy(gameObject);
        }
    }
    #endregion
}
