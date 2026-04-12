using ClientBase;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace Cocoon.Tween
{
    public class CocoonTweenFade : CocoonTweenBase
    {
        [HideLabel]
        public CurveWrapper Fade = new CurveWrapper("Fade");
        public float InitFade;
        public float TargetFade;
        
        private Graphic _Img;
        private Graphic img
        {
            get
            {
                if (_Img == null)
                    _Img = Target.GetComponent<Graphic>(); 
                return _Img;
            }
        }
        protected override void OnAnimationClipBegin()
        {
            base.OnAnimationClipBegin();
            _Img = Target.GetComponent<Graphic>();
        }
        protected override void OnInit()
        {
            base.OnInit();
        }
        protected override void OnProgress(float pProcess)
        {
            if (Target == null)
                return;
            float curFade = img.color.a;
            if (Fade.Enable)
                curFade = InitFade.Lerp(TargetFade, CocoonTweenUtility.GetCurveValue(Fade.Curve, pProcess));
            SetTargetValue(curFade);
        }

        private void SetTargetValue(float pValue)
        {
            if (Target != null)
            {
                img.color = new Color(img.color.r, img.color.g, img.color.b, pValue);
            }
        }
#if UNITY_EDITOR
        [HorizontalGroup("PlayMode", 0.3f)]
        [Button("ResetFade", ButtonSizes.Medium), GUIColor(1, 1, 1, 1)]
        private void EditorReset()
        {
            InitFade = img.color.a;
        }
#endif
    }
}

