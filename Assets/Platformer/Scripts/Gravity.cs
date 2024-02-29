using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gravity : MonoBehaviour
{
    private Vector3 _direction = Vector3.down;
    private float _multiplyer = 1;
    private float _Strength = 1.0f;
    private float _weight = 2.0f;

    private float _maxSpeed = 400;

    private float _velocity = 0;
    private Vector3 _previousLocation = Vector3.zero;

    private bool grounded = false;

    [SerializeField] private LayerMask _groundLayer;

    private void Start()
    {
        _previousLocation = this.transform.position;
        SetDirection(_direction);
    }

    private void Update()
    {
        CheckIfGrounded();
        ApplyGravity();
    }

    public void SetDirection(Vector3 dir)
    {
        float multiplyer = dir.z;
        dir.z = 0;
        _direction = dir;
        _previousLocation = transform.position;
    }

    private void CheckIfGrounded()
    {
        if (Physics.CheckBox((transform.position) - new Vector3(0, -.07f, 0), new Vector3(1, .01f, 1), Quaternion.Euler(Vector3.zero), _groundLayer))
        {
            //Debug.Log("grounded");
            grounded = true;
            _velocity = 0;
            return;
        }
        else
        {
            grounded = false;
            return;
        }
    }


    private void ApplyGravity()
    {
        if (grounded)
            return;

        Vector3 dir = (_previousLocation - transform.position) + _direction;
        //Debug.Log($"previous Location: {_previousLocation}, current location: {transform.position}, direction from list: {ConstData.DIRECTION_LIST[(int)_direction]}, dir: {dir}");
        float distance = Vector3.Distance(Vector3.zero, dir) + Vector3.Distance(_previousLocation, transform.position);
        float power = (_Strength * _multiplyer) * ((_weight) / distance);
        float acceleration = power / _weight;
        _velocity = Mathf.Min(_velocity + (acceleration * Time.deltaTime * 0.1f), _maxSpeed * Time.deltaTime * 0.1f);

        dir = (dir.normalized * _velocity);
        //do a box check here to see if the position it attempts to go to will overlap with anything
        if (Physics.CheckBox((transform.position + dir) - new Vector3 (0,-.07f,0), new Vector3(1, .01f, 1), Quaternion.Euler(Vector3.zero), _groundLayer))
        {
            Debug.Log("OverLapped");
            grounded = true;
            _velocity = 0;
            return;
        }

        _previousLocation = transform.position;
        transform.position = transform.position + dir;


    }

    public float weight() { return _weight; }
    public float velocity() { return _velocity; }
}

public static class ConstData
{
    public static readonly Vector3[] DIRECTION_LIST = new Vector3[] { Vector3.up, Vector3.right, Vector3.down, Vector3.left };
}

public enum Direction
{
    up = 0,
    right = 1,
    down = 2,
    left = 3,
}