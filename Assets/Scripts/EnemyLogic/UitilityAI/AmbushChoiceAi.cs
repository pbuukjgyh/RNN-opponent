using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmbushChoiceAi : BaseChoicesAi
{
    private float _range = 15f;
    private float _rangeMax = 20f;
    public AmbushChoiceAi(float percent) : base(percent)
    {
    }

    public override float CalculatePoints(GameObject player, Transform thisEnemyTransform, float rnnWeight)
    {
        float distanceToPlayer = Vector3.Distance(player.transform.position, thisEnemyTransform.position);

        //if the player is too close or too far, don't ambush
        if (_rangeMax <= distanceToPlayer || distanceToPlayer <= _range)
            return 0f;

        BoxCollider mostVisitedArea = AreaManager.Instance.GetMostVisitedArea();

        if (mostVisitedArea == null)
        {
            return 0f;
        }

        float playerVisits = AreaManager.Instance.GetVisitCount(mostVisitedArea);

        if (playerVisits < 3)
            return _percent * rnnWeight;

        float distanceToArea = Vector3.Distance(thisEnemyTransform.position, mostVisitedArea.bounds.center);

        // Higher points for high visit counts and closer proximity
        return playerVisits / (distanceToPlayer + distanceToArea) * _percent * rnnWeight;
    }

    public override void UpdateMovement(SimpleMovement simpleMovement, GameObject player)
    {
        BoxCollider mostVisitedArea = AreaManager.Instance.GetMostVisitedArea();

        if (mostVisitedArea != null)
        {
            // Move towards the most visited area's center
            Vector3 targetPosition = mostVisitedArea.bounds.center;
            simpleMovement.Target = targetPosition;
        }
    }
}
