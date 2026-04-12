using ClientBase;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Cocoon.Tween
{
    public class CocoonTweenEularRotation : CocoonTweenBase
    {
        [HideLabel]
        public CurveWrapper X = new CurveWrapper("X");
        [HideLabel]
        public CurveWrapper Y = new CurveWrapper("Y");
        [HideLabel]
        public CurveWrapper Z = new CurveWrapper("Z");
        public Vector3 InitAngle;
        public Vector3 TargetAngle;
        public bool InHalfDegree = true;

        protected override void OnAnimationClipBegin()
        {
            base.OnAnimationClipBegin();
            
        }
        protected override void OnInit()
        {
            base.OnInit();
            if (InHalfDegree)
                TargetAngle = _set_euler_into_half_degree(TargetAngle, InitAngle);
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

        public Vector3 _set_euler_into_half_degree(Vector3 targetAngle, Vector3 sourceAngle)
        {
            float x = _set_degree_into_half_degree(targetAngle.x, sourceAngle.x);
            float y = _set_degree_into_half_degree(targetAngle.y, sourceAngle.y);
            float z = _set_degree_into_half_degree(targetAngle.z, sourceAngle.z);
            return new Vector3(x, y, z);
        }

        private float _set_degree_into_half_degree(float targetDegree, float sourceDegree)
        {
            while (Mathf.Abs(targetDegree - sourceDegree) > 180.0f)
            {
                if (targetDegree < sourceDegree)
                    targetDegree += 360.0f;
                else
                    targetDegree -= 360.0f;
            }
            return targetDegree;
        }
#if UNITY_EDITOR
        [HorizontalGroup("PlayMode", 0.3f)]
        [Button("ResetRotation", ButtonSizes.Medium), GUIColor(1, 1, 1, 1)]
        private void EditorReset()
        {
            InitAngle = Target.transform.rotation.eulerAngles;
            TargetAngle = _set_euler_into_half_degree(TargetAngle, InitAngle);
        }
#endif
    }
}

