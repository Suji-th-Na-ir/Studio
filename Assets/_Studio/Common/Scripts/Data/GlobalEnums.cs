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
        [EditorEnumField("Terra.Studio.GameStart", "Start")]
        GameStart,
        [EditorEnumField("Terra.Studio.TriggerAction", "Player")]
        OnPlayerCollide,
        [EditorEnumField("Terra.Studio.MouseAction", "OnClick")]
        OnClick,
        [EditorEnumField("Terra.Studio.Listener")]
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
        Move,
        MoveForever,
        MoveIncrementally,
        MoveIncrementallyForever,
        PingPong,
        PingPongForever
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
        Bootstrap,
        Editor,
        Runtime
    }
}
