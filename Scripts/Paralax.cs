using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Paralax : MonoBehaviour
{
    [SerializeField] private GameObject cam;
    [SerializeField] [Range(0, 1)] private float parallaxEffect;

    private float length, startPos, startY;

    // Start is called before the first frame update
    void Start()
    {
        startPos = transform.position.x;
        startY = transform.position.y;
        length = GetComponent<SpriteRenderer>().bounds.size.x;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        float distance = cam.transform.position.x * parallaxEffect;
        float temp = cam.transform.position.x * (1 - parallaxEffect);
        if (temp > startPos + length)
        {
            startPos += length;
        } else if (temp < startPos - length)
        {
            startPos -= length;
        }

        transform.position = new Vector3(
            startPos + distance, 
            startY, 
            transform.position.z
            );
    }
}
