using ClientBase;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Cocoon.Tween
{
    public class CocoonTweenEularRotationRealTime : CocoonTweenBase
    {
        [HideLabel]
        public CurveWrapper X = new CurveWrapper("X");
        [HideLabel]
        public CurveWrapper Y = new CurveWrapper("Y");
        [HideLabel]
        public CurveWrapper Z = new CurveWrapper("Z");
        private Vector3 InitAngle;
        public Vector3 TargetAngle;

        protected override void OnAnimationClipBegin()
        {
            base.OnAnimationClipBegin();
            InitAngle = Target.transform.eulerAngles;
        }
        protected override void OnInit()
        {
            base.OnInit();
        }

        protected override void OnProgress(float pProcess)
        {
            if (Target == null)
                return;
            Vector3 curAngle = Target.transform.eulerAngles;
            if (X.Enable)
                curAngle.x =
                    InitAngle.x.Lerp(TargetAngle.x, CocoonTweenUtility.GetCurveValue(X.Curve, pProcess));
            if (Y.Enable)
                curAngle.y =
                    InitAngle.y.Lerp(TargetAngle.y, CocoonTweenUtility.GetCurveValue(Y.Curve, pProcess));
            if (Z.Enable)
                curAngle.z =
                    InitAngle.z.Lerp(TargetAngle.z, CocoonTweenUtility.GetCurveValue(Z.Curve, pProcess));
            SetTargetValue(curAngle);
        }

        private void SetTargetValue(Vector3 pValue)
        {
            Target.transform.localRotation = Quaternion.Euler(pValue);
        }
    }
}

