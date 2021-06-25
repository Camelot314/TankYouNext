using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefabKiller : MonoBehaviour
{
    #region Instance Variables
    [SerializeField] private GameObject levelGameObject;

    private const float OFFSET = 0.0118f;
    private Level level;
    private Vector3 originalPosition;
    #endregion

    /// <summary>
    /// Intializes the level object reference and the original position and offset. 
    /// </summary>
    private void Start()
    {
        originalPosition = transform.position;
        originalPosition.y = originalPosition.y + OFFSET;
        level = levelGameObject.GetComponent<Level>();
    }

    /// <summary>
    /// This method is called when something collides with the prefab killer. It will
    /// destroy the object and all its children if the object is not tagged with player or unbreakable. 
    /// The Floor is also destroyed in a special way so it will not destroy the floor.
    /// </summary>
    /// <param name="other">Collider of the object that collides with the prefabKiller</param>
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Unbreakable") && !other.CompareTag("Player") && !other.CompareTag("Floor"))
        {
            Transform parentTransfrom = other.transform.parent;
            bool destroyPartent = 
                parentTransfrom != null && !parentTransfrom.CompareTag("Unbreakable") 
                && !parentTransfrom.CompareTag("Player");

            if (destroyPartent)
            {
                Destroy(parentTransfrom.gameObject);
            } else
            {
                Destroy(other.gameObject);
            }

        }
    }

    /// <summary>
    /// This is the method that destroys the floor. This method is called when the prefab killer
    /// loses contact with the floor which ensures that the floor is not destroyed prematurely while
    /// the player is still on it. It also will only destory the floor if the two floors boolean is
    /// true. This is to stop the killer from destorying the one and only floor. 
    /// </summary>
    /// <param name="other">Collider of the object that leaves the prefab killer</param>
    private void OnTriggerExit(Collider other)
    {
        
        if (level.HasTwoFloors() && other.CompareTag("Floor"))
        {
            Destroy(other.gameObject);
            level.SetTwoFloors(false);
        }
    }

}
