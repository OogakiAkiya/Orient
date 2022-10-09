using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Timer
{
    private System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
    // Start is called before the first frame update
    public Timer()
    {
        timer.Start();
    }
    
    public string GetTime()
    {
        return timer.Elapsed.Minutes.ToString() + timer.Elapsed.Seconds.ToString();
    }
    public int GetTime_Minutes()
    {
        return timer.Elapsed.Minutes;
    }
    public int GetTime_Second()
    {
        return timer.Elapsed.Seconds;
    }

}
