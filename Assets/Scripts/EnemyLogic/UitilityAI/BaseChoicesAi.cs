using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseChoicesAi
{
    protected float _percent;

    public BaseChoicesAi(float percent)
    {
        _percent = percent;
    }

    virtual public float CalculatePoints(GameObject player, Transform thisEnemyTransform, float rnnWeight)
    {
        return 0;
    }

    virtual public void UpdateMovement(SimpleMovement simpleMovement, GameObject player)
    {
        
    }
}
