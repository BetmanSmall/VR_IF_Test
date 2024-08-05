using System;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class VRStartMenu : MonoBehaviour {
    public TMP_Text Text;
    private void Start() {
        Text = VRDebugUIBuilder.instance.AddLabel("Select Sample Scene").GetComponent<TMP_Text>();
        int n = UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings;
        for (int i = 0; i < n; ++i) {
            string path = UnityEngine.SceneManagement.SceneUtility.GetScenePathByBuildIndex(i);
            var sceneIndex = i;
            string name = Path.GetFileNameWithoutExtension(path);
            path = path.Substring(0, path.LastIndexOf("/", StringComparison.Ordinal));
            VRDebugUIBuilder.instance.AddButton(path+"/"+name, () => LoadScene(sceneIndex));
        }
        VRDebugUIBuilder.instance.Show();
        SceneManager.activeSceneChanged += delegate(Scene arg0, Scene scene) {
            Text.text = scene.name;
        };
    }

    private void LoadScene(int idx) {
        // DebugUIBuilder.instance.Hide();
        Debug.Log("Load scene: " + idx);
        UnityEngine.SceneManagement.SceneManager.LoadScene(idx);
    }
}