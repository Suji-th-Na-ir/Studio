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
        [EditorEnumField("Terra.Studio.GameStart", "Start"), AliasDrawer("Game Starts")]
        GameStart,
        [EditorEnumField("Terra.Studio.TriggerAction", "Player"), AliasDrawer("Player Touches")]
        OnPlayerCollide,
        [EditorEnumField("Terra.Studio.TriggerAction", "Other"), AliasDrawer("Other Object Touches")]
        OnObjectCollide,
        [EditorEnumField("Terra.Studio.MouseAction", "OnClick"), AliasDrawer("Clicked")]
        OnClick,
        [EditorEnumField("Terra.Studio.Listener"), AliasDrawer("Broadcast Listened")]
        BroadcastListen
    }

    public enum Direction
    {
        Clockwise,
        AntiClockwise
    }

    public enum RotationType
    {
        [AliasDrawer("Rotate Once")]
        RotateOnce,
        [AliasDrawer("Rotate Forever")]
        RotateForever,
        Oscillate,
        [AliasDrawer("Oscillate Forever")]
        OscillateForever,
        [AliasDrawer("StepWise")]
        IncrementallyRotate,
        [AliasDrawer("StepWise Forever")]
        IncrementallyRotateForever
    }

    public enum TranslateType
    {
        Move,
        [AliasDrawer("Move Forever")]
        MoveForever,
        [AliasDrawer("StepWise")]
        MoveIncrementally,
        [AliasDrawer("StepWise Forever")]
        MoveIncrementallyForever,
        PingPong,
        [AliasDrawer("PingPong Forever")]
        PingPongForever
    }

    public enum TransFormCopyValues
    {
        Position,
        Rotation,
        Scale,
        All
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
