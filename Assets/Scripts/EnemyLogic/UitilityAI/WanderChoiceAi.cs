using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WanderChoiceAi : BaseChoicesAi
{
    private float _rangeMin = 10f;
    private float _rangeMax = 20f;

    public WanderChoiceAi(float percent) : base(percent) 
    {
        
    }

    public override float CalculatePoints(GameObject player, Transform thisEnemyTransform, float rnnWeight)
    {
        float distance = Vector3.Distance(player.transform.position, thisEnemyTransform.position);

        if(distance < _rangeMin)
        {
            return 0;
        }

        if (distance > _rangeMax)
            return 0;

        if(_percent == 1) distance = Mathf.Lerp(0, _percent, distance/10f);
        else distance = Mathf.Lerp(0, 1 - _percent, distance/10f);

        return distance * _percent * rnnWeight;
    }

    private const float MAXANGLE = 90 * Mathf.Deg2Rad;

    private const float MAXPOINTTIME = 1f;
    private float _wanderToPointTime = MAXPOINTTIME;
    public override void UpdateMovement(SimpleMovement simpleMovement, GameObject player)
    {
        Vector3 newAngle = new Vector3(Random.Range(-MAXANGLE, MAXANGLE), 0, Random.Range(-MAXANGLE, MAXANGLE));

        if (_wanderToPointTime > MAXPOINTTIME)
        {
            float distanceToTravel = 5;
            simpleMovement.Target = simpleMovement.gameObject.transform.position + newAngle * distanceToTravel;
            _wanderToPointTime = 0;
        }   

        _wanderToPointTime += Time.deltaTime;
    }
}
