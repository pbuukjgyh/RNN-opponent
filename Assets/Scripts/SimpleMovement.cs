using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleMovement : MonoBehaviour
{
    [SerializeField]
    protected float _movementSpeed = 1f;

    protected Rigidbody _rigidbody;

    protected Vector3 _desiredMoveDir = Vector3.zero;
    protected Vector3 _desiredLookatPoint = Vector3.zero;

    protected Vector3 _target;

    protected virtual void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();

        //set the startTarget on this pos so we don't emediatly go to 0,0
        _target = transform.position;
    }

    public Vector3 Desiredmovedir
    {
        get { return _desiredMoveDir; }
        set { _desiredMoveDir = value; }
    }

    public Vector3 Target
    {
        get { return _target; }
        set { _target = value; }
    }

    protected virtual void FixedUpdate()
    {
        HandleMovement();
    }

    const int SLOWDOWN = 20;
    protected virtual void HandleMovement()
    {
        if (_rigidbody == null) { return; }

        Vector3 movement = _desiredMoveDir.normalized;
        //physics have *Time.delta
        movement *= _movementSpeed;

        movement.y = 0;

        _rigidbody.velocity = movement;

        if (movement.magnitude > 0.1f) // Only rotate if there's significant movement
        {
            Vector3 direction = new Vector3(movement.x, 0, movement.z).normalized;

            if (direction != -transform.forward)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);

                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * _movementSpeed);
            }
        }

        transform.position += movement / SLOWDOWN;
    }

}
