using System.Collections.Generic;
using UnityEngine;


namespace Cocoon.Auido
{
    public class SystemSoundController : AudioBaseCtrl
    {
        protected override void LateAwake()
        {
            _CType = CtrlType.SystemSound;
            _Loop = false;
            _NeedFade = false;
            _FadeSpeed = 0.0f;
            is3D = false;
            _InnerRadius = 0.1f;
            _OuterRadius = 0.1f;
            _GabageWaitTimeMultiple = 1;
            volume = 1f;
        }
        public override bool CanPlay(Vector3 pos, AudioClip audioClip)
        {
            int count = 0;
            var it = _Audios.First;
            while (it != null)
            {
                Audio audio = it.Value;
                if (audio.ctrlType == _CType)
                    count++;
                it = it.Next;
            }
            if (count >= 5)
                return false;
            else
                return true;
        }
    }
}