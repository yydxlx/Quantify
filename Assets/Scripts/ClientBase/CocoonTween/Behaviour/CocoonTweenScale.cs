using ClientBase;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Cocoon.Tween
{
    public class CocoonTweenScale : CocoonTweenBase
    {
        [HideLabel]
        public CurveWrapper X = new CurveWrapper("X");
        [HideLabel]
        public CurveWrapper Y = new CurveWrapper("Y");
        [HideLabel]
        public CurveWrapper Z = new CurveWrapper("Z");
        public Vector3 InitScale;
        public Vector3 TargetScale;
        protected override void OnAnimationClipBegin() 
        {
            base.OnAnimationClipBegin();
           
        }
        protected override void OnInit()
        {
            base.OnInit();
        }
        protected override void OnProgress(float pProcess)
        {
            if (Target == null)
                return;
            var curScale = Target.transform.localScale;
            if (X.Enable)
                curScale.x =
                    InitScale.x.Lerp(TargetScale.x, CocoonTweenUtility.GetCurveValue(X.Curve, pProcess));
            if (Y.Enable)
                curScale.y =
                    InitScale.y.Lerp(TargetScale.y, CocoonTweenUtility.GetCurveValue(Y.Curve, pProcess));
            if (Z.Enable)
                curScale.z =
                    InitScale.z.Lerp(TargetScale.z, CocoonTweenUtility.GetCurveValue(Z.Curve, pProcess));
            SetTargetValue(curScale);
        }

        private void SetTargetValue(Vector3 pValue)
        {
            if (Target != null)
            {
                Target.transform.localScale = pValue;
            }
        }
#if UNITY_EDITOR
        [HorizontalGroup("PlayMode", 0.2f)]
        [Button("ResetSca", ButtonSizes.Medium), GUIColor(1, 1, 1, 1)]
        private void EditorReset()
        {
            InitScale = Target.transform.localScale;
        }
#endif


    }


   

}

