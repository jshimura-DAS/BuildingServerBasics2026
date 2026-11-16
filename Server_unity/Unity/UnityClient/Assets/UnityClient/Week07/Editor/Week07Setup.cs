// Week07Setup.cs
// Week07用の環境設定（HTTP許可・日本語フォント）をメニューから実行する
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

public static class Week07Setup
{
    private const string FontAssetPath = "Assets/UnityClient/Week07/JapaneseFont.asset";

    [MenuItem("Tools/Week07/Allow HTTP (Insecure)")]
    public static void AllowHttp()
    {
        PlayerSettings.insecureHttpOption = InsecureHttpOption.AlwaysAllowed;
        AssetDatabase.SaveAssets();
        Debug.Log("Allow downloads over HTTP を Always allowed に設定しました。反映されない場合はエディタを再起動してください。");
    }

    [MenuItem("Tools/Week07/Setup Japanese Font")]
    public static void SetupJapaneseFont()
    {
        TMP_FontAsset fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAssetPath);
        if (fontAsset == null)
        {
            fontAsset = CreateFontAsset();
            if (fontAsset == null)
            {
                Debug.LogError("日本語フォントを作成できませんでした。OSに日本語フォント（Yu Gothic / Meiryoなど）があるか確認してください。");
                return;
            }
        }

        // 既定フォントのフォールバックに追加（今後作るTMPでも日本語が出る）
        TMP_Settings settings = TMP_Settings.instance;
        if (settings != null && TMP_Settings.defaultFontAsset != null)
        {
            var fallbacks = TMP_Settings.defaultFontAsset.fallbackFontAssetTable;
            if (!fallbacks.Contains(fontAsset))
            {
                fallbacks.Add(fontAsset);
                EditorUtility.SetDirty(TMP_Settings.defaultFontAsset);
            }
        }

        // 開いているシーンのTMPにも設定する
        int count = 0;
        foreach (TMP_Text text in Object.FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            Undo.RecordObject(text, "Set Japanese Font");
            text.font = fontAsset;
            EditorUtility.SetDirty(text);
            count++;
        }

        if (count > 0)
        {
            EditorSceneManager.MarkAllScenesDirty();
        }

        AssetDatabase.SaveAssets();
        Debug.Log("日本語フォントを設定しました（" + count + "個のTMP_Text）。シーンを保存してください。");
    }

    private static TMP_FontAsset CreateFontAsset()
    {
        string[] families = { "Yu Gothic", "Meiryo", "MS Gothic" };
        TMP_FontAsset fontAsset = null;

        foreach (string family in families)
        {
            fontAsset = TMP_FontAsset.CreateFontAsset(family, "Regular", 90);
            if (fontAsset != null)
            {
                break;
            }
        }

        if (fontAsset == null)
        {
            return null;
        }

        fontAsset.name = "JapaneseFont";
        fontAsset.atlasPopulationMode = AtlasPopulationMode.Dynamic;
        fontAsset.isMultiAtlasTexturesEnabled = true;

        AssetDatabase.CreateAsset(fontAsset, FontAssetPath);
        if (fontAsset.material != null)
        {
            fontAsset.material.name = "JapaneseFont Material";
            AssetDatabase.AddObjectToAsset(fontAsset.material, fontAsset);
        }
        foreach (Texture2D atlas in fontAsset.atlasTextures)
        {
            atlas.name = "JapaneseFont Atlas";
            AssetDatabase.AddObjectToAsset(atlas, fontAsset);
        }

        EditorUtility.SetDirty(fontAsset);
        AssetDatabase.SaveAssets();
        return fontAsset;
    }
}
