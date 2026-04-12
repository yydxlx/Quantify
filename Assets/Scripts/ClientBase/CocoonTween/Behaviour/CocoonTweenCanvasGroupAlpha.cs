
using ClientBase;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Cocoon.Tween
{
    public class AnimationGroupAlpha : CocoonTweenBase
    {
        [HideLabel]
        public CurveWrapper ACurve = new CurveWrapper("A");
        public float InitialValue, TargetValue;
        public bool UseInit = true;
        private float _startValue;
        private CanvasGroup _group;
        protected override void OnInit()
        {
            base.OnInit();
            if (Target!=null)
            {
                _group = Target.GetComponent<CanvasGroup>();
            }
        }

        protected override void OnAnimationClipBegin()
        {
            base.OnAnimationClipBegin();
            if (!UseInit)
            {
                _startValue = _group.alpha;
            }
            else
            {
                _startValue = InitialValue;
            }
        }


        protected override void OnProgress(float pProcess)
        {
            if (Target == null)
            {
                return;
            }
            if (ACurve.Enable)
                _group.alpha = _startValue.Lerp(TargetValue, CocoonTweenUtility.GetCurveValue(ACurve.Curve, pProcess));
        }

    }
}

