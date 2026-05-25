using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IsGroundedManager : MonoBehaviour
{
    [SerializeField] private PlayerController PlayerController;

    private Vector2 _contactPoint;
    private Vector2 _newRespawnPoint;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Ground"))
        {
            PlayerController.SetAsGrounded();
            _contactPoint = other.ClosestPoint(transform.position);
            _newRespawnPoint = new Vector2(_contactPoint.x, _contactPoint.y + 2f);
            PlayerController.UpdateRespawnPoint(_newRespawnPoint);
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Ground"))
        {
            _contactPoint = other.ClosestPoint(transform.position);
            _newRespawnPoint = new Vector2(_contactPoint.x, _contactPoint.y + 2f);
            PlayerController.UpdateRespawnPoint(_newRespawnPoint);
        }
        
        
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Ground")) PlayerController.SetAsInAir();
    }
}