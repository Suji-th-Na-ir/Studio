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

    public enum RepeatDirectionType
    {
        SameDirection,
        PingPong
    }

    public enum TransFormCopyValues
    {
        Position,
        Rotation,
        Scale,
        All
    }

    public enum BroadcastAt
    {
        Never,
        End,
        AtEveryPause
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
        Always,
        ConditionBased
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
        ResourceDB,
        [StringValue("DataManagerSO", typeof(RTDataManagerSO))]
        SystemData
    }

    public enum EditorObjectType
    {
        Default,
        SpawnPoint,
        Timer,
        Score,
        Checkpoint
    }

    public enum SwitchState
    {
        Off,
        On
    }

    public enum RequestType
    {
        Get,
        Post
    }

    public enum SaveState
    {
        [StringValue("Unsaved Changes")]
        UnsavedChanges,
        [StringValue("Changes Saved Offline")]
        ChangesSavedOffline,
        [StringValue("Saved To Cloud")]
        SavedToCloud,
        [StringValue("Saving...")]
        Saving,
        [StringValue("")]
        Empty
    }

    public enum TimerType
    {
        [AliasDrawer("Count Down")]
        CountDown,
        [AliasDrawer("Count Up")]
        CountUp
    }

    public enum InstantiateOn
    {
        [EditorEnumField("Terra.Studio.GameStart", "Start"), AliasDrawer("Game Starts")]
        GameStart,
        [EditorEnumField("Terra.Studio.Listener", ""), AliasDrawer("Broadcast Listened")]
        BroadcastListen,
        [EditorEnumField("Terra.Studio.InvokeAfter", "1"), AliasDrawer("Every x seconds")]
        EveryXSeconds
    }

    public enum SpawnWhere
    {
        [AliasDrawer("This Point")]
        CurrentPoint,
        [AliasDrawer("Random In Area")]
        Random
    }
}