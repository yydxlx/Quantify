using ClientBase;
using System;
using System.Collections.Generic;
using System.Resources;
using UnityEngine;

namespace Cocoon.Auido
{
    /// <summary>
    /// 音源管理器
    /// </summary>
    internal class AudioSourceMgr
    {
        public struct AudioSourceStruct
        {
            public int uid;
            public AudioSource source;
            public AudioSourceStruct(int uid, AudioSource source)
            {
                this.uid = uid;
                this.source = source;
            }
        }
        private GameObject _AudioGO1;
        private Dictionary<CtrlType, LinkedList<AudioSourceStruct>> _AllSourceList = new Dictionary<CtrlType, LinkedList<AudioSourceStruct>>();       
        private Dictionary<CtrlType, SpawnerPool> _AudioPoolDic = new Dictionary<CtrlType, SpawnerPool>();
        internal AudioSourceMgr()
        {
            //暂时所有root的对象池都用一个对象，后面对象扩充组件的时候，再另外设置新的GO.
            _AudioGO1 = new GameObject("AudioSourcePreFab");
            _AudioGO1.AddComponent<AudioSource>();
            _AudioGO1.SetActive(false);
            _AudioGO1.transform.parent = AudioMgr.Ins.audioRoot.transform;//GameObject.Find(ClientBaseConst.NameGlobals).transform;



        }
          // 获取一个音源
        public void Fetch(Audio user)
        {
            SpawnerPool pool;
            if (!_AudioPoolDic.TryGetValue(user.ctrlType, out pool))
            {
                // if(user.ctrlType == CtrlType.BGMSound || user.ctrlType == CtrlType.UISound || user.ctrlType == CtrlType.TalkSound || user.ctrlType == CtrlType.SkillSound)
                pool = new SpawnerPool("Audio" + user.ctrlType.ToString(), _AudioGO1);
                _AudioPoolDic.Add(user.ctrlType, pool);
            }

            GameObject go = pool.Spawn(user.position, Quaternion.identity, user.baseCtrl.transform);
            AudioSource source = go.GetComponent<AudioSource>();
            source.clip = user.clip;
            source.mute = user.baseCtrl.mute;
            source.maxDistance = user.baseCtrl._OuterRadius;
            source.minDistance = user.baseCtrl._InnerRadius;
            source.spatialBlend = user.baseCtrl.is3D ? 1.0f : 0.0f;
            user.source = source;
            source.Stop();
            AudioSourceStruct audioAsset = new AudioSourceStruct(user.audioHandleId, source);
            LinkedList<AudioSourceStruct> sourceList;
            if (!_AllSourceList.TryGetValue(user.ctrlType, out sourceList))
            {
                sourceList = new LinkedList<AudioSourceStruct>();
                _AllSourceList.Add(user.ctrlType, sourceList);
            }
            sourceList.AddLast(audioAsset);
        }
        
        // 删除一个音源
        internal void Destroy(int uid, CtrlType type)
        {
            SpawnerPool pool;
            if (!_AudioPoolDic.TryGetValue(type, out pool))
            {
                Debug.LogError("没有这个类型{0}".F(type));
            }
            LinkedList<AudioSourceStruct> sourceList;
            _AllSourceList.TryGetValue(type, out sourceList);
            var it = sourceList.First;
            while (it != null)
            {
                AudioSourceStruct sourceStruct = it.Value;
                it = it.Next;
                if (sourceStruct.uid == uid)
                {
                    sourceStruct.source.clip = null;
                    pool.Despawn(sourceStruct.source.gameObject);
                    sourceList.Remove(sourceStruct);
                    return;
                }
            }
        }
        public void Gabage()
        {
            foreach (KeyValuePair<CtrlType, LinkedList<AudioSourceStruct>> kv in _AllSourceList)
            {
                kv.Value.Clear();
            }
            foreach (KeyValuePair<CtrlType, SpawnerPool> kv in _AudioPoolDic)
            {
                kv.Value.DespawnAll();
            }
        }
        public void GabageByType(CtrlType type)
        {
            foreach (KeyValuePair<CtrlType, LinkedList<AudioSourceStruct>> kv in _AllSourceList)
            {
                if(type == kv.Key)
                {
                    kv.Value.Clear();
                    break;
                }
                    
            }
            foreach (KeyValuePair<CtrlType, SpawnerPool> kv in _AudioPoolDic)
            {
                if (type == kv.Key)
                {
                    kv.Value.DespawnAll();
                    break;
                }
            }
        }
    }
}