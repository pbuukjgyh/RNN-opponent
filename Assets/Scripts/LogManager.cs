using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;

public class LogManager : MonoBehaviour
{
    public static LogManager Instance { get; private set; }
    public int Killed { get; private set; } = 0;
    public List<string> TimesOfDeath { get; private set; } = new List<string>();
    const string LOGPATH = "GameLog.txt";
    private const string TARGETSPATH = "TargetsLog.csv";
    private const string BEHAVIORPATH = "BehaviorLog.csv";
    public int Index = 0;

    // Start is called before the first frame update
    void Start()
    {
        using (StreamWriter writetext = new StreamWriter(LOGPATH, true))
        {
            writetext.WriteLine("Started on: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fffffff"));
            writetext.Close();
        }

        using (StreamWriter writetext = new StreamWriter(TARGETSPATH, true))
        {
            writetext.WriteLine("Index,Chase,Stalk,Predict,Wander,Ambush,Track");
            writetext.Close();
        }

        using (StreamWriter writetext = new StreamWriter(BEHAVIORPATH, true))
        {
            writetext.WriteLine("Index,ChosenBehavior,Score");
            writetext.Close();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void Awake()
    {
        // If there is an instance, and it's not me, delete myself.

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

        DontDestroyOnLoad(gameObject);
    }

    public void OnKill()
    {
        Killed++;
        TimesOfDeath.Add(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fffffff"));
    }

    public void LogBehaviorScores(float[] targetScores)
    {
        using (StreamWriter writetext = new StreamWriter(TARGETSPATH, true))
        {
            string scores = string.Join(",", targetScores);
            writetext.WriteLine($"{Index},{scores}");
            writetext.Close();
        }
    }

    public void LogChosenBehaviors(string behavior, float score)
    {
        using (StreamWriter writetext = new StreamWriter(BEHAVIORPATH, true))
        {
            writetext.WriteLine($"{Index},{behavior},{score}");
            writetext.Close();
        }
    }

    public void WriteEnd()
    {
        using (StreamWriter writetext = new StreamWriter(LOGPATH, true))
        {
            writetext.WriteLine("Ended on: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fffffff"));
            writetext.WriteLine("Number of times killed: " + Killed);
            writetext.WriteLine("Recorded timestamps player was killed:");

            foreach (var item in TimesOfDeath)
            {
                writetext.WriteLine(item);
            }

            writetext.WriteLine("***************************************************************");
            writetext.Close();
        }
    }
}
