using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupController : MonoBehaviour
{
    [SerializeField] private float bobHeight = 0.2f;
    [SerializeField] private float bobSpeed = 2f;

    private Vector3 _startPos;

    private void Start()
    {
        _startPos = transform.localPosition;
    }

    private void Update()
    {
        float offset = Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.localPosition = new Vector3(_startPos.x, _startPos.y + offset, _startPos.z);
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Help Restored");
            Destroy(gameObject);
        }   
    }
    
    
}
