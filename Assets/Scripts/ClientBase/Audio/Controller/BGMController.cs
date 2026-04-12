using System.Collections.Generic;
using UnityEngine;

namespace Cocoon.Auido
{
    public class BGMController : AudioBaseCtrl
    {
        protected override void LateAwake()
        {
            _CType = CtrlType.BGMSound;
            _Loop = true;
            _NeedFade = false;
            _FadeSpeed = 0.0f;
            is3D = false;
            _InnerRadius = 0.1f;
            _OuterRadius = 0.1f;
            _GabageWaitTimeMultiple = 1;
            volume = 0.3f;
        }
        public override bool CanPlay(Vector3 pos, AudioClip audioClip)
        {
            if (_Audios.Count > 0 && audioClip == _Audios.Last.Value.clip)
                return false;
            return true;
        }

        public override void OnProcessAudio()
        {
            LinkedListNode<Audio> it = _Audios.First;
            while (it != null)
            {
                Audio audio = it.Value;
                it = it.Next;
                RemoveAudioByGlobalID(audio.audioHandleId);
            }
        }
        public AudioClip GetCurBgmClip()
        {
            if (_Audios.Count > 0)
                return _Audios.Last.Value.clip;
            else
                return null;
        }
    }
}