using System;
using UniRx;
using UnityEngine;

public class FishMovement : MonoBehaviour
{
    #region Settings
    [SerializeField] private float maxSpeed;
    [SerializeField] private float speedAfterDash;
    [SerializeField] private float directionChangeSpeed;
    [Space]
    [SerializeField] private float acceleration;
    [SerializeField] private float airDeceleration;
    [SerializeField] private float decelerationOnDash;
    [SerializeField] private float decelerationOnStop;
    [Space] 
    [SerializeField] private float displayAngleChangeSpeed;
    [SerializeField] private Transform sprite;
    [Space] 
    [SerializeField] private float waterLevel;
    #endregion
    private readonly Subject<Vector2> _dashed = new Subject<Vector2>();
    private Camera _mainCamera;
    private Vector2 _currentVelocity;
    private float _displayAngle;

    public Vector2 AdditionalSpeed { get; set; }
    public IObservable<Vector2> Dashed => _dashed;

    private void Awake()
    {
        _mainCamera = Camera.main;
    }

    private void Update()
    {
        var inputDirection = (_mainCamera.ScreenToWorldPoint(Input.mousePosition) - transform.position).normalized;
        ProcessAngle(inputDirection);
        ProcessVelocity(inputDirection);
        ProcessDash(inputDirection);
        ProcessDisplayAngle();
        sprite.rotation = Quaternion.Euler(0, 0, _displayAngle);
        transform.position += (Vector3)((_currentVelocity + AdditionalSpeed) * Time.deltaTime);
    }
    private void ProcessVelocity(Vector2 inputDirection)
    {
        if (transform.position.y > waterLevel)
        {
            _currentVelocity += Vector2.down * (airDeceleration * Time.deltaTime);
        }
        else if (inputDirection.magnitude == 0)
        {
            if (_currentVelocity.magnitude > decelerationOnStop * Time.deltaTime)
            {
                _currentVelocity -= _currentVelocity.normalized * (decelerationOnStop * Time.deltaTime);
            }
            else
            {
                _currentVelocity = Vector2.zero;
            }
        }
        else if (_currentVelocity.magnitude > maxSpeed)
        {
            _currentVelocity -= _currentVelocity.normalized * (decelerationOnDash * Time.deltaTime);
        }
        else if (_currentVelocity.magnitude < maxSpeed)
        {
            _currentVelocity += inputDirection * (acceleration * Time.deltaTime);
        }
    }
    private void ProcessAngle(Vector2 inputDirection)
    {
        if(transform.position.y > waterLevel) return;
        var angle = Vector2.SignedAngle(_currentVelocity, inputDirection);
        // if (Mathf.Abs(angle) >= .1f && Mathf.Abs(angle) <= directionChangeSpeed * Time.deltaTime)
        // {
        //     _currentVelocity = inputDirection * _currentVelocity.magnitude;
        // }
        // else 
        if (Mathf.Abs(angle) > directionChangeSpeed * Time.deltaTime)
        {
            _currentVelocity = Vector3.RotateTowards(_currentVelocity, inputDirection,
                directionChangeSpeed * Mathf.Deg2Rad * Time.deltaTime, 0f);
        }
    }
    private void ProcessDash(Vector2 inputDirection)
    {
        if(transform.position.y > waterLevel) return;
        if (Input.GetMouseButtonDown(0))
        {
            _currentVelocity += inputDirection * speedAfterDash;
            _currentVelocity = Vector2.ClampMagnitude(_currentVelocity, speedAfterDash * 2);
            _dashed.OnNext(inputDirection);
        }
    }
    private void ProcessDisplayAngle()
    {
        if(_currentVelocity.magnitude == 0f) return;

        var angle = Vector2.SignedAngle(sprite.right, _currentVelocity);
        if (Mathf.Abs(angle) <= displayAngleChangeSpeed * Time.deltaTime)
        {
            _displayAngle = Vector2.SignedAngle(Vector2.right, _currentVelocity);
        }
        else
        {
            _displayAngle += Mathf.Sign(angle) * displayAngleChangeSpeed * Time.deltaTime;
        }
    }
}