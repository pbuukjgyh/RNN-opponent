using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChaseChoiceAi : BaseChoicesAi
{
    private Vector3 _lastPlayerPosition;
    public ChaseChoiceAi(float percent): base(percent)
    {
        
    }

    private float _range = 15f;

    public override float CalculatePoints(GameObject player, Transform thisEnemyTransform, float rnnWeight)
    {
        float distance = Vector3.Distance(player.transform.position, thisEnemyTransform.position);

        //if ((player.transform.position - _lastPlayerPosition).magnitude < 0.2f)
        //{
        //    _lastPlayerPosition = player.transform.position;
        //    return Mathf.Clamp01(distance) * _percent * rnnWeight;
        //}

        //if we are not in range, we don't chase
        if(distance > _range)
        {
            return 0;
        }

        //the distance / with range to make it between 0 and 1
        return Mathf.Clamp01(1 + distance + (_range - distance) / (_range / 0.5f)) * _percent * rnnWeight;
    }
    public override void UpdateMovement(SimpleMovement simpleMovement, GameObject player)
    {
        simpleMovement.Target = player.transform.position;
    }
}
