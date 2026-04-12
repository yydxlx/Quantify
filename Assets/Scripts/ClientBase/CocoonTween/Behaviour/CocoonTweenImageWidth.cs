using ClientBase;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Cocoon.Tween
{
    public class CocoonTweenImageWidth : CocoonTweenBase
    {
        [HideLabel]
        public CurveWrapper Width = new CurveWrapper("Width");
        [HideLabel]
        public CurveWrapper Height = new CurveWrapper("Height");
        public Vector2 InitSizeDelta;
        public Vector2 TargetSizeDelta;
        private RectTransform targetRect;
        protected override void OnAnimationClipBegin() 
        {
            base.OnAnimationClipBegin();
            targetRect = Target.GetComponent<RectTransform>();
        }
        protected override void OnInit()
        {
            base.OnInit();
        }
        protected override void OnProgress(float pProcess)
        {
            if (targetRect == null)
                return;
            Vector2 curSizeDelta = targetRect.sizeDelta;
            if (Width.Enable)
                curSizeDelta.x =
                    InitSizeDelta.x.Lerp(TargetSizeDelta.x, CocoonTweenUtility.GetCurveValue(Width.Curve, pProcess));
            if (Height.Enable)
                curSizeDelta.y =
                    InitSizeDelta.y.Lerp(TargetSizeDelta.y, CocoonTweenUtility.GetCurveValue(Height.Curve, pProcess));

            SetTargetValue(curSizeDelta);
        }

        private void SetTargetValue(Vector2 pSizeDelta)
        {
            if (targetRect != null)
            {
                targetRect.sizeDelta = pSizeDelta;
            }
        }
#if UNITY_EDITOR
        [HorizontalGroup("PlayMode", 0.2f)]
        [Button("ResetSize", ButtonSizes.Medium), GUIColor(1, 1, 1, 1)]
        private void EditorReset()
        {
            InitSizeDelta = targetRect.sizeDelta;
        }
#endif


    }


   

}

