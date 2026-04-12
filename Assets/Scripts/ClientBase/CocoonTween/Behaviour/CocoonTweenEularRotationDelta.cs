using ClientBase;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Cocoon.Tween
{
    public class CocoonTweenEularRotationDelta : CocoonTweenBase
    {
        [HideLabel]
        public CurveWrapper X = new CurveWrapper("X");
        [HideLabel]
        public CurveWrapper Y = new CurveWrapper("Y");
        [HideLabel]
        public CurveWrapper Z = new CurveWrapper("Z");
        public Vector3 DeltaAngle;
        public bool InHalfDegree = true;
        private Vector3 startAngle;

        protected override void OnAnimationClipBegin()
        {
            base.OnAnimationClipBegin();
            InitEularRotation();
        }
        private void InitEularRotation()
        {
            if (Target == null)
                return;
            startAngle = Target.transform.rotation.eulerAngles;
        }

        protected override void OnProgress(float pProcess)
        {
            if (Target == null)
                return;
            Vector3 targetAngle = startAngle + DeltaAngle;
            Vector3 curAngle = Target.transform.eulerAngles;
            if (X.Enable)
                curAngle.x =
                    startAngle.x.Lerp(targetAngle.x, CocoonTweenUtility.GetCurveValue(X.Curve, pProcess));
            if (Y.Enable)
                curAngle.y =
                    startAngle.y.Lerp(targetAngle.y, CocoonTweenUtility.GetCurveValue(Y.Curve, pProcess));
            if (Z.Enable)
                curAngle.z =
                    startAngle.z.Lerp(targetAngle.z, CocoonTweenUtility.GetCurveValue(Z.Curve, pProcess));
            SetTargetValue(curAngle);
        }

        private void SetTargetValue(Vector3 pValue)
        {
            Target.transform.localRotation = Quaternion.Euler(pValue);
        }

        //public Vector3 _set_euler_into_half_degree(Vector3 targetAngle, Vector3 sourceAngle)
        //{
        //    float x = _set_degree_into_half_degree(targetAngle.x, sourceAngle.x);
        //    float y = _set_degree_into_half_degree(targetAngle.y, sourceAngle.y);
        //    float z = _set_degree_into_half_degree(targetAngle.z, sourceAngle.z);
        //    return new Vector3(x, y, z);
        //}

        //private float _set_degree_into_half_degree(float targetDegree, float sourceDegree)
        //{
        //    while (Mathf.Abs(targetDegree - sourceDegree) > 180.0f)
        //    {
        //        if (targetDegree < sourceDegree)
        //            targetDegree += 360.0f;
        //        else
        //            targetDegree -= 360.0f;
        //    }
        //    return targetDegree;
        //}

    }
}

