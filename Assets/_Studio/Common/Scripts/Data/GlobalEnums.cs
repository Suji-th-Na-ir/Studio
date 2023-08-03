namespace Terra.Studio
{
    public enum Axis
    {
        X,
        Y,
        Z
    }

    public enum StartOn
    {
        None,
        GameStart,
        OnPlayerCollide,
        OnClick,
        BroadcastListen
    }

    public enum Direction
    {
        Clockwise,
        AntiClockwise
    }

    public enum RotationType
    {
        RotateOnce,
        RotateForever,
        Oscillate,
        OscillateForever,
        IncrementallyRotate,
        IncrementallyRotateForever
    }

    public enum TranslateType
    {
        TargetDirection,
        Oscillate
    }

    /// <summary>
    /// 1. For X default value is 1
    /// 2. For Forever hardcode int.MaxValue
    /// </summary>
    public enum RepeatType
    {
        XTimes,
        Forever
    }

    public enum BroadcastAt
    {
        Never,
        AtEveryInterval,
        End
    }

    public enum StudioState
    {
        Editor,
        Runtime
    }
}
