using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[RequireComponent (typeof (Text))]
public class DisplayFPSJay : MonoBehaviour {

    private int FramesPerSec;
    private float frequency = 1.0f;
    private string fps;
    private Text m_refText;
    void Start () {
        m_refText = this.gameObject.GetComponent<Text> ();
        StartCoroutine (FPS ());
    }

    private IEnumerator FPS () {
        for (; ; ) {
            int lastFrameCount = Time.frameCount;
            float lastTime = Time.realtimeSinceStartup;
            yield return new WaitForSeconds (frequency);
            float timeSpan = Time.realtimeSinceStartup - lastTime;
            int frameCount = Time.frameCount - lastFrameCount;

            fps = string.Format ("FPS: {0}", Mathf.RoundToInt (frameCount / timeSpan));
            m_refText.text = fps;
        }
    }
}