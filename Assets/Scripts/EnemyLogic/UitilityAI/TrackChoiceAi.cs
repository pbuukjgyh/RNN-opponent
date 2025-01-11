using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackChoiceAi : BaseChoicesAi
{
    private float _range = 20f;
    public TrackChoiceAi(float percent) : base(percent)
    {
    }

    public override float CalculatePoints(GameObject player, Transform thisEnemyTransform, float rnnWeight)
    {
        float distanceToPlayer = Vector3.Distance(player.transform.position, thisEnemyTransform.position);

        //if the player is close, don't track
        if (distanceToPlayer < _range)
            return 0f;

        BoxCollider lastPlayerArea = AreaManager.Instance.GetLastPlayerArea();

        if (lastPlayerArea == null)
        {
            return 0f;
        }

        float distanceToArea = Vector3.Distance(thisEnemyTransform.position, lastPlayerArea.bounds.center);

        // Higher points for closer to area than to player
        return Mathf.Clamp01(distanceToPlayer / distanceToArea) * _percent * rnnWeight;
    }

    public override void UpdateMovement(SimpleMovement simpleMovement, GameObject player)
    {
        BoxCollider lastPlayerArea = AreaManager.Instance.GetLastPlayerArea();

        if (lastPlayerArea != null)
        {
            Vector3 targetPosition = lastPlayerArea.bounds.center;
            simpleMovement.Target = targetPosition;
        }
    }
}
