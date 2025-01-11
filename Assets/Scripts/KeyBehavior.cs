using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class KeyBehavior : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        //check if player
        if(other.gameObject.GetComponent<PlayerBehavior>() != null)
        {
            Destroy(gameObject);
            OnKeyCollected.Invoke();
        }
    }

    [SerializeField]
    private Vector3 rotationSpeed = new Vector3(0, 100, 0);

    [SerializeField]
    private GameObject _effectsObject;

    public UnityEvent OnKeyCollected = new UnityEvent();

    private void Start()
    {
        OnKeyCollected.AddListener(() => Instantiate(_effectsObject));

        var enemy = FindAnyObjectByType<EnemyBehavior>();
        if (enemy != null) OnKeyCollected.AddListener(enemy.TrainRNN);
    }

    void Update()
    {
        // Rotate the object over time
        transform.Rotate(rotationSpeed * Time.deltaTime);
    }
}
