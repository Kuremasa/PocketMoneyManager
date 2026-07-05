using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary> WebGLネイティブ入力のEditorセットアップ </summary>
[InitializeOnLoad]
public static class WebGLNativeInputFieldSetup
{
    const string PrefabPath = "Assets/Scripts/Prefabs/DateInputField.prefab";
    const string ScenePath = "Assets/Scenes/Main.unity";
    const string DateInputDialogTitle = "日付 (yyyy-MM-dd)";
    const string AmountInputDialogTitle = "金額";
    const string NoteInputDialogTitle = "用途";

    static int _setupRetryCount;

    /// <summary> コンパイル後に未設定なら自動セットアップする </summary>
    static WebGLNativeInputFieldSetup() => EditorApplication.delayCall += TrySetup;

    /// <summary> Prefabとシーンへブリッジを設定する </summary>
    [MenuItem("Tools/Setup WebGL Native Input")]
    public static void SetupFromMenu()
    {
        _setupRetryCount = 0;
        SetupPrefab();
        SetupSceneInstances();
    }

    /// <summary> 未設定時のみブリッジを追加する </summary>
    static void TrySetup()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            return;
        }

        if (EditorApplication.isCompiling)
        {
            if (_setupRetryCount < 50)
            {
                _setupRetryCount++;
                EditorApplication.delayCall += TrySetup;
            }

            return;
        }

        _setupRetryCount = 0;

        if (!System.IO.File.Exists(PrefabPath))
        {
            return;
        }

        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        if (prefab == null)
        {
            return;
        }

        if (prefab.GetComponent<WebGLNativeTmpInputField>() == null)
        {
            SetupPrefab();
        }

        SetupSceneInstances();
    }

    /// <summary> DateInputField Prefabにブリッジを追加する </summary>
    static void SetupPrefab()
    {
        var prefabRoot = PrefabUtility.LoadPrefabContents(PrefabPath);
        var bridge = prefabRoot.GetComponent<WebGLNativeTmpInputField>();
        if (bridge == null)
        {
            bridge = prefabRoot.AddComponent<WebGLNativeTmpInputField>();
        }

        var inputField = prefabRoot.GetComponent<TMP_InputField>();
        ApplyBridgeSettings(bridge, inputField, DateInputDialogTitle);
        PrefabUtility.SaveAsPrefabAsset(prefabRoot, PrefabPath);
        PrefabUtility.UnloadPrefabContents(prefabRoot);
        AssetDatabase.SaveAssets();
    }

    /// <summary> Mainシーンの入力フィールドにダイアログタイトルを設定する </summary>
    static void SetupSceneInstances()
    {
        if (!System.IO.File.Exists(ScenePath))
        {
            return;
        }

        var activeScenePath = SceneManager.GetActiveScene().path;
        var sceneWasOpen = activeScenePath == ScenePath;
        if (!sceneWasOpen)
        {
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        }

        var sceneChanged = false;
        sceneChanged |= ConfigureInputField("DateInputField", DateInputDialogTitle);
        sceneChanged |= ConfigureInputField("AmountInputField", AmountInputDialogTitle);
        sceneChanged |= ConfigureInputField("NoteInputField", NoteInputDialogTitle);

        if (sceneChanged)
        {
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        }

        if (!sceneWasOpen && activeScenePath != string.Empty)
        {
            EditorSceneManager.OpenScene(activeScenePath, OpenSceneMode.Single);
        }
    }

    /// <summary> 指定名の入力フィールドを設定する </summary>
    static bool ConfigureInputField(string objectName, string dialogTitle)
    {
        var inputObjectList = Object.FindObjectsOfType<TMP_InputField>(true);
        foreach (var inputField in inputObjectList)
        {
            if (inputField.gameObject.name != objectName)
            {
                continue;
            }

            var bridge = inputField.GetComponent<WebGLNativeTmpInputField>();
            if (bridge == null)
            {
                bridge = inputField.gameObject.AddComponent<WebGLNativeTmpInputField>();
            }

            ApplyBridgeSettings(bridge, inputField, dialogTitle);
            EditorUtility.SetDirty(inputField.gameObject);
            return true;
        }

        return false;
    }

    /// <summary> ブリッジのシリアライズ設定を反映する </summary>
    static void ApplyBridgeSettings(WebGLNativeTmpInputField bridge, TMP_InputField inputField, string dialogTitle)
    {
        var serializedBridge = new SerializedObject(bridge);
        serializedBridge.FindProperty("_inputField").objectReferenceValue = inputField;
        serializedBridge.FindProperty("_dialogTitle").stringValue = dialogTitle;
        serializedBridge.ApplyModifiedPropertiesWithoutUndo();
    }
}
