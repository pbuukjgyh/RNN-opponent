using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndPointLogic : MonoBehaviour
{
    [SerializeField] GameObject _barrier;

    void Start()
    {
        PlayerBehavior player = FindAnyObjectByType<PlayerBehavior>();
        player.OnCollectedAllKeys.AddListener(RemoveBarrier);
    }

    private void RemoveBarrier()
    {
        _barrier.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        //check if player
        if (other.gameObject.GetComponent<PlayerBehavior>() != null)
        {
            SceneManager.LoadScene(1);
        }
    }
}
