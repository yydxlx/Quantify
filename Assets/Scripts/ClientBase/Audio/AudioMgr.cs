using System;
using System.Collections.Generic;
using System.Text;
using ClientBase;
using UnityEngine;

namespace Cocoon.Auido
{
    public enum CtrlType
    {
        BGMSound = 1,
        UISound = 2,
        TalkSound = 3,
        SkillSound = 4,
        LoopUISound = 5,
        SystemSound = 6,

    }

    /// <summary>
    /// 音频管理器
    /// </summary>
    public class AudioMgr : Singleton<AudioMgr>
    {
        //public static int InvalidAudioHandleId = 0;
        private Dictionary<CtrlType, AudioBaseCtrl> _AllAuidoCtrl;
        private Dictionary<int, Audio> _AllAudioDic = new Dictionary<int, Audio>();
        private int _AudioCurUid;
        private Stack<Audio> _AudioPool;
        
        public float voiceVolumeFactor { get; private set; } = 1.0f; /// 语音影响整体音量因子
        internal AudioSourceMgr audioSourceMgr;
        internal GameObject audioRoot { get; }
        /// <summary>
        /// 游戏整体音量
        /// </summary>
        public float Volume
        {
            get { return AudioListener.volume; }
            set { AudioListener.volume = value; }
        }

        /// <summary>
        /// 游戏耳朵
        /// </summary>
        public GameObject MainListener { get; }

        /// <summary>
        /// 游戏耳朵位置
        /// </summary>
        public Vector3 MainListenerPosition
        {
            get { return MainListener.transform.position; }
            set { MainListener.transform.position = value; }
        }
        public AudioMgr()
        {
            _AudioCurUid = 1;
            _AudioPool = new Stack<Audio>();
            _AllAuidoCtrl = new Dictionary<CtrlType, AudioBaseCtrl>();
            audioRoot = new GameObject("AudioRoot");
            audioRoot.transform.parent = GameObject.Find(ClientBaseConst.NameGlobals).transform;
            audioRoot.transform.position = Vector3.zero;
            MainListener = GameObject.Find("/MainCamera");//= new GameObject("MainListener");
            //MainListener.transform.SetParent(audioRoot.transform);
            //MainListener.transform.localPosition = Vector3.zero;
            MainListener.AddComponent<AudioListener>();
            //TryInit();
        }
        public void Init()
        {
            //register
            RegistComponent(CtrlType.BGMSound, typeof(BGMController));
            RegistComponent(CtrlType.UISound, typeof(UISoundController));
            RegistComponent(CtrlType.TalkSound, typeof(TalkController));
            RegistComponent(CtrlType.SkillSound, typeof(SkillSoundController));
            RegistComponent(CtrlType.LoopUISound, typeof(UIloopController));
            RegistComponent(CtrlType.SystemSound, typeof(SystemSoundController));
            audioSourceMgr = new AudioSourceMgr();
        }

        private void RegistComponent(CtrlType type, Type t)
        {
            GameObject go = new GameObject(type.ToString() + "Root");
            go.transform.SetParent(audioRoot.transform);
            AudioBaseCtrl component = go.AddComponent(t) as AudioBaseCtrl;
            _AllAuidoCtrl.Add(type, component);
        }

        internal void GabageToPool(Audio a)
        {
            _AudioPool.Push(a);
        }

        /// <summary>
        /// 播放一个声音
        /// </summary>
        /// <returns>一个音频播放实例</returns>
        public int PlayAudio(AudioClip clip, CtrlType ctrlType, Vector3 pos, Action onFinish)
        {
            AudioBaseCtrl v;
            if (!_AllAuidoCtrl.TryGetValue(ctrlType, out v))
            {
                Debug.LogError("没有类型" + ctrlType);
                return -1;
            }
            if (!v.CanPlay(pos, clip))
                return -1;
            if (clip == null)
            {
                //Debug.LogError("没有clip");
                return -1;
            }
            v.OnProcessAudio();

            Audio audio = _AudioPool.Count == 0 ? new Audio() : _AudioPool.Pop();
            _AudioCurUid++;
            audio.Reset(_AudioCurUid, clip, ctrlType, v, onFinish);
            audio.position = pos;
            
            v.AddAudio(audio);
            _AllAudioDic.Add(_AudioCurUid, audio);
            return audio.audioHandleId;
        }
        //准备释放一个音源(准备删除，之后要经过声音渐隐之类的过程)
        public void RemoveByUid(int uid)
        {
            Audio audio;
            if (!_AllAudioDic.TryGetValue(uid, out audio))
            {
                Debug.LogError("没有音乐,uid:{0}" + uid);
            }
            AudioBaseCtrl ctrl = _AllAuidoCtrl[audio.ctrlType];
            ctrl.RemoveAudioByGlobalID(uid);
        }
        //暂停音频
        public void PauseByUid(int uid)
        {
            Audio audio;
            if (!_AllAudioDic.TryGetValue(uid, out audio))
            {
                Debug.LogError("没有音乐,uid:{0}" + uid);
            }
            _AllAuidoCtrl[audio.ctrlType].PauseAudioByGlobalID(uid);
        }
        //暂停所有音频
        public void PauseAll()
        {
            foreach (KeyValuePair<CtrlType, AudioBaseCtrl> kv in _AllAuidoCtrl)
            {
                if(kv.Key != CtrlType.SystemSound)
                    kv.Value.PauseAllAudio();
            }
        }
        //继续所有音频
        public void ContinueAll()
        {
            foreach (KeyValuePair<CtrlType, AudioBaseCtrl> kv in _AllAuidoCtrl)
            {
                if(kv.Key != CtrlType.SystemSound)
                    kv.Value.PlayAllAudio();
            }
        }
        //继续播放音频
        public void ContinueByUid(int uid)
        {
            Audio audio;
            if (!_AllAudioDic.TryGetValue(uid, out audio))
            {
                Debug.LogError("没有音乐,uid:{0}" + uid);
            }
            AudioBaseCtrl ctrl = _AllAuidoCtrl[audio.ctrlType];
            ctrl.ContinueAudioByGlobalID(uid);
        }
        /// <summary>
        /// 删除声音
        /// </summary>
        internal void Destroy(Audio audio)
        {
            //audio.preStopTime = Time.time;
            audioSourceMgr.Destroy(audio.audioHandleId, audio.ctrlType);
            audio.Reset(0, null, CtrlType.UISound, null, null);
        }
        /// <summary>
        /// 清理所有音效
        /// </summary>
        public void RemoveAllAudios()
        {
            foreach (KeyValuePair<CtrlType, AudioBaseCtrl> kv in _AllAuidoCtrl)
            {
                kv.Value.RemoveAllAudios();
            }
            //_AudioPool.Clear();
            audioSourceMgr.Gabage();
        }
        /// <summary>
        /// 清理特定音效
        /// </summary>
        public void RemoveAudiosByCtrlType(CtrlType type)
        {
            foreach (KeyValuePair<CtrlType, AudioBaseCtrl> kv in _AllAuidoCtrl)
            {
                if(kv.Key == type)
                {
                    kv.Value.RemoveAllAudios();
                    break;
                }
            }
            //_AudioPool.Clear();
            audioSourceMgr.GabageByType(type);
        }

        public void SetVolumeByType(CtrlType type, float value)
        {
            foreach (KeyValuePair<CtrlType, AudioBaseCtrl> kv in _AllAuidoCtrl)
            {
                if (kv.Key == type)
                {
                    kv.Value.volume = value;
                    break;
                }
            }
        }

        public void HideVolumeByVoice()
        {
            voiceVolumeFactor = 0;
        }
        public void RestoreVolumeByVoice()
        {
            voiceVolumeFactor = 1;
        }
        public AudioClip GetCurBgmClip()
        {
            return (_AllAuidoCtrl[CtrlType.BGMSound] as BGMController).GetCurBgmClip();
        }
    }
}