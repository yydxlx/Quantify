using System;
using UnityEngine.EventSystems;
//Editor By XiaoMai
//2019.8.12
namespace UnityEngine.UI
{
    public enum FrameState
    {
        None,
        Play,
        Stop,
        Pause
    }


    /// <summary>
    /// 序列帧动画
    /// </summary>
    public class FrameAnimation : UIBehaviour
    {
        public enum PlayModel
        {
            Once,           //播放一次后就自动停止
            Loop,           //循环播放
            LoopAndPause    //循环播放，循环一定次数后暂停
        }
        public Vector2 rowCols;// 行列数
        public int count;// 帧总数
        //public float duration;// 一次帧动画总数
        public bool playOnAwake;
        private bool raycastTarget = false;
        public PlayModel playModel;
        public Texture texture;// Texture对象
        private RawImage _RawImage;// RawImage对象
        private int _CurrentIndex;// 当前帧
        private Vector2 _CellSize;// 每帧大小，归一化
        private bool _Prepared;// 是否已经准备好
        private int _CurrentLoopIndex;// 当前循环Index
        private FrameState _CurrentState;// 当前播放状态
        private float _LifeTime = -1;// 生命周期，-1代表循环
        private float _BeginPlayTime = 0;// 开始播放时间
        private float _NextPlayTime = 0;// 下一次播放时间
        private float _InternalCheckTime = 1f / 30f;// 帧率
        private float _RunTime = 0;// 总的运行时间
        private CanvasRenderer _CanvasRenderer = null;// 渲染器
        private Action _OnFin;
        private int _CurrentRow// 当前行
        {
            get
            {
                return Mathf.FloorToInt(_CurrentIndex / (int)rowCols.y);
            }
        }
        private int _CurrentCols// 当前列
        {
            get
            {
                return _CurrentIndex % (int)rowCols.y;
            }
        }
        private Vector2 _CurrentUVPostion// 当前UV Position
        {
            get
            {
                Vector2 uvPos = new Vector2();
                uvPos.x = _CurrentCols / rowCols.y;
                uvPos.y = 1 - ((_CurrentRow + 1) / rowCols.x);
                return uvPos;
            }
        }
        private Rect _CurrentUVRect// 当前UV Rect
        {
            get
            {
                Rect uvRect = new Rect(_CurrentUVPostion, _CellSize);
                return uvRect;
            }
        }

        protected override void Awake()
        {
            base.Awake();
            _CanvasRenderer = this.gameObject.GetComponentInParent<CanvasRenderer>();
            if (playOnAwake)
                Play(null);
        }

        /// <summary>
        /// 播放
        /// </summary>
        public void Play(Action onFin)
        {
            if (playModel == PlayModel.Once)
                _OnFin = onFin;
            _CurrentState = FrameState.Play;
            if (_Prepared)
            {
                DoPlay();
            }
            else
            {
                InitTexture();
            }
            
        }

        /// <summary>
        /// 实际播放
        /// </summary>
        public void DoPlay()
        {
            _CurrentIndex = 0;
            _CurrentLoopIndex = 0;
            _RawImage.uvRect = _CurrentUVRect;
            _RawImage.enabled = true;
            //_InternalCheckTime = 1f/30f;
            _RunTime = 0;
            _BeginPlayTime = Time.time;
            _NextPlayTime = _BeginPlayTime;
        }

        public void PlayeAnim()
        {
            if (_CurrentState != FrameState.Play)
            {
                return;
            }
            ++_CurrentIndex;
            ++_CurrentLoopIndex;
            if (_CurrentIndex >= count)
            {
                switch (playModel)
                {
                    case PlayModel.Loop:
                        {
                            _CurrentIndex %= count;
                            break;
                        }
                    case PlayModel.LoopAndPause:
                        {
                            _CurrentIndex %= count;
                            break;
                        }
                    case PlayModel.Once:
                        {
                            Stop();
                            return;
                        }
                }
            }
            _RawImage.uvRect = _CurrentUVRect;
        }


        public void InitTexture()
        {
            _RawImage = GetComponent<RawImage>();
            if (_RawImage == null)
            {
                _RawImage = gameObject.AddComponent<RawImage>();
            }
            _RawImage.enabled = false;
            _RawImage.raycastTarget = raycastTarget;
            // _CurrentIndex = 0;
            // _CurrentLoopIndex = 0;
            // _RawImage.uvRect = _CurrentUVRect;
            AcceptSprite();
        }

        public void AcceptSprite()
        {
            // if (_CurrentState == FrameState.None || _CurrentState == FrameState.Stop)
            //     return;
            _RawImage.texture = texture;
            _Prepared = true;
            _CellSize = new Vector2(1 / rowCols.y, 1 / rowCols.x);
            // if (_CurrentState == FrameState.Play)
            // {
                DoPlay();
            // }
        }

        /// <summary>
        /// 暂停
        /// </summary>
        public void Pause()
        {
            _CurrentState = FrameState.Pause;
        }

        /// <summary>
        /// 继续
        /// </summary>
        public void Continue()
        {
            _CurrentState = FrameState.Play;
        }

        /// <summary>
        /// 停止。停止播放，但保留当前的RawImage数据，等待下一次Play
        /// </summary>
        public void Stop()
        {
            if (_CurrentState == FrameState.None || _CurrentState == FrameState.Stop)
                return;
            _OnFin?.Invoke();
            _CurrentState = FrameState.Stop;
            _CurrentIndex = 0;
            _RawImage.enabled = false;
        }

        /// <summary>
        /// 结束。停止并且清理当前RawImage数据。
        /// </summary>

        public void Clear()
        {
            texture = null;
            
            if (_RawImage != null)
            {
                _RawImage.texture = null;
                _RawImage.enabled = false;
            }
            _Prepared = false;
            _CurrentLoopIndex = 0;
            _CurrentState = FrameState.None;
        }

        void Update()
        {
            //不是播放状态，不进行任何处理
            if (_CurrentState != FrameState.Play)
                return;

            //裁剪掉的不进行处理
            if (_CanvasRenderer != null && _CanvasRenderer.cull)
                return;

            //资源未准备好或者rawImage禁用，直接返回
            if (_RawImage == null || !_RawImage.enabled)
                return;
            if (Time.time >= _NextPlayTime)
            {
                _NextPlayTime = Time.time + _InternalCheckTime;
                PlayeAnim();
            }
        }
    }
}
