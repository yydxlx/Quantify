using UnityEngine;
using System.Collections.Generic;
using System;
using System.Threading;
using System.Linq;
using ClientBase;

public class Loom : Singleton<Loom>
{
    public static int maxThreads = 8;
    static int numThreads;
    private int _count;
    public struct DelayedQueueItem
    {
        public float time;
        public Action action;
    }
    private List<Action> _actions = new List<Action>();
    private List<Action> _currentActions = new List<Action>();
    private List<DelayedQueueItem> _delayed = new List<DelayedQueueItem>();
    private List<DelayedQueueItem> _currentDelayed = new List<DelayedQueueItem>();

    public void QueueOnMainThread(Action action)//入口
    {
        
        QueueOnMainThread(action, 0f);
    }
    public void QueueOnMainThread(Action action, float time)//入口
    {
        if (time != 0)
        {
            lock (Ins._delayed)
            {
                Ins._delayed.Add(new DelayedQueueItem { time = Time.time + time, action = action });
            }
        }
        else
        {
            lock (Ins._actions)
            {
                Ins._actions.Add(action);
            }
        }
    }
    public Thread RunAsync(Action a)//入口
    {
        //Init();
        while (numThreads >= maxThreads)
        {
            Thread.Sleep(1);
        }
        Interlocked.Increment(ref numThreads);
        ThreadPool.QueueUserWorkItem(RunAction, a);
        return null;
    }
    private void RunAction(object action)
    {
        try
        {
            ((Action)action)();
        }
        catch
        {
        }
        finally
        {
            Interlocked.Decrement(ref numThreads);
        }
    }
    public void Update()
    {
        lock (_actions)
        {
            _currentActions.Clear();
            _currentActions.AddRange(_actions);
            _actions.Clear();
        }
        foreach (var a in _currentActions)
        {
            a();
        }
        lock (_delayed)
        {
            _currentDelayed.Clear();
            _currentDelayed.AddRange(_delayed.Where(d => d.time <= Time.time));
            foreach (var item in _currentDelayed)
                _delayed.Remove(item);
        }
        foreach (var delayed in _currentDelayed)
        {
            delayed.action();
        }
    }
}