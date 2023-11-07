namespace Terra.Studio
{
    public class PlayerSpawnPoint : BaseBehaviour
    {
        public override string ComponentName => nameof(PlayerSpawnPoint);
        public override bool CanPreview => false;
        protected override bool CanBroadcast => false;
        protected override bool CanListen => false;
        protected override bool ShowComponentIcon => false;

        public override (string type, string data) Export()
        {
            return default;
        }

        public override void Import(EntityBasedComponent data) { }
    }
}
