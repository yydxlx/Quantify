using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Cocoon.Auido
{
    /// <summary>
    /// 声音的播放
    /// </summary>
    public class Audio
    {
        public const int State_Idle = 0;//播放状态：空闲（未播放但随时等待播放）
        public const int State_Play = 1;//播放状态：播放中
        public const int State_Stop = 2;//播放状态：停止
        public const int State_Pause = 3;//播放状态：暂停
        public const int State_Interval = 4;//播放状态：循环等待中

        public const int Fade_Stable = 0;// 渐变状态：稳定中
        public const int Fade_In = 1;// 渐变状态：渐渐播放出来
        public const int Fade_Out = 2;// 渐变状态：渐渐隐去
        private int _AudioUid; //全局唯一id
        private CtrlType _CtrlType; //声音类型

        private Vector3 _Position;// 位置
        private AudioSource _Source;
        private int _State;
        private int _Fade;
        private float _PreStopTime;//最后使用的时间
        private float _CurVolume;
        private AudioBaseCtrl _BaseCtrl;
        private AudioClip _Clip;
        public Action OnFinish;

        public Audio()
        {
            Reset(0, null, CtrlType.UISound, null, null);
        }
        /// <summary>
        /// 重置音效
        /// </summary>
        /// <param name="uid">全局id</param>
        /// <param name="id">资源id</param>
        /// <param name="clip">The clip.</param>
        /// <param name="ctrlType">声音类型</param>
        /// <param name="baseCtrl">The base control.</param>
        public void Reset(int uid, AudioClip clip, CtrlType ctrlType, AudioBaseCtrl baseCtrl, Action onFinish)
        {
            _AudioUid = uid;
            _Clip = clip;
            _CtrlType = ctrlType;
            _Position = Vector3.zero;
            _Source = null;
            _State = State_Idle;
            _Fade = Fade_Stable;
            _PreStopTime = Time.time;
            _CurVolume = 0.0f;
            _BaseCtrl = baseCtrl;
            OnFinish = onFinish;
        }
        public AudioClip clip
        {
            get { return _Clip; }
            set { _Clip = value; }
        }
        public CtrlType ctrlType
        {
            get { return _CtrlType; }
            set { _CtrlType = value; }
        }
        public int audioHandleId
        {
            get { return _AudioUid; }
        }
        public Vector3 position
        {
            get { return _Position; }
            set
            {
                _Position = value;
                if (_Source != null)
                {
                    _Source.transform.position = value;
                }
            }
        }
        public int state
        {
            get { return _State; }
            set { _State = value; }
        }
        public int fade
        {
            get { return _Fade; }
            set { _Fade = value; }
        }
        public float preStopTime
        {
            get { return _PreStopTime; }
            set { _PreStopTime = value; }
        }
        public float curVolume
        {
            get { return _CurVolume; }
            set { _CurVolume = value; }
        }
        public AudioSource source
        {
            get { return _Source; }
            set { _Source = value; }
        }
        public AudioBaseCtrl baseCtrl
        {
            get { return _BaseCtrl; }
            set { _BaseCtrl = value; }
        }
        /// <summary>
        /// 播放中
        /// </summary>
        public bool isPlaying
        {
            get
            {
                return _Fade != Fade_Stable || _State == State_Play || _State == State_Interval;
            }
        }
        /// <summary>
        /// 判断音乐是否随时都可以播放
        /// </summary>
        public bool isIdle
        {
            get { return _State == State_Idle; }
        }
        /// <summary>
        /// 已经停止了（或者马上就要停止了）
        /// </summary>
        public bool isStop
        {
            get { return _State == State_Stop; }
        }
        /// <summary>
        /// 是否需要回收
        /// </summary>
        public bool needGabage
        {
            get
            {
                return _State == State_Stop && !isPlaying;
            }
        }
        /// <summary>
        /// 真正创建音源（并播放）
        /// </summary>
        public void Fetch()
        {
            if (_Source != null)
                return;
            AudioMgr.Ins.audioSourceMgr.Fetch(this);
        }
    }
}