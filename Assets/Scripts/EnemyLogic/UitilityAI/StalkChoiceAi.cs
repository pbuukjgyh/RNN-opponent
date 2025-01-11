using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StalkChoiceAi : BaseChoicesAi
{
    private float _rangeMax = 3f;
    private float _rangeMin = 1f;

    private float _timeStalked;
    private const float MAXTIMESTALKED = 5f;

    public StalkChoiceAi(float percent): base(percent)
    {
        
    }

    public override float CalculatePoints(GameObject player, Transform thisEnemyTransform, float rnnWeight)
    {
        float distance = Vector3.Distance(player.transform.position, thisEnemyTransform.position);
        //if we are not in range, we don't stalk and reset our timer
        if (distance > _rangeMax || distance < _rangeMin)
        {
            _timeStalked = 0;
            return 0;
        }

        Rigidbody rigidbody = player.GetComponent<Rigidbody>();
        if (rigidbody == null) { return 0; }

        //Vector3 rightOfPlayer = new Vector3 (-rigidbody.velocity.x, rigidbody.velocity.z);
        Vector3 normilazedPos = thisEnemyTransform.position.normalized;

        //if the dot is smaller than 0 we are behind the player and can stalk or if the player is standing still
        if(Vector3.Dot(rigidbody.velocity,normilazedPos) < 0 || rigidbody.velocity == Vector3.zero)
        {
            return (1 - distance / _rangeMax - _timeStalked / MAXTIMESTALKED) * _percent * rnnWeight;
        }

        return 0;
    }

    private const float DISTANCE = 1.55f;
    private Vector3 lastVelocity = new Vector3(1,0,1);

    public override void UpdateMovement(SimpleMovement simpleMovement, GameObject player)
    {
        Rigidbody rigidbody = player.GetComponent<Rigidbody>();
        if (rigidbody == null) { return; }

        //if we are moving the enemy stays behind us and saves the velocity
        if (rigidbody.velocity != Vector3.zero)
        {
            simpleMovement.Target = player.transform.position - rigidbody.velocity * DISTANCE; 

            lastVelocity = rigidbody.velocity;
        }
        else
            simpleMovement.Target = player.transform.position - lastVelocity * DISTANCE;

        _timeStalked += Time.deltaTime;
    }
}
