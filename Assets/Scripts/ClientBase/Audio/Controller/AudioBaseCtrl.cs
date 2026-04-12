using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Cocoon.Auido
{
    //控制器基类
    public class AudioBaseCtrl : MonoBehaviour
    {
        public float _Volume;// 音量
        public float volume
        {
            get { return _Volume; }
            set
            {
                _Volume = value;
                foreach(Audio audio in _Audios)
                {
                    audio.curVolume = value;
                }
            }
        }
        protected LinkedList<Audio> _Audios;
        protected CtrlType _CType;
        private bool _Mute;// 是否静音
        //private float _NextDriveSourceMgrTime;
        protected float _FadeSpeed;
        protected bool _Loop;// 是否循环播放
        protected bool _NeedFade;

        public float _InnerRadius { get; set; } // 恒音半径
        public float _OuterRadius { get; set; } // 衰减半径
        public bool is3D { get; set; }
        public int _GabageWaitTimeMultiple { get; set; }//回收等待时间的倍数
        public bool mute
        {
            get { return _Mute; }
            set
            {
                _Mute = value;
                ApplyMute();
            }
        }
        private void Awake()
        {
            _Audios = new LinkedList<Audio>();
            _Volume = 1;
            _Mute = false;
            //_NextDriveSourceMgrTime = 0.0f;
            LateAwake();
        }
        protected virtual void LateAwake()
        {
            _Loop = false;
            _FadeSpeed = 0.5f;
            is3D = false;
            _NeedFade = false;
            _InnerRadius = 3.0f;
            _OuterRadius = 20.0f;
            _GabageWaitTimeMultiple = 1;
            _CType = CtrlType.UISound;
        }
        /// <summary>
        /// 添加播放实例
        /// </summary>
        public void AddAudio(Audio audio)
        {
            _Audios.AddLast(audio);
        }


        //public void SetChildAudioSource()
        //{
        //    var it = _Audios.First;
        //    while (it != null)
        //    {
        //        Audio audio = it.Value;
        //        it = it.Next;
        //    }
        //}
        public virtual bool CanPlay(Vector3 pos, AudioClip audioClip)
        {
            return false;
        }

        public virtual void OnProcessAudio() { }

        void Update()
        {
            Vector3 pos = AudioMgr.Ins.MainListenerPosition;
            if (_Audios.Count > 0)
            {
                var it = _Audios.First;
                while (it != null)
                {
                    var cur = it;
                    it = it.Next;
                    Audio audio = cur.Value;
                    AudioUpdate(audio);
                    if (audio.needGabage)
                    {//需要回收本次播放实例
                        _Audios.Remove(cur);
                        Destroy(audio);
                        AudioMgr.Ins.GabageToPool(audio);
                        continue;
                    }
                    if (audio.isIdle)
                    {//随时等待播放
                        if (is3D == true)
                        {
                            if (InPlayRange(audio.position))
                            {
                                audio.Fetch();
                                Play(audio);
                            }
                        }
                        else
                        {
                            audio.Fetch();
                            Play(audio);
                        }
                        
                        continue;
                    }
                    if (audio.isPlaying && is3D == true)
                    {//播放中
                        if (!InPlayRange(audio.position))
                        {
                            Silent(audio);
                        }
                        continue;
                    }
                }
            }

            //if (Time.time > _NextDriveSourceMgrTime)
            //{
            //    _NextDriveSourceMgrTime = Time.time + 15f;
            //    AudioMgr.Ins. AudioSourceMgr.Gabage();
            //}
        }
        /// <summary>
        /// 每帧更新
        /// </summary>
        public void AudioUpdate(Audio audio)
        {
            if (audio.source == null)
                return;
            if (audio.source.isPlaying)
            {//音乐在播放中
                if (audio.fade == Audio.Fade_In)
                {//渐渐播放出来
                    audio.curVolume += _FadeSpeed * Time.deltaTime;
                    if (audio.curVolume >= _Volume)
                    {//完全播放出来
                        audio.curVolume = _Volume;
                        audio.fade = Audio.Fade_Stable;
                    }
                }
                else if (audio.fade == Audio.Fade_Out)
                {//渐渐隐去
                    audio.curVolume -= _FadeSpeed * Time.deltaTime;
                    if (audio.curVolume <= 0.0f)
                    {//完全隐去
                        audio.curVolume = 0.0f;
                        audio.fade = Audio.Fade_Stable;
                        if (audio.state == Audio.State_Stop)
                            audio.source.Stop();
                        else
                            audio.source.Pause();
                        audio.preStopTime = Time.time;
                    }
                }
                ApplyVolume();
            }
            else
            {//音乐已停止
                if (audio.state == Audio.State_Play)
                {//在正常播放中停止
                    audio.preStopTime = Time.time;
                    if (_Loop)
                    {//循环播放的音效
                        audio.source.loop = true;
                        audio.source.Play();
                        audio.state = Audio.State_Play;
                    }
                    else
                    {//不是循环播放的音效，直接标记为停止
                        audio.source.Stop();
                        audio.OnFinish?.Invoke();
                        audio.state = Audio.State_Stop;
                        audio.fade = Audio.Fade_Stable;
                    }
                }
                else if (audio.state == Audio.State_Interval)
                {//等待下次播放
                    if (_Loop)
                    {//再次播放
                        Play(audio);
                    }
                    else
                    {//停止
                        Stop(audio);
                    }
                }
                else
                {//其它情况
                    audio.fade = Audio.Fade_Stable;
                }
            }
        }
        public void ApplyMute()
        {
            foreach (Audio audio in _Audios)
            {
                if (audio.source == null)
                    return;
                audio.source.mute = _Mute;
            }
        }
        public void ApplyVolume()
        {
            foreach (Audio audio in _Audios)
            {
                if (audio.source == null)
                    return;
                float v = audio.curVolume;
                if (is3D)
                {
                    float len = _OuterRadius - _InnerRadius;
                    float distance = _OuterRadius - Vector3.Distance(AudioMgr.Ins.MainListenerPosition, audio.position);
                    float ratio = Mathf.Clamp(distance / len, 0.0f, 1.0f);
                    v *= ratio;
                }
                v *= AudioMgr.Ins.voiceVolumeFactor;
                if (audio.source.volume != v)
                {
                    audio.source.volume = v;
                }
            }
        }
        public void Play(Audio audio)
        {
            if (audio.state == Audio.State_Play)
                return;
            if (audio.source == null)
            {
                //没有创建播放实体，重置状态
                //调用这个函数是因为Play默认是从当前位置开始播放，而不是从头开始播放
                audio.fade = Audio.Fade_Stable;
                audio.state = Audio.State_Idle;
                return;
            }
            if (audio.source.isPlaying)
            {
                //播放中，调整状态
                audio.state = Audio.State_Play;
                audio.fade = Audio.Fade_In;
            }
            else
            {
                //没有播放，启动播放
                if (audio.source.enabled)
                {//播放成功
                    audio.source.Play();
                    if (_NeedFade)
                    {//音乐需要音量渐变处理
                        audio.source.volume = audio.curVolume = 0.0f;
                        audio.fade = Audio.Fade_In;
                    }
                    else
                    {//普通音效直接播放
                        audio.source.volume = audio.curVolume = _Volume;
                        audio.fade = Audio.Fade_Stable;
                    }
                    audio.state = Audio.State_Play;
                }
                else
                {//播放失败，等待下次
                    audio.fade = Audio.Fade_Stable;
                    audio.state = Audio.State_Idle;
                }
            }
        }
        /// <summary>
        /// 停止播放
        /// </summary>
        public void Stop(Audio audio)
        {
            if (audio.state == Audio.State_Stop)
                return;
            if (audio.source == null)
            {//没有播放实体，直接设置状态
                audio.fade = Audio.Fade_Stable;
                audio.state = Audio.State_Stop;
                return;
            }

            if (audio.source.isPlaying)
            {//在播放中，尝试停止
                if (_NeedFade)
                {//音乐需要音量渐变处理
                    audio.fade = Audio.Fade_Out;
                    audio.state = Audio.State_Stop;
                }
                else
                {//普通声音直接停止
                    audio.source.Stop();
                    audio.state = Audio.State_Stop;
                    audio.fade = Audio.Fade_Stable;
                    audio.preStopTime = Time.time;
                }
            }
            else
            {//直接停止，也需要同样处理
                audio.source.Stop();
                audio.state = Audio.State_Stop;
                audio.fade = Audio.Fade_Stable;
                if (audio.preStopTime + 0.3f < Time.time)
                {//这样处理，是为了保证记录正确的停止时间
                    audio.preStopTime = Time.time;
                }
            }
        }
        /// <summary>
        /// 暂停播放
        /// </summary>
        public void Pause(Audio audio)
        {
            if (audio.state == Audio.State_Pause || audio.state == Audio.State_Stop)
            {
                //已经暂停或者停止
                return;
            }
            if (audio.source == null)
            {
                //没有播放实体，直接设置状态
                audio.fade = Audio.Fade_Stable;
                audio.state = Audio.State_Pause;
                return;
            }
            if (audio.source.isPlaying)
            {
                //在播放中，尝试停止
                if (_NeedFade)
                {
                    //音乐需要音量渐变处理
                    audio.state = Audio.State_Pause;
                    audio.fade = Audio.Fade_Out;
                }
                else
                {
                    //普通声音直接停止
                    audio.source.Pause();
                    audio.state = Audio.State_Pause;
                    audio.fade = Audio.Fade_Stable;
                    audio.preStopTime = Time.time;
                }
            }
            else
            {
                //直接停止
                audio.source.Pause();
                audio.state = Audio.State_Pause;
                audio.fade = Audio.Fade_Stable;
                if (audio.preStopTime + 0.3f < Time.time)
                {
                    //这样处理，是为了保证记录正确的停止时间
                    audio.preStopTime = Time.time;
                }
            }
        }
        /// <summary>
        /// 静音 
        /// </summary>
        public void Silent(Audio audio)
        {
            if (audio.state == Audio.State_Pause
                || audio.state == Audio.State_Stop
                || audio.state == Audio.State_Idle)
            {
                //已经暂停或者停止
                return;
            }
            if (audio.source == null)
            {
                //没有播放实体，直接设置状态
                audio.fade = Audio.Fade_Stable;
                audio.state = Audio.State_Idle;
                return;
            }
            if (audio.source.isPlaying)
            {
                //在播放中，尝试停止
                if (_NeedFade)
                {
                    //音乐需要音量渐变处理
                    audio.state = Audio.State_Idle;
                    audio.fade = Audio.Fade_Out;
                }
                else
                {
                    //普通声音直接停止
                    audio.source.Pause();
                    audio.state = Audio.State_Idle;
                    audio.fade = Audio.Fade_Stable;
                    audio.preStopTime = Time.time;
                }
            }
            else
            {
                //直接停止
                audio.state = Audio.State_Idle;
                audio.fade = Audio.Fade_Stable;
            }
        }
        /// <summary>
        /// 删除声音(所有过程结束的删除)
        /// </summary>
        public void Destroy(Audio audio)
        {
            //audio.preStopTime = Time.time;
            AudioMgr.Ins.Destroy(audio);
            audio.Reset(0, null, CtrlType.UISound, null, null);
        }

        /// <summary>
        /// 继续播放音频
        /// </summary>
        public void ContinueAudioByGlobalID(int audioHandleId)
        {
            var it = _Audios.First;
            while (it != null)
            {
                Audio audio = it.Value;
                it = it.Next;
                if (audio.audioHandleId == audioHandleId && !audio.isStop)
                {
                    Play(audio);
                    break;
                }
            }
        }

        /// <summary>
        /// 暂停音频
        /// </summary>
        public void PauseAudioByGlobalID(int audioHandleId)
        {
            var it = _Audios.First;
            while (it != null)
            {
                Audio audio = it.Value;
                it = it.Next;
                if (audio.audioHandleId == audioHandleId && !audio.isStop)
                {
                    Pause(audio);
                    break;
                }
            }
        }
        /// <summary>
        /// 暂停所有音频
        /// </summary>
        public void PauseAllAudio()
        {
            var it = _Audios.First;
            while (it != null)
            {
                Audio audio = it.Value;
                it = it.Next;
                Pause(audio);
            }
        }
        /// <summary>
        /// 继续所有音频
        /// </summary>
        public void PlayAllAudio()
        {
            var it = _Audios.First;
            while (it != null)
            {
                Audio audio = it.Value;
                it = it.Next;
                if(audio.state != Audio.Fade_Out)
                    Play(audio);
            }
        }
        /// <summary>
        /// 移除给定全局id的播放实例(准备删除，之后要经过声音渐隐之类的过程)
        /// </summary>
        public void RemoveAudioByGlobalID(int audioHandleId)
        {
            var it = _Audios.First;
            while (it != null)
            {
                Audio audio = it.Value;
                it = it.Next;
                if (audio.audioHandleId == audioHandleId && !audio.isStop)
                {
                    Stop(audio);
                    break;
                }
            }
        }
        /// <summary>
        /// 清理所有音效
        /// </summary>
        public void RemoveAllAudios()
        {
            var it = _Audios.First;
            while (it != null)
            {
                Audio audio = it.Value;
                it = it.Next;

                audio.preStopTime = Time.time;

                Destroy(audio);
                // AudioMgr.Ins.GabageToPool(audio);

            }
            _Audios.Clear();
        }

        /// <summary>
        /// 判断给定点是否在该音源的播放范围内
        /// </summary>
        public bool InPlayRange(Vector3 position)
        {
            Vector3 pos = AudioMgr.Ins.MainListenerPosition;
            float distance = (pos.x - position.x) * (pos.x - position.x)
                    + (pos.y - position.y) * (pos.y - position.y)
                    + (pos.z - position.z) * (pos.z - position.z);
            return distance < _OuterRadius * _OuterRadius;
        }
    }
}