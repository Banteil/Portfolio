using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Burst;
using System.Threading;
using System;
using System.Diagnostics;

[BurstCompile]
public class JobTest : MonoBehaviour
{
    public struct MyJopParrale : IJobParallelFor
    {
        public void Execute(int index)
        {
            
        }
    }

    public struct MyJob : IJob
    {
        public int ID;
        public float Duration;
        float ElapsedTime;
        
        public void Execute()
        {
            var sw = new Stopwatch();
            while (ElapsedTime < Duration)
            {
                ElapsedTime = sw.Elapsed.Seconds;
                UnityEngine.Debug.Log($"ElapsedTime : {ElapsedTime}");
            }
        }
    }


    void Start()
    {
        var job = new MyJob();
        job.ID = 1;
        job.Duration = 10f;
        var handler = job.Schedule();

        //var job2 = new MyJob();
        //job2.ID = 2;
        //job2.Duration = 5f;
        //var handler2 = job2.Schedule();

        //handler2.Complete();

        UnityEngine.Debug.Log("Complete2");
    }
}
