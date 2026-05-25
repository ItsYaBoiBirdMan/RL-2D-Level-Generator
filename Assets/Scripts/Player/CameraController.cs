using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform Target;
    [SerializeField] private Vector2 DeadZoneSize;
    [SerializeField] private float SmoothTime;
    [SerializeField] private Vector2 Offset;
    [SerializeField] private float LookAheadDistance;
    [SerializeField] private float LookAheadSmoothTime;

    [SerializeField] private GameObject SideScrollerPlayer;
    [SerializeField] private GameObject TopDownPlayer;

    private Vector3 _velocity;
    private Vector3 _targetPos;
    private Vector3 _currentPos;
    private Vector3 _delta;
    private Vector3 _desiredPos;
    private float _moveX;
    private float _moveY;

    private float _targetLookAheadX;
    private Rigidbody2D _targetRb;
    private float _moveDirection;
    private float _currentLookAheadX;
    private float _lookAheadVelocity;
    

    private void Start()
    {
        StartCoroutine(StartRoutine());
    }

    private IEnumerator StartRoutine()
    {
        yield return new WaitForSeconds(0.25f);
        
        if (!SideScrollerPlayer.gameObject.activeInHierarchy)
        {
            Debug.Log("Side Player off");
            Target = TopDownPlayer.transform;
        }
        
        if (!TopDownPlayer.gameObject.activeInHierarchy)
        {
            Debug.Log("Top Down Player off");
            Target = SideScrollerPlayer.transform;
        }
        
        _targetRb = Target.GetComponent<Rigidbody2D>();
    }

    private void LateUpdate()
    {
        CameraMovementWithLookAhead();
    }

    /*private void RegularCameraMovement()
    {
        if(!Target) return;

        _targetPos = Target.position + (Vector3)Offset;
        _currentPos = transform.position;

        _delta = _targetPos - _currentPos;

        if (Mathf.Abs(_delta.x) > DeadZoneSize.x / 2f) _moveX = _delta.x - Mathf.Sign(_delta.x) * (DeadZoneSize.x / 2f);
        if (Mathf.Abs(_delta.y) > DeadZoneSize.y / 2f) _moveY = _delta.y - Mathf.Sign(_delta.y) * (DeadZoneSize.y / 2f);

        _desiredPos = _currentPos + new Vector3(_moveX, _moveY, 0f);

        transform.position = Vector3.SmoothDamp(_currentPos, new Vector3(_desiredPos.x, _desiredPos.y, _currentPos.z), ref _velocity, SmoothTime);
    }*/
    
    private void CameraMovementWithLookAhead()
    {
        if(!Target) return;
        if (_targetRb)
        {
            _moveDirection = Mathf.Sign(_targetRb.velocity.x);
            if (Mathf.Abs(_targetRb.velocity.x) > 0.1f) _targetLookAheadX = _moveDirection * LookAheadDistance;
        }
        
        _currentLookAheadX = Mathf.SmoothDamp(_currentLookAheadX, _targetLookAheadX, ref _lookAheadVelocity, LookAheadSmoothTime);

        _targetPos = Target.position + (Vector3)Offset + new Vector3(_currentLookAheadX, 0f, 0f);
        _currentPos = transform.position;

        _delta = _targetPos - _currentPos;

        if (Mathf.Abs(_delta.x) > DeadZoneSize.x / 2f) _moveX = _delta.x - Mathf.Sign(_delta.x) * (DeadZoneSize.x / 2f);
        if (Mathf.Abs(_delta.y) > DeadZoneSize.y / 2f) _moveY = _delta.y - Mathf.Sign(_delta.y) * (DeadZoneSize.y / 2f);

        _desiredPos = _currentPos + new Vector3(_moveX, _moveY, 0f);

        transform.position = Vector3.SmoothDamp(_currentPos, new Vector3(_desiredPos.x, _desiredPos.y, _currentPos.z), ref _velocity, SmoothTime);
    }

    public void SnapToTarget()
    {
        if (!Target) return;

        _velocity = Vector3.zero;

        _currentLookAheadX = 0f;
        _lookAheadVelocity = 0f;

        Vector3 snapPos = Target.position + (Vector3)Offset;

        transform.position = new Vector3(snapPos.x, snapPos.y, transform.position.z);
    }
}
