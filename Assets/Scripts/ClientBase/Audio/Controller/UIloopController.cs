using System.Collections.Generic;
using UnityEngine;

namespace Cocoon.Auido
{
    public class UIloopController : AudioBaseCtrl
    {
        protected override void LateAwake()
        {
            _CType = CtrlType.LoopUISound;
            _Loop = true;
            _NeedFade = false;
            _FadeSpeed = 0.0f;
            is3D = false;
            _InnerRadius = 0.1f;
            _OuterRadius = 0.1f;
            _GabageWaitTimeMultiple = 1;
            volume = 0.25f;
        }
        public override bool CanPlay(Vector3 pos, AudioClip audioClip)
        {
            return true;
        }
    }
}