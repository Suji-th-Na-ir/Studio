using UnityEngine;
using Newtonsoft.Json;

namespace Terra.Studio
{
    [EditorDrawComponent("Terra.Studio.PlayerHealth"), AliasDrawer("Player Health")]
    public class PlayerHealth : BaseBehaviour
    {
        public override string ComponentName => nameof(PlayerHealth);
        public override bool CanPreview => false;
        protected override bool CanBroadcast => true;
        protected override bool CanListen => false;

        [Header("Player Health \t 100")]
        [SerializeField] private bool healthRegeneration;
        [SerializeField] private float regenerationPerSec;
        [Header("When Dead:")]
        [SerializeField] private Atom.Broadcast broadcast = new();
        protected override string[] BroadcasterRefs => new string[] { broadcast.broadcast };

        protected override void Awake()
        {
            base.Awake();
            broadcast.Setup(gameObject, this);
        }

        public override (string type, string data) Export()
        {
            var component = new PlayerHealthComponent()
            {
                IsBroadcastable = !string.IsNullOrEmpty(broadcast.broadcast),
                Broadcast = broadcast.broadcast,
                regenerateHealth = healthRegeneration,
                regenerationPerSec = Mathf.Abs(regenerationPerSec)
            };
            var json = JsonConvert.SerializeObject(component);
            var type = EditorOp.Resolve<DataProvider>().GetCovariance(this);
            return (type, json);
        }

        public override void Import(EntityBasedComponent data)
        {
            var component = JsonConvert.DeserializeObject<PlayerHealthComponent>(data.data);
            broadcast.broadcast = component.Broadcast;
            regenerationPerSec = component.regenerationPerSec;
            healthRegeneration = component.regenerateHealth;
            ImportVisualisation(broadcast.broadcast, null);
        }
    }
}