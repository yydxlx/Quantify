using ClientBase;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Cocoon.Tween
{
    public class CocoonTweenMoveDelta : CocoonTweenBase
    {
        [HideLabel]
        public CurveWrapper X = new CurveWrapper("X");
        [HideLabel]
        public CurveWrapper Y = new CurveWrapper("Y");
        [HideLabel]
        public CurveWrapper Z = new CurveWrapper("Z");
        public Vector3 DeltaVector;
        public bool UseWorldSpacePosition = false;

        private Vector3 startPosition;

        protected override void OnAnimationClipBegin()
        {
            base.OnAnimationClipBegin();
            InitPos();
        }

        private void InitPos()
        {
            if (Target == null)
                return;
            if (UseWorldSpacePosition)
                startPosition = Target.transform.position;
            else
                startPosition = Target.transform.localPosition;
        }

        protected override void OnProgress(float pProcess)
        {
            if (Target == null)
                return;
            var curPosition = UseWorldSpacePosition ? Target.transform.position : Target.transform.localPosition;
            var tarPos = startPosition + DeltaVector;
            if (X.Enable)
                curPosition.x =
                    startPosition.x.Lerp(tarPos.x, CocoonTweenUtility.GetCurveValue(X.Curve, pProcess));
            if (Y.Enable)
                curPosition.y =
                    startPosition.y.Lerp(tarPos.y, CocoonTweenUtility.GetCurveValue(Y.Curve, pProcess));
            if (Z.Enable)
                curPosition.z =
                    startPosition.z.Lerp(tarPos.z, CocoonTweenUtility.GetCurveValue(Z.Curve, pProcess));
            SetTargetValue(curPosition);
        }

        private void SetTargetValue(Vector3 pValue)
        {
            if (UseWorldSpacePosition)
                Target.transform.position = pValue;
            else
                Target.transform.localPosition = pValue;
        }
    }
}

