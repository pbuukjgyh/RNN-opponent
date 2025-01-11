using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PredictChoiceAi : BaseChoicesAi
{
    private float _rangeMax = 15f;
    //so we don't predict before we need to
    private float _rangeMin = 5f;

    public PredictChoiceAi(float percent):base(percent)
    {
        
    }

    public override float CalculatePoints(GameObject player, Transform thisEnemyTransform, float rnnWeight)
    {
        float distance = Vector3.Distance(player.transform.position, thisEnemyTransform.position);
        //if we are not in range, we don't predict
        if (distance > _rangeMax || distance < _rangeMin)
        {
            return 0;
        }

        Rigidbody rigidbody = player.GetComponent<Rigidbody>();
        if (rigidbody == null) { return 0; }

        //Vector3 rightOfPlayer = new Vector3 (-rigidbody.velocity.x, rigidbody.velocity.z);
        Vector3 normilazedPos = thisEnemyTransform.position.normalized;

        //if the dot is bigger than 0 we are in front of the player the player and can stalk
        if (Vector3.Dot(rigidbody.velocity, normilazedPos) > 0)
        {
            float distanceInCircle = distance/(_rangeMax - _rangeMin) ;
            return (1 - distanceInCircle) * _percent * rnnWeight;
        }

        return 0;
    }

    private const float DISTANCE = 1f;
    public override void UpdateMovement(SimpleMovement simpleMovement, GameObject player)
    {
        Rigidbody rigidbody = player.GetComponent<Rigidbody>();
        if (rigidbody == null) { return; }

        Vector3 playerPosition = player.transform.position;
        Vector3 playerVelocity = rigidbody.velocity;

        Vector3 normilazedPos = simpleMovement.transform.position.normalized;

        Vector3 targetPos;
        if (Vector3.Dot(rigidbody.velocity, normilazedPos) > 0 || rigidbody.velocity == Vector3.zero)
        {
            // Calculate a strategic cut-off point on the map
            targetPos = CalculateStrategicCutOffPoint(simpleMovement.transform.position, playerPosition, playerVelocity);
            Debug.Log(targetPos);
        }
        else
        {
            // Directly follow the player
            targetPos = playerPosition + playerVelocity * DISTANCE;
        }

        simpleMovement.Target = targetPos;
    }

    /// Calculates a strategic cut-off point that forces a detour.
    private Vector3 CalculateStrategicCutOffPoint(Vector3 ourPosition, Vector3 playerPosition, Vector3 playerVelocity)
    {
        Vector3 futurePlayerPos = playerPosition + playerVelocity * DISTANCE;

        // Calculate a detour point that is offset from the player's trajectory
        Vector3 directionToFuture = (futurePlayerPos - ourPosition).normalized;

        // Perpendicular vector for offset
        Vector3 perpendicular;
        if (playerPosition.z < 15) perpendicular = Vector3.Cross(playerVelocity.normalized, Vector3.up).normalized;
        else perpendicular = Vector3.Cross(playerVelocity.normalized, Vector3.down).normalized;

        float detourDistance = 5.0f;
        Vector3 detourPoint = futurePlayerPos + perpendicular * detourDistance;

        // Ensure the detour point is farther away from the AI than the player
        if (Vector3.Distance(ourPosition, detourPoint) < Vector3.Distance(ourPosition, futurePlayerPos))
        {
            detourPoint += perpendicular * detourDistance; // Double the offset if too close
        }

        return detourPoint;
    }
}
