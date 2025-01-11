using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitChoiceAi : BaseChoicesAi
{
    public WaitChoiceAi(float percent): base(percent)
    {
        
    }

    public override float CalculatePoints(GameObject player, Transform thisEnemyTransform, float rnnWeight)
    {
        float distance = Vector3.Distance(player.transform.position, thisEnemyTransform.position);

        distance = Mathf.Lerp(0, 1 - _percent, distance);

        return distance * _percent * rnnWeight;
    }

    public override void UpdateMovement(SimpleMovement simpleMovement, GameObject player)
    {
        simpleMovement.Target = simpleMovement.gameObject.transform.position;
    }
}
