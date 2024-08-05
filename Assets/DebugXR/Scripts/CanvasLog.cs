using System.Collections.Generic;
using UnityEngine;

public class CanvasLog : MonoBehaviour {
    public GameObject content;
    private static List<CanvasLog> scripts;
    private static CanvasLog current;
    public GameObject[] DebugLogPanelsGameObjects;
    private GameObject movablePanel;

    private void Start() {
        enabled = false;
        if (scripts == null) {
            scripts = new List<CanvasLog>();
            current = this;
            current.enabled = true;
        }
        scripts.Add(this);
        movablePanel = gameObject.transform.GetChild(0).gameObject;
        // if (Application.isEditor || !Debug.isDebugBuild) {
            // gameObject.SetActive(false);
        // }
    }

    public void OnOffCanvasLog() {
        current.gameObject.SetActive(!current.gameObject.activeSelf);
    }

    public void MoveContentToUp() {
        content.transform.localPosition += content.transform.parent.up * 20f;
    }

    public void MoveContentToDown() {
        content.transform.localPosition -= content.transform.parent.up * 10f;
    }
}
