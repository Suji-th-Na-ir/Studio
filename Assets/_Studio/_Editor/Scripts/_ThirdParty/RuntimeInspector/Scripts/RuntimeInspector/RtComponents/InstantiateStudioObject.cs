using System.Linq;
using Newtonsoft.Json;

namespace Terra.Studio
{
    [EditorDrawComponent("Terra.Studio.InstantiateStudioObject"), AliasDrawer("Instantiate")]
    public class InstantiateStudioObject : BaseBehaviour
    {
        //public enum SpawnWhere
        //{
        //    [AliasDrawer("This Point")]
        //    CurrentPoint,
        //    Random
        //}

        public override string ComponentName => nameof(InstantiateStudioObject);
        public override bool CanPreview => true;
        protected override bool CanBroadcast => false;
        protected override bool CanListen => true;

        //public string helloWorld;

        //[SerializeField] private InstantiateOn SpawnWhen;
        //[SerializeField] private string listenTo;
        //[SerializeField] private uint rounds;
        //[SerializeField] private bool repeatForever;
        //[SerializeField] private SpawnWhere spawnWhere;
        //[SerializeField] private uint howMany;

        public override (string type, string data) Export()
        {
            var virtualEntities = new VirtualEntity[transform.childCount];
            for (int i = 0; i < virtualEntities.Length; i++)
            {
                virtualEntities[i] = EditorOp.Resolve<SceneDataHandler>().GetVirtualEntity(transform.GetChild(0).gameObject, i, true);
                virtualEntities[i].shouldLoadAssetAtRuntime = false;
            }
            var components = new EntityBasedComponent[0];
            var attachedBehaviours = GetComponents<BaseBehaviour>();
            if (attachedBehaviours != null && attachedBehaviours.Length > 0)
            {
                components = attachedBehaviours.
                Where(x => x.ComponentName != ComponentName).
                Select(y =>
                {
                    var export = y.Export();
                    return new EntityBasedComponent()
                    {
                        type = export.type,
                        data = export.data
                    };
                }).
                ToArray();
            }
            var comp = new InstantiateStudioObjectComponent()
            {
                IsConditionAvailable = true,
                ConditionType = "Terra.Studio.Listener",
                ConditionData = "SpawnMe",
                IsBroadcastable = true,
                Broadcast = "Spawned Object!",
                canPlaySFX = false,
                canPlayVFX = false,
                childrenEntities = virtualEntities,
                componentsOnSelf = components
            };
            var type = EditorOp.Resolve<DataProvider>().GetCovariance(this);
            var data = JsonConvert.SerializeObject(comp);
            return (type, data);
        }

        public override void Import(EntityBasedComponent data)
        {
            var component = JsonConvert.DeserializeObject<InstantiateStudioObjectComponent>(data.data);
            EditorOp.Resolve<SceneDataHandler>().OnSceneSetupDone += () =>
            {
                ImportPostSceneSetup(component);
            };
        }

        private void ImportPostSceneSetup(InstantiateStudioObjectComponent component)
        {
            var componentsOnSelf = component.componentsOnSelf;
            if (componentsOnSelf != null && componentsOnSelf.Length > 0)
            {
                EditorOp.Resolve<SceneDataHandler>().AttachComponents(gameObject, component.componentsOnSelf);
            }
            if (component.childrenEntities != null && component.childrenEntities.Length > 0)
            {
                EditorOp.Resolve<SceneDataHandler>().HandleChildren(gameObject, component.childrenEntities);
            }
        }
    }
}