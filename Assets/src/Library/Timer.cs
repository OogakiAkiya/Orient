using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Timer
{
    private System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
    // Start is called before the first frame update
    Timer()
    {
        timer.Start();
    }
    
    public string GetTime()
    {
        return ""+timer.Elapsed.Minutes + timer.Elapsed.Seconds;
    }

}
