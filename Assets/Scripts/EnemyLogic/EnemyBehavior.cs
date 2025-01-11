using Grpc.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.MLAgents.Sensors;
using UnityEngine;
using UnityEngine.SceneManagement;
using static UnityEngine.GraphicsBuffer;

public enum PersonalityType
{
    Passive,
    Aggressive
}

public class EnemyBehavior : MonoBehaviour
{
    [SerializeField] private GameObject _player;
    [SerializeField] private float _range = 1.0f;

    private SimpleMovement _movement;

    [SerializeField] int _inputSize = 6; //the things we want to remember
    [SerializeField] int _hiddenSize = 10; //number of neurons
    [SerializeField] int _outputSize = 6; //actions: bv. movex, movez,wait...

    //personality modifiers
    private float _chasePercent     = 1f;
    private float _stalkPercent     = 0.5f;
    private float _predictPercent   = 1f;
    private float _wanderPercent    = 1f;
    //private float _waitPercent      = 0.5f;
    private float _ambushPercent    = 0.5f;
    private float _trackPercent     = 0.5f;

    private float _chaseWeight      = 1f;
    private float _stalkWeight      = 1f;
    private float _predictWeight    = 1f;
    private float _wanderWeight     = 1f;
    //private float _waitWeight       = 1f;
    private float _ambushWeight     = 1f;
    private float _trackWeight      = 1f;

    private float[] _inputs;
    private float[] _outputs;

    Vector3 _lastPos;
    Vector3 _lastPosPlayer;

    private RNNRLAgent _RNNRLAgent;

    const string WEIGHTSPATH = "AgentWeights.txt";

    [SerializeField] private PersonalityType _personality = PersonalityType.Passive;


    void Start()
    {
        if (_player == null) _player = FindAnyObjectByType<PlayerBehavior>().gameObject;

        _movement = GetComponent<SimpleMovement>();

        //add the choices
        //_choices.Add(new WaitChoiceAi(_waitPercent));
        _choices.Add(new WanderChoiceAi(_wanderPercent));
        _choices.Add(new ChaseChoiceAi(_chasePercent));
        _choices.Add(new StalkChoiceAi(_stalkPercent));
        _choices.Add(new PredictChoiceAi(_predictPercent));
        _choices.Add(new AmbushChoiceAi(_ambushPercent));
        _choices.Add(new TrackChoiceAi(_trackPercent));

        //init rnn
        _RNNRLAgent = new RNNRLAgent(_inputSize, _hiddenSize, _outputSize);
        _inputs = new float[_inputSize];
        _outputs = new float[_outputSize];
        _lastPos = transform.position;
        _lastPosPlayer = _player.transform.position;

        //if we have saved weights, load them
        if (File.Exists(WEIGHTSPATH) && new FileInfo(WEIGHTSPATH).Length != 0)
        {
            _RNNRLAgent.LoadWeights(WEIGHTSPATH);
        }
    }

    private void OnDestroy()
    {
        _RNNRLAgent.SaveWeights(WEIGHTSPATH);
    }

    //reset the agent when the app closes
    private void OnApplicationQuit()
    {
        File.WriteAllText(WEIGHTSPATH, string.Empty);
        LogManager.Instance.WriteEnd();
    }

    void Update()
    {
        //reset
        if (Vector3.Distance(transform.position, _player.transform.position) < _range)
        {
            SceneManager.LoadScene(0);
            LogManager.Instance.OnKill();
        }

        //check area
        AreaManager.Instance.UpdateEntityArea(_player);

        //rnn
        _inputs[0] = Mathf.Clamp01(Vector3.Distance(_player.transform.position, transform.position));
        _inputs[1] = (transform.position - _lastPos).magnitude; // Movement from the last position
        _inputs[2] = (_player.transform.position - transform.position).normalized.x; // X direction
        _inputs[3] = (_player.transform.position - transform.position).normalized.z; // Z direction
        _lastPos = transform.position;

        int playerAreaIndex = AreaManager.Instance.GetAreaIndex(_player);
        int enemyAreaIndex = AreaManager.Instance.GetAreaIndex(gameObject);

        _inputs[4] = playerAreaIndex >= 0 ? (float)playerAreaIndex / AreaManager.Instance.areas.Count : 0f;
        _inputs[5] = (_player.transform.position - _lastPosPlayer).magnitude; // Movement player
        _lastPosPlayer = _player.transform.position;

        //give the inputs to the agent to calculate the best course of action
        _outputs = _RNNRLAgent.ForwardPropagation(_inputs);

        DecideActions();

        HandleMovement();

        TrainRNN();

        LogManager.Instance.Index++;
    }

    private void DecideActions()
    {
        //in our example outputs[0] controlls the movement
        //float moveDirX = Mathf.Clamp(_outputs[0], -1f, 1f);
        //float moveDirZ = Mathf.Clamp(_outputs[1], -1f, 1f);
        //bool isActive = _outputs[2] > 0.5f;

        _chaseWeight = Mathf.Clamp01(_outputs[0]);
        _stalkWeight = Mathf.Clamp01(_outputs[1]);
        _predictWeight = Mathf.Clamp01(_outputs[2]);
        _wanderWeight = Mathf.Clamp01(_outputs[3]);
        //_waitWeight = Mathf.Clamp01(_outputs[4]);
        _ambushWeight = Mathf.Clamp01(_outputs[5]);
        _trackWeight = Mathf.Clamp01(_outputs[6]);
    }

    public void TrainRNN()
    {
        float[] target = new float[_outputSize];
        float distance = Vector3.Distance(transform.position, _player.transform.position);

        //chase
        // Encourage "Chase" heavily if the distance is within a reasonable range
        target[0] = 1.0f - Mathf.Clamp01(distance / 10f);

        // Penalize staying still to encourage chasing
        if ((transform.position - _lastPos).magnitude < 0.2f)
        {
            target[0] -= 0.2f;
        }

        //stalk
        // Encourage "Stalk" if close to the player
        target[1] = 1.0f - Mathf.Clamp01(1+ distance / 10f);

        //predict
        // Encourage "Predict" if in vicinity of player
        float midRangeMin = 5f;
        float midRangeMax = 15f;

        if (distance >= midRangeMin && distance <= midRangeMax)
        {
            float normalizedDistance = (distance - midRangeMin) / (midRangeMax - midRangeMin);
            target[2] = 1.0f - Mathf.Pow((normalizedDistance - 0.5f) * 2f, 2f);
        }

        //wander
        // Encourage "Wander" if far from the player
        target[3] = Mathf.Clamp01(distance / 10f);

        // Penalize staying still
        if ((transform.position - _lastPos).magnitude < 0.2f)
        {
            target[3] -= 0.5f;
        }

        //wait
        // Encourage staying still
        //if ((transform.position - _lastPos).magnitude < 0.2f)
        //{
        //    target[4] = 0.55f;
        //}

        // Penalize if player is close
        //target[4] -= 1.0f - Mathf.Clamp01(distance / 10f);

        //ambush
        // Encourage "Ambush" if far from the player
        target[4] = Mathf.Clamp01(distance / 10f);

        // Penalize if player is close
        target[4] -= 1.0f - Mathf.Clamp01(distance / 10f);

        // Penalize if player has't entered many areas
        BoxCollider mostVisitedArea = AreaManager.Instance.GetMostVisitedArea();
        float playerVisits = 0f;
        if (mostVisitedArea != null)
            playerVisits = AreaManager.Instance.GetVisitCount(mostVisitedArea);

        if (playerVisits < 3)
            target[4] = 0;

        //track
        // Encourage "Track" if far from the player
        target[5] = Mathf.Clamp01(distance / 10f);

        // Penalize if player is close
        target[5] -= 1.0f - Mathf.Clamp01(distance / 10f);



        //normalize targets
        float total = target.Sum();
        if (total > 1.0f)
        {
            for (int i = 0; i < target.Length; i++)
            {
                target[i] /= total;
            }
        }

        LogManager.Instance.LogBehaviorScores(target);
        _RNNRLAgent.BackwardPropagation(_inputs, _outputs, target);
    }


    private BaseChoicesAi _currentChoice;
    private List<BaseChoicesAi> _choices = new List<BaseChoicesAi>();

    void HandleMovement()
    {
        if (_movement == null || _player == null)
        {
            return;
        }

        float currentHighestScore = -1;

        foreach (BaseChoicesAi choice in _choices)
        {
            if (choice != null)
            {
                float weight = 1f;

                if (choice is ChaseChoiceAi) weight = _chaseWeight;
                if (choice is StalkChoiceAi) weight = _stalkWeight;
                if (choice is PredictChoiceAi) weight = _predictWeight;
                if (choice is WanderChoiceAi) weight = _wanderWeight;
                //if (choice is WaitChoiceAi) weight = _waitWeight;
                if (choice is AmbushChoiceAi) weight = _ambushWeight;
                if (choice is TrackChoiceAi) weight = _trackWeight;

                float score = choice.CalculatePoints(_player, transform, weight);

                if (currentHighestScore < score)
                {
                    _currentChoice = choice;
                    currentHighestScore = score;
                }
            }
        }

        if (_currentChoice != null)
        {
            _currentChoice.UpdateMovement(_movement, _player);

            LogManager.Instance.LogChosenBehaviors(_currentChoice.ToString(), currentHighestScore);

            Debug.Log("CurrentChoice: "+_currentChoice.ToString() + " with SCORE: " + currentHighestScore);
            //Debug.Log("SCORE: "+currentHighestScore);

            //BoxCollider mostVisitedArea = AreaManager.Instance.GetMostVisitedArea();

            //float playerVisits = 0f;

            //if (mostVisitedArea != null)
            //    playerVisits = AreaManager.Instance.GetVisitCount(mostVisitedArea);



            //Debug.Log("AREA VISITED: " + playerVisits + " TIMES");
        }
    }

}
