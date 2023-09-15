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
        [EditorEnumField("Terra.Studio.GameStart", "Start"),DisplayName("Game Starts")]
        GameStart,
        [EditorEnumField("Terra.Studio.TriggerAction", "Player"),DisplayName("Player Touches")]
        OnPlayerCollide,
        [EditorEnumField("Terra.Studio.MouseAction", "OnClick"),DisplayName("Clicked")]
        OnClick,
        [EditorEnumField("Terra.Studio.Listener"),DisplayName("Broadcast Listened")]
        BroadcastListen
    }

    public enum Direction
    {
        Clockwise,
        AntiClockwise
    }

    public enum RotationType
    {
        [DisplayName("Rotate Once")]
        RotateOnce,
        [DisplayName("Rotate Forever")]
        RotateForever,
        Oscillate,
        [DisplayName("Oscillate Forever")]
        OscillateForever,
        [DisplayName("StepWise")]
        IncrementallyRotate,
        [DisplayName("StepWise Forever")]
        IncrementallyRotateForever
    }

    public enum TranslateType
    {
        Move,
        [DisplayName("Move Forever")]
        MoveForever,
        [DisplayName("StepWise")]
        MoveIncrementally,
        [DisplayName("StepWise Forever")]
        MoveIncrementallyForever,
        PingPong,
        [DisplayName("PingPong Forever")]
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

    public enum BroadcastAtForPushObjects
    {
        Never,
        OnPushStart,
        OnPushEnd
    }

    public enum Listen
    {
        Once,
        Always
    }

    public enum StudioState
    {
        Bootstrap,
        Editor,
        Runtime
    }

    public enum GameEndState
    {
        Win,
        Lose
    }

    public enum LoadFor
    {
        [StringValue("System/")]
        System,
        [StringValue("Editortime/")]
        Editortime,
        [StringValue("Runtime/")]
        Runtime,
        [StringValue("Common/")]
        Common
    }

    public enum ResourceTag
    {
        [StringValue("SystemSettings", typeof(SystemConfigurationSO))]
        SystemConfig,
        [StringValue("Player", typeof(UnityEngine.GameObject))]
        Player,
        [StringValue("SOs/Presets/", typeof(UnityEngine.Object))]
        ComponentPresets,
        [StringValue("ResourceDB", typeof(ResourceDB))]
        ResourceDB
    }

    public enum EditorObjectType
    {
        Default,
        SpawnPoint,
        Timer,
        Score
    }

    public enum SwitchState
    {
        Off,
        On
    }
}
