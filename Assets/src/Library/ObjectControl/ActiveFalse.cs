using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActiveFalse : MonoBehaviour
{
    public long interval=100;
    protected System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();


    private void OnEnable()
    {
        timer.Restart();
    }

    // Update is called once per frame
    void Update()
    {
        if (timer.ElapsedMilliseconds > interval)
        {
            this.gameObject.SetActive(false);
        }
    }
}
