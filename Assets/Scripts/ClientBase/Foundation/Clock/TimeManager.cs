using System;
using System.Collections.Generic;
using UnityEngine;

namespace ClientBase
{
    public class TimeManager : Singleton<TimeManager>
    {
        Timer firstTimer; //链表中第一个定时器
        float gameTime = 0;

        private ObjectPool<Timer> timePool;
        private int uid = 0;
        private List<int> _RemoveList;
        public TimeManager()
        {
            timePool = new ObjectPool<Timer>();
            _RemoveList = new List<int>();
        }
        public Timer AddTimer(float time, Action onFin, bool loop = false)//除非是循环调用，否则外部接收了Timer变量后都需要在回调结束后手写置null，不然就会出现两次删除的bug
        {
            uid++;
            Timer timer = timePool.Get();
            timer.Init(time, onFin, uid, gameTime, loop);
            //Debug.Log("实际添加计时器 " + timer.uid + "当前时间 " + gameTime + "结束时间 " + timer.nNextTickTime);
            InsertTimer(timer);
            return timer;
        }

        //把Timer对象加入到双向链表中
        internal void InsertTimer(Timer timer)
        {
            Timer nowTimer = firstTimer;
            if (nowTimer == null)
                firstTimer = timer;
            else
            {
                bool inserted = false;
                Timer lastTimer = nowTimer;
                int t = 0;
                while (nowTimer != null)
                {
                    lastTimer = nowTimer;
                    if (timer.nNextTickTime <= nowTimer.nNextTickTime)
                    {
                        if (nowTimer.Pre != null)
                        {
                            timer.Pre = nowTimer.Pre;
                            nowTimer.Pre.Next = timer;
                            timer.Next = nowTimer;
                            nowTimer.Pre = timer;
                        }
                        else
                        {
                            firstTimer = timer;
                            timer.Pre = null;
                            timer.Next = nowTimer;
                            nowTimer.Pre = timer;
                        }
                        inserted = true;
                        break;
                    }
                    t++;
                    nowTimer = nowTimer.Next;
                }
                //新的Timer应该放在最后
                if (inserted == false)
                {
                    lastTimer.Next = timer;
                    timer.Pre = lastTimer;
                    timer.Next = null;
                }
            }
        }
        internal void DeleteTimer(Timer timer)
        {
            if (timer.Next != null && timer.Pre != null)//在中间
            {
                timer.Pre.Next = timer.Next;
                timer.Next.Pre = timer.Pre;
            }
            else if (timer.Next == null && timer.Pre == null)//只有一个
            {
                firstTimer = null;
            }
            else if (timer.Pre == null && timer.Next != null)//第一个
            {
                firstTimer = timer.Next;
                timer.Next.Pre = null;
            }
            else if (timer.Pre != null && timer.Next == null)//最后一个
            {
                timer.Pre.Next = null;
            }
            timer.Pre = null;
            timer.Next = null;
        }
        public void RemoveTimer(Timer timer)
        {
            //Debug.Log("实际删除计时器 " + timer.uid + "当前时间 " + gameTime + "结束时间 " + timer.nNextTickTime);
            DeleteTimer(timer);
            timer.OnDestroy();
            timePool.Release(timer);
        }

        public void FixedUpdate()
        {
            gameTime += Time.deltaTime;
            if (firstTimer != null)
            {
                if (firstTimer.nNextTickTime > gameTime)
                    return;
            }
            Timer nowTimer = firstTimer;
            while (nowTimer != null)
            {
                if (nowTimer.nNextTickTime <= gameTime)
                {
                    nowTimer.Run(gameTime);
                    //run了之后会删除本身，TimerManager.firstTimer会设为第二个节点，所以需要重新指定
                    //如果是空则说明只有这一个Timer，对象已经找不到
                    nowTimer = firstTimer;
                }
                else
                    break;
            }
        }
        public void RemoveAllTimer()
        {
            Timer nowTimer = firstTimer;
            while (nowTimer != null)
            {
                DeleteTimer(nowTimer);
                nowTimer = firstTimer;
            }
        }
    }
    public class Timer
    {
        public Timer Next = null;
        public Timer Pre = null;
        public int uid;// 用户指定的定时器标志，便于手动清除、暂停、恢复
        private float _OnceCycleTime = -1;   // 用户设定的定时时长
        private bool _Loop;          // 是否循环执行
        private Action _OnFin;
        public int loopedTimes = 0;//定时器已经循环了多少次
        public float nNextTickTime;
        internal void Init(float time, Action onFin, int _uid, float gameTime, bool loop = false)
        {
            uid = _uid;
            _OnceCycleTime = time;
            _Loop = loop;
            _OnFin = onFin;
            nNextTickTime = gameTime + time;
        }
        internal void Run(float gameTime)
        {
            //Debug.Log("Run计时器 " + uid + "当前时间 " + gameTime + "结束时间 " + nNextTickTime);
            loopedTimes = (int)Math.Floor(gameTime / _OnceCycleTime);
            if (_Loop == true)
            {
                TimeManager.Ins.DeleteTimer(this);
                nNextTickTime = gameTime + _OnceCycleTime;
                TimeManager.Ins.InsertTimer(this);
            }
            else
            {
                //Debug.Log("准备删除计时器 " + uid);
                TimeManager.Ins.RemoveTimer(this);
                
            }
            _OnFin?.Invoke();
        }
        internal void OnDestroy() // 回收定时器
        {
            //_OnFin = null;
            _OnceCycleTime = -1;
            _Loop = false;
            //uid = -1;
            Pre = null;
            Next = null;
        }
    }
}
