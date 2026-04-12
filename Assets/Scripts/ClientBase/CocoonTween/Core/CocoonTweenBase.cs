using System;
using Sirenix.OdinInspector;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Cocoon.Tween
{
    public class CocoonTweenBase : MonoBehaviour
    {
        [SerializeField]
        //[OnValueChanged("OnTargetChange")]
        private GameObject target;
        public GameObject Target {
            get {
                return target;
            }
            set {
                if (value != target)
                    target = value;
            }
        }
        [SerializeField]
        [ReadOnly]
        private string relatedPath;

        [InfoBox("前置等待时间")]
        [SuffixLabel("seconds", Overlay = true)]
        [HorizontalGroup("Time", 0.3f)]
        [HideLabel]
        [MinValue(0)]
        public float AnimationDelay = 0f;

        [InfoBox("动画时长")]
        [SuffixLabel("seconds", Overlay = true)]
        [HorizontalGroup("Time", 0.4f)]
        [HideLabel]
        [MinValue(0.01f)]
        public float AnimationLength = 0.5f;

        [InfoBox("循环间隔时间(LoopEx模式下生效)")]
        [SuffixLabel("seconds", Overlay = true)]
        [HorizontalGroup("Time", 0.3f)]
        [HideLabel]
        [MinValue(0)]
        public float LoopInterval = 0.0f;

        private bool _isInited = false;

        public string RelatedPath
        {
            get { return relatedPath; }
            set { relatedPath = value; }
        }

        protected virtual void OnInit(){ }
        protected virtual void OnAnimationClipBegin() { }
        protected virtual void OnAnimationClipEnd() { }
        protected virtual void OnProgress(float pProcess){}

        public void DoProgress(float progress)
        {
            if (!_isInited)
            {
                OnInit();
                _isInited = true;
            }

            try
            {
                OnProgress(progress);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public void DoAnimationClipBegin()
        {
            if (!_isInited)
            {
                OnInit();
                _isInited = true;
            }

            try
            {
                OnAnimationClipBegin();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public void DoAnimationClipEnd()
        {
            if (!_isInited)
            {
                OnInit();
                _isInited = true;
            }

            try
            {
                OnAnimationClipEnd();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

//        private void OnTargetChange()
//        {
//#if UNITY_EDITOR
//            if (Application.isPlaying)
//                return;
//            var mainDirector = CocoonTweenUtility.FindMainDirector(this);
//            if (mainDirector == null)
//            {
//                EditorUtility.DisplayDialog("错误", "没有找到MainDirector", "确定");
//                return;
//            }

//            string relatedPath;
//            if (!CocoonTweenUtility.FindRelatedPath(Target.transform, mainDirector.Root.transform, out relatedPath))
//            {
//                EditorUtility.DisplayDialog("错误", "当前节点不在MainDirector的Target节点下", "确定");
//                return;
//            }

//            RelatedPath = relatedPath;
//#endif
//        }
    }

    [Serializable]
    public class CurveWrapper
    {
        public CurveWrapper(string name)
        {
            Name = name;
        }

        [HideLabel, LabelWidth(50)] [HorizontalGroup(20)]
        [ReadOnly]
        public string Name;

        [HideLabel, LabelWidth(50)]
        [HorizontalGroup(20)]
        public bool Enable = true;

        [HideLabel, LabelWidth(150)]
        [HorizontalGroup()]
        public AnimationCurve Curve;

        //[TableColumnWidth(50)]
        [HorizontalGroup(50)]
        public void Disable()
        {
            Curve = new AnimationCurve();
            Enable = false;
        }
    }
}

