using System;
using UnityEngine;
using UnityEngine.UI;
using PlayShifu.Terra;
using Object = UnityEngine.Object;

namespace Terra.Studio
{
    public class OscillateDrawComponent
    {
        private const string RESOURCE_FUNCTION_NAME = "Functions/OscillateFunction";
        private const string TOGGLE_LOC = "LoopToggle";

        public static (string, GameObject) Draw(Transform spawnparent, string referenceId)
        {
            var gameObject = Interop<EditorInterop>.Current.Resolve<StudioGameObjectsHolder>().Resolve(referenceId);
            var resObj = Resources.Load<GameObject>(RESOURCE_FUNCTION_NAME);
            var instance = Object.Instantiate(resObj, spawnparent) as GameObject;
            var toggle = Helper.FindDeepChild<Toggle>(instance.transform, TOGGLE_LOC);
            var behaviourInstance = new OscillatorEditorBehaviour<OscillateComponent>();
            gameObject.RegisterComponent<OscillateDrawComponent, OscillatorEditorBehaviour<OscillateComponent>>(behaviourInstance);
            toggle.onValueChanged.AddListener((value) =>
            {
                var behaviour = gameObject.GetBehaviour<OscillateDrawComponent, OscillateComponent>();
                behaviour?.OnComponentDataUpdated(new OscillateComponent()
                {
                    fromPoint = Vector3.zero,
                    toPoint = Vector3.one,
                    loop = value
                });
            });
            return (behaviourInstance.Id, instance);
        }
    }

    public abstract class StudioEditorBehaviour<T>
    {
        public abstract void OnComponentAttached();
        public abstract void OnComponentDataUpdated(T param);
    }

    public class OscillatorEditorBehaviour<OscillateComponent> : StudioEditorBehaviour<OscillateComponent>
    {
        private readonly string _id;
        private OscillateComponent componentData;
        public string Id => _id;

        public OscillatorEditorBehaviour()
        {
            componentData = default;
            _id = Guid.NewGuid().ToString("N");
        }

        public override void OnComponentAttached()
        {
            componentData = default;
        }

        public override void OnComponentDataUpdated(OscillateComponent param)
        {
            componentData = param;
            Debug.Log($"Data: {JsonUtility.ToJson(param)}");
        }
    }
}
