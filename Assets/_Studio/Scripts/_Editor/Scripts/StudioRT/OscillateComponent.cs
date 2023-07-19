using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Terra.Studio;

namespace StudioRT
{
    public class OscillateComponent : MonoBehaviour, IComponent
    {
        public Vector3 fromPoint;
        public Vector3 toPoint;
        public bool loop = true;
        public bool onClick = false;

        public void ExportData()
        {
            Debug.Log("--- Oscilate Component --");
            Debug.Log("from point: " + fromPoint);
            Debug.Log("to point: " + toPoint);
            Debug.Log("Loop: " + loop);
            Debug.Log("Enable on click " + onClick);

            var resp = SelectionHandler.Get.GetSceneData(this);
            Interop<SystemInterop>.Current.Resolve<CrossSceneDataHolder>().Set(resp);
        }
    }
}