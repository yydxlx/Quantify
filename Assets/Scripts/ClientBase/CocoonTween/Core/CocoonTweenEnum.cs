namespace Cocoon.Tween
{
    /// <summary>
    /// Play mode
    /// </summary>
    public enum PlayMode:byte
    {
        Once = 0,
        PingPong = 1,
        Loop = 2,       // 时间线走过最长一个动画后会归零
        ExLoop = 3,     // 时间线一直会往前走
    }

    /// <summary>
    /// Animation tmime scale type
    /// </summary>
    public enum AnimationTimeScaleType:byte
    {
        IgnoreTimeScale = 0,
        TimeScale = 1,
    }

    /// <summary>
    /// Statue
    /// </summary>
    public enum AnimationPlayStatue:byte
    {
        None = 0,
        Play = 1,
        Stop = 2,
        Pause = 3,
    }

    public enum AnimType:byte
    {
        Idle,
        Running,
        Finished
    }


}
