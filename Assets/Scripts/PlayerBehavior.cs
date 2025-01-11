using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PlayerBehavior : MonoBehaviour
{
    [SerializeField]
    private InputActionAsset _inputAsset;

    private SimpleMovement _simpleMovement;

    [SerializeField]
    private InputActionReference _movementActionZ;
    [SerializeField]
    private InputActionReference _movementActionX;

    private int _totalAmountKeys;
    private int _amountCollectedKeys;

    [SerializeField]
    private HUDLogic _hudLogic;

    public bool HasCollectedAllKeys()
    {
        return _amountCollectedKeys >= _totalAmountKeys;
    }

    public UnityEvent OnCollectedAllKeys = new UnityEvent();

    private void Start()
    {
        KeyBehavior[] keys = FindObjectsOfType<KeyBehavior>();

        _totalAmountKeys = keys.Length;

        _hudLogic.TotalAmountKeys = _totalAmountKeys;
        _hudLogic.SetText(0);

        foreach (var key in keys)
        {
            key.OnKeyCollected.AddListener(() => { ++_amountCollectedKeys; if (HasCollectedAllKeys()) OnCollectedAllKeys.Invoke(); });
            key.OnKeyCollected.AddListener(() => _hudLogic.SetText(_amountCollectedKeys));
        }
    }

    private void OnEnable()
    {
        if (_inputAsset == null) return;

        _inputAsset.Enable();
    }
    private void OnDisable()
    {
        if (_inputAsset == null) { return; }
        _inputAsset.Disable();
    }

    void Awake()
    {
        _simpleMovement = GetComponent<SimpleMovement>();
    }

    // Update is called once per frame
    void Update()
    {
        if (_inputAsset == null) return;

        HandleMovementInput();
    }

    void HandleMovementInput()
    {
        if (_movementActionX == null || _movementActionZ == null) { return; }

        float moveXInput = _movementActionX.action.ReadValue<float>();
        float moveZInput = _movementActionZ.action.ReadValue<float>();

        Vector3 moveX = moveXInput * transform.right;
        Vector3 moveZ = moveZInput * transform.forward;

        Vector3 combinedMovement = moveX + moveZ;

        //to prevent faster diagonal movement
        if (combinedMovement.magnitude > 1f)
        {
            combinedMovement.Normalize();
        }

        _simpleMovement.Desiredmovedir = combinedMovement;
    }

}
