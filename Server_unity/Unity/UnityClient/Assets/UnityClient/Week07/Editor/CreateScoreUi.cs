// CreateScoreUi.cs
// メニュー Tools/Week07/Create Score UI で、開いているシーンにスコア登録UIを生成する
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public static class CreateScoreUi
{
    private const string RootName = "ScoreUiCanvas";

    [MenuItem("Tools/Week07/Create Score UI")]
    public static void Create()
    {
        if (GameObject.Find(RootName) != null)
        {
            EditorUtility.DisplayDialog("Create Score UI", "すでに" + RootName + "が存在します。削除してから実行してください。", "OK");
            return;
        }

        // Canvas
        GameObject canvasObj = new GameObject(RootName, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Undo.RegisterCreatedObjectUndo(canvasObj, "Create Score UI");
        Canvas canvas = canvasObj.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObj.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720);

        EnsureEventSystem();

        TMP_DefaultControls.Resources res = new TMP_DefaultControls.Resources
        {
            standard = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd"),
            inputField = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/InputFieldBackground.psd"),
            background = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd"),
        };

        // name入力欄
        TMP_InputField nameField = CreateInputField(canvasObj.transform, res, "NameInput", "name", new Vector2(0, 200));
        // score入力欄
        TMP_InputField scoreField = CreateInputField(canvasObj.transform, res, "ScoreInput", "score", new Vector2(0, 120));
        scoreField.contentType = TMP_InputField.ContentType.IntegerNumber;

        // 送信ボタン
        GameObject buttonObj = TMP_DefaultControls.CreateButton(res);
        buttonObj.name = "SubmitButton";
        buttonObj.transform.SetParent(canvasObj.transform, false);
        SetRect(buttonObj.GetComponent<RectTransform>(), new Vector2(0, 40), new Vector2(200, 50));
        TMP_Text buttonLabel = buttonObj.GetComponentInChildren<TMP_Text>();
        buttonLabel.text = "Send";

        // 結果表示
        GameObject resultObj = TMP_DefaultControls.CreateText(res);
        resultObj.name = "ResultText";
        resultObj.transform.SetParent(canvasObj.transform, false);
        SetRect(resultObj.GetComponent<RectTransform>(), new Vector2(0, -100), new Vector2(900, 150));
        TMP_Text resultText = resultObj.GetComponent<TMP_Text>();
        resultText.text = "result";
        resultText.fontSize = 28;
        resultText.alignment = TextAlignmentOptions.Center;

        // 通信担当オブジェクト
        GameObject clientObj = new GameObject("ScoreClient");
        Undo.RegisterCreatedObjectUndo(clientObj, "Create Score UI");
        ScoreClient client = clientObj.AddComponent<ScoreClient>();

        SerializedObject so = new SerializedObject(client);
        so.FindProperty("nameField").objectReferenceValue = nameField;
        so.FindProperty("scoreField").objectReferenceValue = scoreField;
        so.FindProperty("resultText").objectReferenceValue = resultText;
        so.ApplyModifiedProperties();

        // ボタンのOnClickに登録
        Button button = buttonObj.GetComponent<Button>();
        UnityEventTools.AddPersistentListener(button.onClick, client.OnSubmitClicked);

        EditorSceneManager.MarkSceneDirty(canvasObj.scene);
        Selection.activeGameObject = clientObj;
        Debug.Log("Score UIを作成しました。シーンを保存してください。");
    }

    private static TMP_InputField CreateInputField(Transform parent, TMP_DefaultControls.Resources res, string objName, string placeholder, Vector2 pos)
    {
        GameObject obj = TMP_DefaultControls.CreateInputField(res);
        obj.name = objName;
        obj.transform.SetParent(parent, false);
        SetRect(obj.GetComponent<RectTransform>(), pos, new Vector2(400, 50));
        TMP_InputField field = obj.GetComponent<TMP_InputField>();
        if (field.placeholder is TMP_Text placeholderText)
        {
            placeholderText.text = placeholder;
        }
        return field;
    }

    private static void SetRect(RectTransform rect, Vector2 anchoredPosition, Vector2 size)
    {
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;
    }

    private static void EnsureEventSystem()
    {
        if (Object.FindFirstObjectByType<EventSystem>() != null)
        {
            return;
        }

        GameObject es = new GameObject("EventSystem", typeof(EventSystem));
        Undo.RegisterCreatedObjectUndo(es, "Create Score UI");
#if ENABLE_INPUT_SYSTEM
        es.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
#else
        es.AddComponent<StandaloneInputModule>();
#endif
    }
}
