using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Quit : MonoBehaviour
{
    public void QuitGame()
    {
        LogManager.Instance.WriteEnd();
        Application.Quit();
    }
}
