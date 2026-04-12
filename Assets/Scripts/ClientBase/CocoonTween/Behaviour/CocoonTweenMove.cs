using ClientBase;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Cocoon.Tween
{
    public class CocoonTweenMove : CocoonTweenBase
    {
        [HideLabel]
        public CurveWrapper X = new CurveWrapper("X");
        [HideLabel]
        public CurveWrapper Y = new CurveWrapper("Y");
        [HideLabel]
        public CurveWrapper Z = new CurveWrapper("Z");
        public Vector3 InitialPosition;
        public Vector3 TargetPosition;
        public bool UseWorldSpacePosition = false;
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
            var curPosition = UseWorldSpacePosition ? Target.transform.position : Target.transform.localPosition;
            if (X.Enable)
                curPosition.x =
                    InitialPosition.x.Lerp(TargetPosition.x, CocoonTweenUtility.GetCurveValue(X.Curve, pProcess));
            if (Y.Enable)
                curPosition.y =
                    InitialPosition.y.Lerp(TargetPosition.y, CocoonTweenUtility.GetCurveValue(Y.Curve, pProcess));
            if (Z.Enable)
                curPosition.z =
                    InitialPosition.z.Lerp(TargetPosition.z, CocoonTweenUtility.GetCurveValue(Z.Curve, pProcess));
            SetTargetValue(curPosition);
        }

        private void SetTargetValue(Vector3 pValue)
        {
            if (Target != null)
            {
                if (UseWorldSpacePosition)
                    Target.transform.position = pValue;
                else
                    Target.transform.localPosition = pValue;
            }
        }
#if UNITY_EDITOR
        [HorizontalGroup("PlayMode", 0.2f)]
        [Button("ResetPos", ButtonSizes.Medium), GUIColor(1, 1, 1, 1)]
        private void EditorReset()
        {
            if (UseWorldSpacePosition)
                InitialPosition = Target.transform.position;
            else
                InitialPosition = Target.transform.localPosition;
        }
#endif
    }
}

