using ClientBase;
using CocoonAsset;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ClientBase
{
    public class TickMgr : Singleton<TickMgr>
    {
        public ObjectPool<Tick> tickPool;
        private Dictionary<int, Tick> _TickDic;
        public List<int> toRemoveList;
        private int uid = 0;
        public TickMgr()
        {
            tickPool = new ObjectPool<Tick>();
            _TickDic = new Dictionary<int, Tick>();
            toRemoveList = new List<int>();
        }
        public void AddTick(float _totalTime, Action<float> _onTick, Action _onFin)
        {
            uid++;
            Tick tick = tickPool.Get();
            tick.Init(_totalTime, _onTick, _onFin, uid);
            _TickDic.Add(uid, tick);
        }
        public void RemoveTick(int _uid)
        {
            tickPool.Release(_TickDic[_uid]);
            _TickDic.Remove(_uid);
        }
        public void Update()
        {
            for (int i = toRemoveList.Count - 1; i >= 0; i--)
            {
                _TickDic.Remove(toRemoveList[i]);
                toRemoveList.RemoveAt(i);
            }
            foreach (var item in _TickDic)
            {
                item.Value.Update();
            }
        }
        public void Release()
        {
            tickPool.Clear();
            _TickDic.Clear();
        }
    }
    public class Tick
    {
        public int uid;
        Action<float> onTick;
        Action onFin;
        float totalTime;
        float curTime;
        public void Init(float _totalTime, Action<float> _onTick, Action _onFin, int _uid)
        {
            uid = _uid;
            onTick = _onTick;
            onFin = _onFin;
            totalTime = _totalTime;
        }
        public void Update()
        {
            onTick.Invoke(curTime);
            curTime += Time.deltaTime;
            if (curTime >= totalTime)
            {
                onFin.Invoke();
                TickMgr.Ins.toRemoveList.Add(uid);
            }
        }
    }
}