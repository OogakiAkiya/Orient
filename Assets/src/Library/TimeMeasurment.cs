using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;


public class TimeMeasurment
{
    protected Stopwatch timer = new Stopwatch();
    //Actionに設定した処理にtimes回試行し平均して何秒かかったのか測定する
    public double Start(Action _action, int _times=1)
    {
        double avarage = 0;
        for (int i = 0; i < _times; i++)
        {
            timer.Reset();
            timer.Start();
            _action();
            timer.Stop();
            avarage = (double)timer.ElapsedTicks / (double)Stopwatch.Frequency;

        }
        return avarage /= (double)_times;
    }

    public long StartMS(Action _action, int _times=1)
    {
        long avarage = 0;
        for (int i = 0; i < _times; i++)
        {
            timer.Reset();
            timer.Start();
            _action();
            timer.Stop();
            avarage = timer.ElapsedMilliseconds;

        }
        return avarage /= _times;
    }

}