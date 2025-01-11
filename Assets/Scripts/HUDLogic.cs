using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HUDLogic : MonoBehaviour
{
    [SerializeField] TMPro.TMP_Text _text;

    private int _totalAmountKeys;
    public int TotalAmountKeys {  get { return _totalAmountKeys; } set { _totalAmountKeys = value; } }

    public void SetText(int newKeysAmount)
    {
        _text.text = ":" + newKeysAmount + '/' + _totalAmountKeys;
    }
}
