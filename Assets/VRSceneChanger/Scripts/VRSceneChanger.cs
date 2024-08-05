using System;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Management;
using Object = UnityEngine.Object;

public class VRSceneChanger : MonoBehaviour {
    public static string className;
    public static VRSceneChanger instance;

    public GameObject xrOriginSetup;
    public GameObject currentXROrigin;
    public GameObject xrDeviceSimulator;

    public InputActionReference prevSceneInputActionReference;
    public InputActionReference nextSceneInputActionReference;

    public List<Type> types = new List<Type>() { typeof(XROrigin), typeof(XRRig), typeof(BNG.TrackedDevice), typeof(BNG.XRTrackedPoseDriver),
        typeof(UnityEngine.SpatialTracking.TrackedPoseDriver), typeof(UnityEngine.InputSystem.XR.TrackedPoseDriver) };

    private void Awake() {
        if (instance != null) {
            enabled = false;
            DestroyImmediate(gameObject);
            return;
        }
        instance = this;
        SceneManager.activeSceneChanged += (scene1, scene2) => {
            Debug.Log("activeSceneChanged(); -- scene1.name:" + scene1.name);
            Debug.Log("activeSceneChanged(); -- scene2.name:" + scene2.name);
            Debug.Log("activeSceneChanged(); -- currentXROrigin:" + currentXROrigin);
        };
        SceneManager.sceneUnloaded += scene => {
            Debug.Log("sceneUnloaded(); -- scene.name:" + scene.name);
            Debug.Log("sceneUnloaded(); -- currentXROrigin:" + currentXROrigin);
        };
        SceneManager.sceneLoaded += (scene, mode) => {
            Debug.Log("sceneLoaded(); -- scene.name:" + scene.name);
            Debug.Log("sceneLoaded(); -- mode:" + mode);
            Debug.Log("sceneLoaded(); -- currentXROrigin:" + currentXROrigin);
            foreach(Type type in types) {
                Debug.Log("sceneLoaded(); -- type:" + type);
                Object[] obects = FindObjectsOfType(type);
                Debug.Log("sceneLoaded(); -- obects:" + obects);
                if (obects.Length != 0) {
                    return;
                }
            }
            foreach (Camera camera in Camera.allCameras) {
                if (camera != null) {
                    camera.gameObject.SetActive(false);
                }
            }
            currentXROrigin = Instantiate(xrOriginSetup);
            if (Application.isEditor) {
                Instantiate(xrDeviceSimulator);
            }
        };
        prevSceneInputActionReference.action.performed += delegate(InputAction.CallbackContext context) {
            Debug.Log("context.action:" + context.action);
            SceneChange(-1);
        };
        nextSceneInputActionReference.action.performed += delegate(InputAction.CallbackContext context) {
            Debug.Log("context.action:" + context.action);
            SceneChange(-1);
        };
    }

    private void Start() {
        DontDestroyOnLoad(gameObject);
        if (className == null) {
            className = gameObject.name;
        }
        Debug.Log("Start(); -- XRGeneralSettings.Instance.Manager.isInitializationComplete:" + XRGeneralSettings.Instance?.Manager?.isInitializationComplete);
    }

    private void SceneChange(int sceneChange) {
        if (sceneChange != 0) {
            GameObject[] dontDestroyOnLoadObjects = GetDontDestroyOnLoadObjects();
            foreach (GameObject dontDestroyOnLoadObject in dontDestroyOnLoadObjects) {
                Debug.Log("dontDestroyOnLoadObject:" + dontDestroyOnLoadObject, dontDestroyOnLoadObject);
                if (!dontDestroyOnLoadObject.name.Equals(className)) {
                    DestroyImmediate(dontDestroyOnLoadObject.gameObject);
                }
            }
            int activeSceneIdx = SceneManager.GetActiveScene().buildIndex;
            int nextSceneIdx = Wrap(activeSceneIdx + sceneChange, 0, SceneManager.sceneCountInBuildSettings - 1);
            SceneManager.LoadScene(nextSceneIdx, LoadSceneMode.Single);
        }
    }

    private int Wrap(int value, int min, int max) {
        if (value < min) {
            return max;
        } else if (value > max) {
            return min;
        }
        return value;
    }

    private static GameObject[] GetDontDestroyOnLoadObjects() {
        GameObject temp = null;
        try {
            temp = new GameObject();
            Object.DontDestroyOnLoad(temp);
            UnityEngine.SceneManagement.Scene dontDestroyOnLoad = temp.scene;
            Object.DestroyImmediate(temp);
            temp = null;
            return dontDestroyOnLoad.GetRootGameObjects();
        } finally {
            if (temp != null)
                Object.DestroyImmediate(temp);
        }
    }
}
