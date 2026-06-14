using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

/// <summary> 日本語TMPフォントアセットを生成するEditorユーティリティ </summary>
public static class JapaneseFontAssetGenerator
{
    const string FontPath = "Assets/TextMesh Pro/Fonts/NotoSansJP-Regular.otf";
    const string FontAssetPath = "Assets/TextMesh Pro/Resources/Fonts & Materials/NotoSansJP SDF.asset";
    const string TmpSettingsPath = "Assets/TextMesh Pro/Resources/TMP Settings.asset";
    const int AtlasSize = 1024;

    /// <summary> 日本語フォントアセットを生成してプロジェクト設定を更新する </summary>
    public static void GenerateFromCommandLine()
    {
        Generate();
        EditorApplication.Exit(0);
    }

    /// <summary> 日本語フォントアセットを生成してプロジェクト設定を更新する </summary>
    [MenuItem("Tools/Generate Japanese Font Asset")]
    public static void Generate()
    {
        var sourceFont = AssetDatabase.LoadAssetAtPath<Font>(FontPath);
        if (sourceFont == null)
        {
            Debug.LogError($"フォントが見つかりません: {FontPath}");
            return;
        }

        var fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAssetPath);
        if (fontAsset != null)
        {
            AssetDatabase.DeleteAsset(FontAssetPath);
        }

        fontAsset = CreateJapaneseFontAsset(sourceFont);

        EditorUtility.SetDirty(fontAsset);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        ApplyDefaultFont(fontAsset);
        ApplyFontToSceneTexts(fontAsset);

        Debug.Log($"日本語フォントアセットを生成しました: {FontAssetPath}");
    }

    /// <summary> 日本語用TMPフォントアセットを新規作成する </summary>
    static TMP_FontAsset CreateJapaneseFontAsset(Font sourceFont)
    {
        var fontAsset = TMP_FontAsset.CreateFontAsset(
            sourceFont,
            48,
            5,
            GlyphRenderMode.SDFAA,
            AtlasSize,
            AtlasSize,
            AtlasPopulationMode.Dynamic,
            true);

        var atlasTexture = fontAsset.atlasTextures[0];
        atlasTexture.name = "NotoSansJP SDF Atlas";

        var material = new Material(Shader.Find("TextMeshPro/Distance Field"));
        material.name = "NotoSansJP SDF Material";
        material.SetTexture(ShaderUtilities.ID_MainTex, atlasTexture);
        material.SetFloat(ShaderUtilities.ID_TextureWidth, AtlasSize);
        material.SetFloat(ShaderUtilities.ID_TextureHeight, AtlasSize);
        material.SetFloat(ShaderUtilities.ID_GradientScale, fontAsset.atlasPadding + 1f);
        material.SetFloat(ShaderUtilities.ID_WeightNormal, fontAsset.normalStyle);
        material.SetFloat(ShaderUtilities.ID_WeightBold, fontAsset.boldStyle);
        fontAsset.material = material;

        AssetDatabase.CreateAsset(fontAsset, FontAssetPath);
        AssetDatabase.AddObjectToAsset(atlasTexture, fontAsset);
        AssetDatabase.AddObjectToAsset(material, fontAsset);
        return fontAsset;
    }

    /// <summary> TMP Settingsのデフォルトフォントを更新する </summary>
    static void ApplyDefaultFont(TMP_FontAsset fontAsset)
    {
        var tmpSettings = AssetDatabase.LoadAssetAtPath<TMP_Settings>(TmpSettingsPath);
        if (tmpSettings == null)
        {
            return;
        }

        var serializedSettings = new SerializedObject(tmpSettings);
        serializedSettings.FindProperty("m_defaultFontAsset").objectReferenceValue = fontAsset;
        serializedSettings.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(tmpSettings);
        AssetDatabase.SaveAssets();
    }

    /// <summary> Mainシーン内のTMPテキストに日本語フォントを適用する </summary>
    static void ApplyFontToSceneTexts(TMP_FontAsset fontAsset)
    {
        const string scenePath = "Assets/Scenes/Main.unity";
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        var textComponents = Object.FindObjectsOfType<TextMeshProUGUI>(true);
        foreach (var text in textComponents)
        {
            text.font = fontAsset;
            EditorUtility.SetDirty(text);
        }

        EditorSceneManager.SaveScene(scene);
    }
}
