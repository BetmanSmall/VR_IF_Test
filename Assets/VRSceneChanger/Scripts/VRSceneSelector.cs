using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;

#endif

public class VRSceneSelector : MonoBehaviour {
    private static VRSceneSelector instance;
    private string main = "";
    private List<string> scenes;

    private void Awake() {
        if (instance != null) {
            enabled = false;
            DestroyImmediate(this);
            return;
        }
        instance = this;
        if (scenes != null) scenes.Clear();
        else scenes = new List<string>();
        int overflow = 100, count = 0;
        while (overflow-- > 0) {
            try {
                var path = SceneUtility.GetScenePathByBuildIndex(count++);
                if (string.IsNullOrEmpty(path)) break;
                //int start = path.LastIndexOf("/");
                //Debug.Log(start + " " + path.Length + " " + path);
                //string name = path.Substring(start, path.Length - start);
                if (string.IsNullOrEmpty(main)) main = path;
                scenes.Add(path);
            }
            catch (System.Exception e) {
                e.ToString();
                break;
            }
        }
    }
}

#if UNITY_EDITOR

[CustomEditor(typeof(VRSceneSelector))]
public class VRSceneSelectorEditor : Editor {
    //[InitializeOnLoadMethod]
    //static void Hook() => AssemblyReloadEvents.beforeAssemblyReload += UpdateSceneList;
    //static void UpdateSceneList()
    //{
    //    AssemblyReloadEvents.beforeAssemblyReload -= UpdateSceneList;
    //    if( staticTarget != null ) staticTarget.scenes = GetEnabledScenes();
    //}

    static VRSceneSelector staticTarget = null;

    static bool foldout = true;
    static bool arrange = false;

    // SerializedProperty header;

    private void OnEnable() {
        if (Application.isPlaying) return;

        staticTarget = (VRSceneSelector) target;

        // header = serializedObject.FindProperty("header");

        PrependFirstScene();

        //UpdateTargetScenes();
        //UpdateTargetLoader();
    }

    #region GUI

    public override void OnInspectorGUI() {
        if (Application.isPlaying) {
            PlayModeGUI();
            return;
        }

        InitStyles();

        GUILayout.Space(5);

        serializedObject.Update();
        // EditorGUILayout.PropertyField(header);
        serializedObject.ApplyModifiedProperties();

        GUILayout.Space(5);

        if (GUILayout.Button("Find and Add all scenes")) {
            IncludeAllScenesToBuild();

            PrependFirstScene();

            foldout = true;

            return;
        }

        GUILayout.Space(5);

        var current = GetCurrentScenePath();

        var current_scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(current);

        bool allowSceneObjects = !EditorUtility.IsPersistent(current_scene);

        using (new GUILayout.HorizontalScope()) {
            GUILayout.Label("Main Scene", EditorStyles.boldLabel);

            EditorGUILayout.ObjectField(current_scene, typeof(SceneAsset), allowSceneObjects);
        }

        EditorGUILayout.HelpBox("Current scene is the main scene that holds the navigation options", MessageType.None);

        GUILayout.Space(10);

        foldout = EditorGUILayout.Foldout(foldout, "Edit Scenes", true);

        if (!foldout) return;

        GUILayout.Space(5);
        GUILayout.Label(GUIContent.none, GUI.skin.horizontalSlider);
        GUILayout.Space(20);

        using (new GUILayout.HorizontalScope()) {
            if (GUILayout.Button("Enable All", EditorStyles.miniButtonLeft)) {
                SetBuildActiveAll(true);
            }
            if (GUILayout.Button("Disable All", EditorStyles.miniButtonRight)) {
                SetBuildActiveAll(false);
                ToggleBuildActive(0, true);
            }
        }

        GUILayout.Space(5);

        arrange = EditorGUILayout.ToggleLeft("Rearrange", arrange);

        GUILayout.Space(5);

        using (new GUILayout.VerticalScope(EditorStyles.helpBox)) SceneListGUI();
    }

    void PlayModeGUI() {
        EditorGUILayout.HelpBox("Component changes locked during playmode", MessageType.Warning);
    }

    void SceneListGUI() {
        var current = GetCurrentScenePath();
        var scenes = EditorBuildSettings.scenes;
        var lenght = scenes.Length;

        for (var i = 0; i < lenght; ++i) {
            var scene = scenes[i];
            var path = scene.path;

            if (path == current) continue;

            var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);

            using (new GUILayout.HorizontalScope()) {
                bool buildCheck = EditorGUILayout.ToggleLeft(GUIContent.none, scene.enabled, GUILayout.Width(15));

                if (buildCheck != scene.enabled) ToggleBuildActive(i, buildCheck);

                GUILayout.Space(10);

                // GUILayout.TextArea( sceneAsset.name );
                EditorGUILayout.ObjectField(sceneAsset, typeof(SceneAsset), false);
                //if(GUILayout.Button( "Select", EditorStyles.miniButtonLeft, GUILayout.Width( 50 ) ))
                //    EditorGUIUtility.PingObject( sceneAsset );

                if (arrange) {
                    using (new EditorGUI.DisabledGroupScope(i < 2)) // 1st one at 0 is main 
                        if (GUILayout.Button("▲", sBtnUp, GUILayout.Width(25)))
                            SwapBuildSceneIndexes(i, i - 1);

                    using (new EditorGUI.DisabledGroupScope(i == lenght - 1))
                        if (GUILayout.Button("▼", sBtnDown, GUILayout.Width(25)))
                            SwapBuildSceneIndexes(i, i + 1);
                }

                if (GUILayout.Button("X", sBtnDel, GUILayout.Width(25))) {
                    RemoveScene(path);
                    //UpdateTargetScenes();
                    return;
                }
            }
        }
    }

    #endregion

    #region Build Scenes - Methods

    void PrependFirstScene() {
        var build = GetBuildScenes();

        var current = GetCurrentScenePath();

        var index = build.ToList().IndexOf(current);

        // Auto add current scene to build settings if missing 

        if (index == -1) Prepend(current);

        // Swap if main scene is not first

        else if (index > 0) SwapBuildSceneIndexes(0, index);
    }

    void SwapBuildSceneIndexes(int a, int b) {
        var tmp = EditorBuildSettings.scenes.ToList();
        var t = tmp[a];
        tmp[a] = tmp[b];
        tmp[b] = t;
        EditorBuildSettings.scenes = tmp.ToArray();
    }

    void ToggleBuildActive(int build_index, bool active) {
        var list = EditorBuildSettings.scenes.ToList();
        list[build_index].enabled = active;
        EditorBuildSettings.scenes = list.ToArray();
    }

    void SetBuildActiveAll(bool active) {
        var list = EditorBuildSettings.scenes.ToList();
        list.ForEach(scene => scene.enabled = active);
        EditorBuildSettings.scenes = list.ToArray();
    }

    void Prepend(string path) {
        EditorBuildSettings.scenes = (new EditorBuildSettingsScene[] {
                new EditorBuildSettingsScene(path, true)
            })
            .Concat(EditorBuildSettings.scenes.ToList())
            .ToArray();
    }

    static List<string> GetEnabledScenes() => EditorBuildSettings.scenes
        .Where(s => s.enabled).Select(s => s.path).ToList();

    //void UpdateTargetScenes() => ((VRSceneSelector)target).scenes = GetEnabledScenes();

    //void UpdateTargetLoader() => ((VRSceneSelector)target).loader = GetCurrentScenePath();

    void RemoveScene(string path) {
        var scenes = EditorBuildSettings.scenes.ToList();
        scenes.Remove(scenes.Where(x => x.path == path).First());
        EditorBuildSettings.scenes = scenes.ToArray();
    }

    string GetCurrentScenePath() => SceneManager.GetActiveScene().path;

    string[] GetAllScenes() {
        // source : https://github.com/Demkeys/SceneLoaderWindow/blob/master/SceneLoaderWindow.cs#L59
        return AssetDatabase.FindAssets("t:scene")
            .Select(x => AssetDatabase.GUIDToAssetPath(x))
            .ToArray();
    }

    string[] GetBuildScenes() {
        return EditorBuildSettings.scenes.Select(s => s.path).ToArray();
    }

    void IncludeAllScenesToBuild() {
        var build = GetBuildScenes();

        EditorBuildSettings.scenes = GetAllScenes()
            .Select(x => new EditorBuildSettingsScene(x, build.Contains(x)))
            .ToArray();
    }

    #endregion

    #region GUI Styles

    GUIStyle sBtnUp, sBtnDown, sBtnDel;

    void InitStyles() {
        if (sBtnUp != null) return;
        sBtnUp = new GUIStyle(EditorStyles.miniButtonLeft);
        sBtnUp.fontSize -= 2;
        sBtnDown = new GUIStyle(EditorStyles.miniButtonMid);
        sBtnDown.fontSize -= 2;
        sBtnDel = new GUIStyle(EditorStyles.miniButtonRight);
        sBtnDel.fontStyle = FontStyle.Bold;
    }

    #endregion
}

#endif
