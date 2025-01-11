using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using UnityEngine.AI;

public class NavMeshMovement : SimpleMovement
{
    private NavMeshAgent _navMeshAgent;

    private Vector3 _previosTargetPos = Vector3.zero;

    protected override void Awake()
    {
        base.Awake();

        _navMeshAgent = GetComponent<NavMeshAgent>();
        _navMeshAgent.speed = _movementSpeed;

        _previosTargetPos = transform.position;
    }

    const float MOV_EPILON = .25f;

    protected override void HandleMovement()
    {
        if (_target == null)
        {
            _navMeshAgent.isStopped = true;
            return;
        }

        if ((_target - _previosTargetPos).sqrMagnitude > MOV_EPILON)
        {
            _navMeshAgent.SetDestination(_target);
            _navMeshAgent.isStopped = false;
            _previosTargetPos = _target;
        }
    }

}
