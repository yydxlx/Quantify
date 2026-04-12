using System.Collections.Generic;
using UnityEngine;

namespace Cocoon.Auido
{
    public class SkillSoundController : AudioBaseCtrl
    {
        protected override void LateAwake()
        {
            _CType = CtrlType.SkillSound;
            _Loop = false;
            _NeedFade = false;
            _FadeSpeed = 0.0f;
            is3D = true;
            _InnerRadius = 3.0f;
            _OuterRadius = 40.0f;
            _GabageWaitTimeMultiple = 2;//对技能进行特殊处理（因为经常是技能在自动战斗，轮着来，所以需要把技能声音多缓存一会儿）
        }
        public override bool CanPlay(Vector3 pos, AudioClip audioClip)
        {
            //if (count >= 5)
            //    return false;
            //else
            if (InPlayRange(pos))
                return true;
            else

                return false;
        }

        public override void OnProcessAudio()
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

            if (count < 10)
            {
                volume = 1.0f;
            }
            else if (count >= 10)
            {
                volume = 0.7f;
            }
            else if (count >= 18)
            {
                volume = 0.5f;
            }
        }
    }
}