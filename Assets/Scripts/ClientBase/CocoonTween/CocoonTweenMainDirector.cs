using System.Collections.Generic;
using UnityEngine;
using ClientBase;
using Sirenix.OdinInspector;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Cocoon.Tween
{
    public class CocoonTweenMainDirector : MonoBehaviour
    {
//#if UNITY_EDITOR
        //[Title("Description", bold: true)]
        //[HideLabel]
        //[MultiLineProperty(5)]
        //public string Description = "请输入备注信息";
//#endif
        //[Title("Property")]
        //[OnValueChanged("OnTargetChange")]
        //[ValidateInput("HasRootValid", "需指定Root对象")]
        //[SerializeField] private GameObject root;
        [SerializeField] private float playSpeed = 1;
        [SerializeField] private bool autoPlay = false;
        public AnimationTimeScaleType timeScaleTime = AnimationTimeScaleType.IgnoreTimeScale;
        [SerializeField] private PlayMode playMode = PlayMode.Once;

        private float _passedAnimationTime = 0f;
        private AnimationPlayStatue curPlayStatue = AnimationPlayStatue.None;
        private System.Action finishCallBack = null;
        private System.Action finishBackCallBack = null;
        private float _backupPlaySpeed;
        private float _maxAnimationTime = 0.1f;
        private bool isInit = false;
        private readonly List<CocoonTweenBase> _allRmAnimator = new List<CocoonTweenBase>();
        private readonly List<CocoonTweenBase> _runningRmAnimators = new List<CocoonTweenBase>();
        
        //public GameObject Root { get { return root; } set{ root = value; } }

        public void Play(System.Action callback)
        {
            Assert.AssertTrue(curPlayStatue != AnimationPlayStatue.Play);
            if (playMode == PlayMode.Once)
                finishCallBack = callback;
            else
                Assert.AssertTrue(callback == null);
            OnInit();
            _passedAnimationTime = 0;
            curPlayStatue = AnimationPlayStatue.Play;
            playSpeed = 1;
            //DoUpdate();
        }

        public void PlayBack(System.Action callback)
        {
            playSpeed = -1;
           
            PlayFrom(1, callback);
        }

        public void PlayFrom(float ratio, System.Action callback)
        {
            Assert.AssertInRange(ratio, 0, 1);
            if (playMode == PlayMode.Once)
                finishBackCallBack = callback;
            else
                Assert.AssertTrue(callback == null);
            OnInit();
            _passedAnimationTime = _maxAnimationTime * ratio;
            curPlayStatue = AnimationPlayStatue.Play;
        }

        public void Stop()
        {
            curPlayStatue = AnimationPlayStatue.Stop;
            _passedAnimationTime = 0;
        }

        public void ResetAll()
        {
            OnInit();
            curPlayStatue = AnimationPlayStatue.None;
            playSpeed = 1;
            for (int i = 0; i < _allRmAnimator.Count; i++)
                _allRmAnimator[i].DoProgress(0);
        }

        public void FinishAll()
        {
            OnInit();
            curPlayStatue = AnimationPlayStatue.Stop;
            for (int i = 0; i < _allRmAnimator.Count; i++)
                _allRmAnimator[i].DoProgress(1);
        }
        public void Pause()
        {
            if (curPlayStatue != AnimationPlayStatue.Play)
                Debug.LogWarning("MainDirector: {0} pause failed, Current state is not Play.".F(gameObject.name));
            else
            {
                _backupPlaySpeed = playSpeed;
                playSpeed = 0;
                curPlayStatue = AnimationPlayStatue.Pause;
            }
        }
        public void Resume()
        {
            if (curPlayStatue != AnimationPlayStatue.Pause)
                Debug.LogWarning("MainDirector: {0} resume failed, Current state is not Pause.".F(gameObject.name));
            else
            {
                playSpeed = _backupPlaySpeed;
                curPlayStatue = AnimationPlayStatue.Play;
            }
        }
        public float GetPassedTime()
        {
            return _passedAnimationTime;
        }
        public void SetPassedTime(float passedTime)
        {
            _passedAnimationTime = passedTime;
            for (int i = 0; i < _allRmAnimator.Count; i++)
                _allRmAnimator[i].DoProgress(passedTime/_maxAnimationTime);
            playSpeed = 0;
            curPlayStatue = AnimationPlayStatue.Pause;
        }

        private void Awake()
        {
            if (autoPlay)
            {
                Play(null);
            }
        }

        public void Update()
        {
            //Debug.Log(curPlayStatue);
            DoUpdate();
        }

        private void OnInit()
        {
            //if (isInit == true)
            //   return;
            //isInit = true;
            LoadAllChildrenAnimatorCompontent();
        }

        private void LoadAllChildrenAnimatorCompontent()//添加所有子动画，设置动画时长
        {
            _allRmAnimator.Clear();
            _runningRmAnimators.Clear();

            CocoonTweenBase[] animatorBaseList = gameObject.GetComponentsInChildren<CocoonTweenBase>();
            if (animatorBaseList != null && animatorBaseList.Length > 0)
            {
                for (int j = 0; j < animatorBaseList.Length; j++)
                {
                    _allRmAnimator.Add(animatorBaseList[j]);
                    if ((animatorBaseList[j].AnimationDelay + animatorBaseList[j].AnimationLength) > _maxAnimationTime)
                        _maxAnimationTime = animatorBaseList[j].AnimationDelay + animatorBaseList[j].AnimationLength;
                }
            }

            Assert.AssertTrue(_maxAnimationTime> 0, "Animation total time must be larger than 0.");
        }
        #region Time Process
        private void DoUpdate()
        {
            //Debug.Log(curPlayStatue);
            if (curPlayStatue != AnimationPlayStatue.Play)
                return;

            DoTimePassed();

            switch (playMode)
            {
                case PlayMode.Once:
                    DoUpdateOnce();
                    break;
                case PlayMode.Loop:
                    DoUpdateLoop();
                    break;
                case PlayMode.PingPong:
                    DoUpdatePingPong();
                    break;
                case PlayMode.ExLoop:
                    DoUpdateLoopEx();
                    break;
                default:
                    Assert.Fail("Unexpected play mode: " + playMode);
                    break;
            }
        }

        private void DoTimePassed()
        {
            switch (timeScaleTime)
            {
                case AnimationTimeScaleType.IgnoreTimeScale:
                    _passedAnimationTime += Time.unscaledDeltaTime * playSpeed;
                    break;
                case AnimationTimeScaleType.TimeScale:
                    _passedAnimationTime += Time.deltaTime * playSpeed;
                    break;
                default:
                    Debug.LogError("Unexpected time scale type: " + timeScaleTime);
                    break;
            }
        }

        void DoUpdateOnce()
        {
            DoInLoopTime(_passedAnimationTime);

            if (playSpeed >= 0)
            {
                if (_passedAnimationTime >= _maxAnimationTime)
                {
                    curPlayStatue = AnimationPlayStatue.Stop;
                    finishCallBack.InvokeSafely();
                    finishCallBack = null;
                }
            }
            else if (playSpeed < 0)
            {
                if (_passedAnimationTime <= 0)
                {
                    curPlayStatue = AnimationPlayStatue.Stop;
                    finishBackCallBack.InvokeSafely();
                    finishBackCallBack = null;
                }
            }
          
        }

        void DoUpdateLoop()
        {
            var time = _passedAnimationTime % _maxAnimationTime;
            DoInLoopTime(time);
        }

        void DoUpdatePingPong()
        {
            var time = _passedAnimationTime % (2 * _maxAnimationTime);
            if (time >= _maxAnimationTime)
                time = 2 * _maxAnimationTime - time;
            DoInLoopTime(time);
        }

        void DoUpdateLoopEx()
        {
            float animationTime = _passedAnimationTime;

            for (int index = 0; index < _allRmAnimator.Count; index++)
            {
                var result = CompareAnimatorTimeValidInLoopEx(_allRmAnimator[index], animationTime);
                if (result.Item1)
                {
                    AddRunningAnimator(_allRmAnimator[index]);
                    _allRmAnimator[index].DoProgress(result.Item2);
                }
                else
                    RemoveRunningAnimatorn(_allRmAnimator[index], false);
            }
        }

        void DoInLoopTime(float animationTime )
        {
            for (int index = 0; index < _allRmAnimator.Count; index++)
            {
                var compare = CompareAnimatorTimeValid(_allRmAnimator[index], animationTime);
                if (compare == 0)
                    AddRunningAnimator(_allRmAnimator[index]);
                else
                    RemoveRunningAnimatorn(_allRmAnimator[index], compare == 1);
            }
            for (int i = 0; i < _runningRmAnimators.Count; i++)
            {
                _runningRmAnimators[i].DoProgress(GetAnimatorProcess(_runningRmAnimators[i], animationTime));
            }
        }

        /// <summary>
        /// Compares the animator time valid.
        /// </summary>
        /// <param name="pAnimatorBase">The p animator base.</param>
        /// <param name="animationTime">The animation time.</param>
        /// <returns>
        /// 0: In Range
        /// 1: Less Than
        /// 2: Larger Than
        /// </returns>
        private int CompareAnimatorTimeValid(CocoonTweenBase pAnimatorBase, float animationTime)
        {
            if (animationTime < pAnimatorBase.AnimationDelay)
                return 1;
            else if (animationTime > (pAnimatorBase.AnimationDelay + pAnimatorBase.AnimationLength))
                return 2;
            else
                return 0;
        }

        /// <summary>
        /// Compares the animator time valid in loop ex.
        /// </summary>
        /// <param name="pAnimatorBase">The p animator base.</param>
        /// <param name="animationTime">The animation time.</param>
        /// <returns>
        /// Val1: isValid
        /// Val2: progress
        /// </returns>
        private Tuple<bool, float> CompareAnimatorTimeValidInLoopEx(CocoonTweenBase pAnimatorBase, float animationTime)
        {
            if (animationTime >= pAnimatorBase.AnimationDelay)
            {
                float loopTime = pAnimatorBase.AnimationLength + pAnimatorBase.LoopInterval;
                float remainTime = animationTime - pAnimatorBase.AnimationDelay;
                int num = (int)(remainTime / loopTime);
                remainTime = remainTime - (loopTime) * num;
                if (remainTime >= 0 && remainTime <= pAnimatorBase.AnimationLength)
                {
                    var progress = remainTime / pAnimatorBase.AnimationLength;
                    return new Tuple<bool, float>(true, progress);
                }
            }

            return new Tuple<bool, float>(false, 0);
        }

        private void AddRunningAnimator(CocoonTweenBase pAnimatorBase)
        {
            if (pAnimatorBase != null)
            {
                if (!_runningRmAnimators.Contains(pAnimatorBase))
                {
                    _runningRmAnimators.Add(pAnimatorBase);
                    pAnimatorBase.DoAnimationClipBegin();
                }
            }
        }
        private void RemoveRunningAnimatorn(CocoonTweenBase pAnimatorBase, bool isResetToStart)
        {
            if (pAnimatorBase != null)
            {
                if (_runningRmAnimators.Contains(pAnimatorBase))
                {
                    _runningRmAnimators.Remove(pAnimatorBase);
                    pAnimatorBase.DoProgress(isResetToStart ? 0 : 1);
                    pAnimatorBase.DoAnimationClipEnd();
                }
            }
        }

        private float GetAnimatorProcess(CocoonTweenBase pAnimatorBase, float pTime)
        {

            if (pAnimatorBase.AnimationDelay + pAnimatorBase.AnimationLength > 0)
            {
                return (pTime - pAnimatorBase.AnimationDelay) / pAnimatorBase.AnimationLength;
            }
           
            return 0;
        }
        #endregion
#if UNITY_EDITOR



        #region Editor method
        private bool HasRootValid(GameObject go)
        {
            return go!=null;
        }
        #endregion


        private void EmpHook()
        {
            EmpDehook();
            if (Application.isEditor && !Application.isPlaying)
                EditorApplication.update += DoUpdate;
        }

        private void EmpDehook()
        {
            EditorApplication.update -= DoUpdate;
        }

        [HorizontalGroup("PlayMode", 0.2f)]
        [ContextMenu("Reset")]
        [Button("|<", ButtonSizes.Medium), GUIColor(0, 0, 1, 1)]
        private void EditorReset()
        {
            //(this as CocoonTweenMainDirector).EmpReset();
            EmpDehook();
            ResetAll();
        }

        [HorizontalGroup("PlayMode", 0.2f)]
        [ContextMenu("Play")]
        [Button(">", ButtonSizes.Medium), GUIColor(0, 1, 0, 1)]
        private void EditorPlay()
        {
            //(this as CocoonTweenMainDirector).EmpPlay();
            EmpHook();
            Play(null);
        }

        [HorizontalGroup("PlayMode", 0.2f)]
        [ContextMenu("Stop")]
        [Button("||", ButtonSizes.Medium), GUIColor(1, 0, 0, 1)]
        private void EditorStop()
        {
            //(this as CocoonTweenMainDirector).EmpStop();
            EmpDehook();
            Stop();
        }

        [HorizontalGroup("PlayMode", 0.2f)]
        [ContextMenu("Finish")]
        [Button(">|", ButtonSizes.Medium), GUIColor(0, 0, 1, 1)]
        private void EditorFinish()
        {
            //(this as CocoonTweenMainDirector).EmpFinish();
            EmpDehook();
            FinishAll();
        }

        [HorizontalGroup("PlayMode", 0.2f)]
        [ContextMenu("PlayBack")]
        [Button("<<", ButtonSizes.Medium), GUIColor(0, 0, 1, 1)]
        private void EditorPlayBack()
        {
            //(this as CocoonTweenMainDirector).EmpFinish();
            EmpHook();
            PlayBack(null);
        }

        //[HorizontalGroup("PlayMode", 0.2f)]
        //[ContextMenu("Static")]
        //[Button("G", ButtonSizes.Medium), GUIColor(0, 0, 1, 1)]
        //private void EditoStatic()
        //{
        //    (this as CocoonTweenMainDirector).EmpFinish();
        //}
        //private void OnTargetChange()
        //{
        //    if (Root == null)
        //        return;

        //    //var result = EditorUtility.DisplayDialog("提示", "是否自动刷新所有RmAnimator的Target对象", "确定", "取消");
        //    //if (!result)
        //    //    return;

        //    foreach (var animator in gameObject.GetComponentsInChildren<CocoonTweenBase>(true))
        //    {
        //        if (string.IsNullOrEmpty(animator.RelatedPath))
        //            continue;

        //        string relatedPath = animator.RelatedPath.Substring(1);
        //        var target = Root.transform.Find(relatedPath);
        //        if (target != null)
        //            animator.Target = target.gameObject;
        //        else
        //            Debug.LogError("Cannot  find  RelatedPath : " + relatedPath);
        //    }
        //}
        //private void guardedUpdate()
        //{
        //    Update();
        //}
#endif
    }
}

