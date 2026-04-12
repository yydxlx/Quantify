using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cocoon.Auido
{
    public class TalkController : AudioBaseCtrl
    {
        protected override void LateAwake()
        {
            _CType = CtrlType.TalkSound;
            _Loop = false;
            _NeedFade = true;
            _FadeSpeed = 0.5f;
            is3D = false;
            _InnerRadius = 0.1f;
            _OuterRadius = 0.1f;
            _GabageWaitTimeMultiple = 1;
        }
        public override bool CanPlay(Vector3 pos, AudioClip audioClip)
        {
            return true;
        }

        public override void OnProcessAudio()
        {
            var it = _Audios.First;
            while (it != null)
            {
                Audio audio = it.Value;
                it = it.Next;
                RemoveAudioByGlobalID(audio.audioHandleId);
            }
        }
    }
}