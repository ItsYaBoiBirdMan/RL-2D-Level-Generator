using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuzzSawController : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 360f; 
    [SerializeField] private bool clockwise = true;

    void Update()
    {
        float direction = clockwise ? -1f : 1f;

        transform.Rotate(0f, 0f, direction * rotationSpeed * Time.deltaTime);
    }
}
