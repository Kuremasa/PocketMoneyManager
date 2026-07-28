using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary> MainシーンのUIを構築するEditorユーティリティ </summary>
[InitializeOnLoad]
public static class HomeSceneSetup
{
    const string ScenePath = "Assets/Scenes/Main.unity";
    const string FontAssetPath = "Assets/TextMesh Pro/Resources/Fonts & Materials/NotoSansJP SDF.asset";
    const string SelectedTabSpritePath = "Assets/Images/Sprites/btn_square_main.png";

    /// <summary> コンパイル後に未セットアップなら自動構築する </summary>
    static HomeSceneSetup() => EditorApplication.delayCall += TryAutoSetup;

    /// <summary> 未セットアップ時のみ自動構築する </summary>
    static void TryAutoSetup()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling)
        {
            return;
        }

        var uiManagerList = Object.FindObjectsOfType<UIManager>(true);
        if (uiManagerList.Length == 0)
        {
            if (EditorSceneManager.GetActiveScene().path != ScenePath)
            {
                return;
            }

            Setup();
            return;
        }

        if (Object.FindObjectsOfType<SimplePopupView>(true).Length == 0)
        {
            TryAddSimplePopup();
        }

        TryAddVersionText();
        TryAddHistoryItemButton();
        TryAddExpenseDeleteButton();
    }

    /// <summary> SimplePopupをMainシーンに追加する </summary>
    [MenuItem("Tools/Add Simple Popup")]
    public static void TryAddSimplePopup()
    {
        var scene = EditorSceneManager.GetActiveScene().path == ScenePath
            ? EditorSceneManager.GetActiveScene()
            : EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        var uiManager = Object.FindObjectOfType<UIManager>(true);
        if (uiManager == null)
        {
            return;
        }

        if (Object.FindObjectsOfType<SimplePopupView>(true).Length > 0)
        {
            return;
        }

        var fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAssetPath);
        var canvas = Object.FindObjectOfType<Canvas>();
        var simplePopup = CreateSimplePopup(canvas.transform, fontAsset);

        var serializedObject = new SerializedObject(uiManager);
        serializedObject.FindProperty("_simplePopup").objectReferenceValue = simplePopup;
        serializedObject.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("SimplePopup added to Main scene.");
    }

    /// <summary> バージョン表示をMainシーンに追加する </summary>
    [MenuItem("Tools/Add Version Text")]
    public static void TryAddVersionText()
    {
        var scene = EditorSceneManager.GetActiveScene().path == ScenePath
            ? EditorSceneManager.GetActiveScene()
            : EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        var homeView = Object.FindObjectOfType<HomeView>(true);
        if (homeView == null)
        {
            return;
        }

        var serializedObject = new SerializedObject(homeView);
        if (serializedObject.FindProperty("_versionText").objectReferenceValue != null)
        {
            return;
        }

        var fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAssetPath);
        var versionText = CreateVersionText(homeView.transform, fontAsset);
        SetHomeViewVersionReference(homeView, versionText);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("VersionText added to Main scene.");
    }

    /// <summary> HistoryItemPrefabをボタン化する </summary>
    [MenuItem("Tools/Add History Item Button")]
    public static void TryAddHistoryItemButton()
    {
        var scene = EditorSceneManager.GetActiveScene().path == ScenePath
            ? EditorSceneManager.GetActiveScene()
            : EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        var itemViewList = Object.FindObjectsOfType<HistoryItemView>(true);
        if (itemViewList.Length == 0)
        {
            return;
        }

        var sceneChanged = false;
        foreach (var itemView in itemViewList)
        {
            var serializedObject = new SerializedObject(itemView);
            if (serializedObject.FindProperty("_itemButton").objectReferenceValue != null)
            {
                continue;
            }

            var itemButton = GetOrAddComponent<Button>(itemView.gameObject);
            var itemImage = GetOrAddComponent<Image>(itemView.gameObject);
            itemButton.targetGraphic = itemImage;
            DisableRaycastOnChildTexts(itemView.transform);

            var dateText = itemView.transform.Find("DateText")?.GetComponent<TextMeshProUGUI>();
            var amountText = itemView.transform.Find("AmountText")?.GetComponent<TextMeshProUGUI>();
            var noteText = itemView.transform.Find("NoteText")?.GetComponent<TextMeshProUGUI>();
            SetHistoryItemReferences(itemView, itemButton, dateText, amountText, noteText);
            sceneChanged = true;
        }

        if (!sceneChanged)
        {
            return;
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("HistoryItemButton added to Main scene.");
    }

    /// <summary> ExpenseInputPopupに履歴削除ボタンを追加する </summary>
    [MenuItem("Tools/Add Expense Delete Button")]
    public static void TryAddExpenseDeleteButton()
    {
        var scene = EditorSceneManager.GetActiveScene().path == ScenePath
            ? EditorSceneManager.GetActiveScene()
            : EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        var expensePopup = Object.FindObjectOfType<ExpenseInputPopupView>(true);
        if (expensePopup == null)
        {
            return;
        }

        var serializedObject = new SerializedObject(expensePopup);
        var deleteButtonProperty = serializedObject.FindProperty("_deleteButton");
        var registerButtonTextProperty = serializedObject.FindProperty("_registerButtonText");
        if (deleteButtonProperty.objectReferenceValue != null && registerButtonTextProperty.objectReferenceValue != null)
        {
            return;
        }

        var fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAssetPath);
        var panel = expensePopup.transform.Find("Panel");
        if (panel == null)
        {
            return;
        }

        var registerButton = serializedObject.FindProperty("_registerButton").objectReferenceValue as Button;
        var registerButtonText = registerButton != null
            ? registerButton.transform.Find("Text")?.GetComponent<TextMeshProUGUI>()
            : null;
        var deleteButton = CreateButton(panel, "DeleteButton", "履歴削除", new Vector2(0f, -420f), fontAsset);
        ConfigureExpenseDeleteButton(deleteButton);
        deleteButton.gameObject.SetActive(false);

        if (registerButtonTextProperty != null)
        {
            registerButtonTextProperty.objectReferenceValue = registerButtonText;
        }

        deleteButtonProperty.objectReferenceValue = deleteButton;
        serializedObject.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("ExpenseDeleteButton added to Main scene.");
    }

    /// <summary> MainシーンのUIを構築する </summary>
    [MenuItem("Tools/Setup Home Scene")]
    public static void Setup()
    {
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        var fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAssetPath);

        var canvas = FindOrCreateCanvas();
        var homeViewGo = FindOrCreateHomeView(canvas.transform);
        var homeView = homeViewGo.GetComponent<HomeView>();

        RemoveLegacySampleButton(homeViewGo.transform);

        var balanceText = CreateBalanceText(homeViewGo.transform, fontAsset);
        var expenseButton = CreateButton(homeViewGo.transform, "ExpenseButton", "消費", new Vector2(0f, -200f), fontAsset);
        var summaryButton = CreateButton(homeViewGo.transform, "SummaryButton", "消費累計", new Vector2(0f, -320f), fontAsset);
        var historyButton = CreateButton(homeViewGo.transform, "HistoryButton", "履歴", new Vector2(0f, -440f), fontAsset);
        var configButton = CreateButton(homeViewGo.transform, "ConfigButton", "コンフィグ", new Vector2(0f, -560f), fontAsset);
        var versionText = CreateVersionText(homeViewGo.transform, fontAsset);

        SetHomeViewReferences(homeView, balanceText, expenseButton, summaryButton, historyButton, configButton, versionText);

        var expensePopup = CreateExpenseInputPopup(canvas.transform, fontAsset);
        var summaryPopup = CreateExpenseSummaryPopup(canvas.transform, fontAsset);
        var historyPopup = CreateHistoryPopup(canvas.transform, fontAsset);
        var simplePopup = CreateSimplePopup(canvas.transform, fontAsset);

        var uiManagerGo = FindOrCreateGameObject("UIManager", canvas.transform);
        var uiManager = GetOrAddComponent<UIManager>(uiManagerGo);
        SetUIManagerReferences(uiManager, homeView, expensePopup, summaryPopup, historyPopup, simplePopup);

        var homePresenterGo = FindOrCreateRootGameObject("HomePresenter");
        var homePresenter = GetOrAddComponent<HomePresenter>(homePresenterGo);
        SetHomePresenterReference(homePresenter, uiManager);

        RenameLegacyObjects(homeViewGo, homePresenterGo);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("Home scene setup completed.");
    }

    /// <summary> Canvasを取得または作成する </summary>
    static GameObject FindOrCreateCanvas()
    {
        var canvas = Object.FindObjectOfType<Canvas>();
        if (canvas != null)
        {
            EnsureEventSystem();
            return canvas.gameObject;
        }

        var canvasGo = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvasComponent = canvasGo.GetComponent<Canvas>();
        canvasComponent.renderMode = RenderMode.ScreenSpaceOverlay;

        var scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);
        scaler.matchWidthOrHeight = 0.5f;

        EnsureEventSystem();
        return canvasGo;
    }

    /// <summary> EventSystemを確保する </summary>
    static void EnsureEventSystem()
    {
        if (Object.FindObjectOfType<EventSystem>() != null)
        {
            return;
        }

        new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
    }

    /// <summary> HomeViewを取得または作成する </summary>
    static GameObject FindOrCreateHomeView(Transform canvasTransform)
    {
        var homeView = Object.FindObjectOfType<HomeView>();
        if (homeView != null)
        {
            return homeView.gameObject;
        }

        var homeViewGo = new GameObject("HomeView", typeof(RectTransform), typeof(HomeView));
        var rectTransform = homeViewGo.GetComponent<RectTransform>();
        rectTransform.SetParent(canvasTransform, false);
        StretchFull(rectTransform);
        return homeViewGo;
    }

    /// <summary> 旧サンプルボタンを削除する </summary>
    static void RemoveLegacySampleButton(Transform homeViewTransform)
    {
        var sampleButton = homeViewTransform.Find("SampleButton");
        if (sampleButton != null)
        {
            Object.DestroyImmediate(sampleButton.gameObject);
        }
    }

    /// <summary> 残高テキストを作成する </summary>
    static TextMeshProUGUI CreateBalanceText(Transform parent, TMP_FontAsset fontAsset)
    {
        var existing = parent.Find("BalanceText");
        if (existing != null)
        {
            return existing.GetComponent<TextMeshProUGUI>();
        }

        var textGo = new GameObject("BalanceText", typeof(RectTransform), typeof(TextMeshProUGUI));
        var rectTransform = textGo.GetComponent<RectTransform>();
        rectTransform.SetParent(parent, false);
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = new Vector2(0f, 400f);
        rectTransform.sizeDelta = new Vector2(900f, 120f);

        var text = textGo.GetComponent<TextMeshProUGUI>();
        text.font = fontAsset;
        text.fontSize = 36f;
        text.alignment = TextAlignmentOptions.Center;
        text.text = "今月のおこづかいは残り 0円 です";
        return text;
    }

    /// <summary> バージョンテキストを作成する </summary>
    static TextMeshProUGUI CreateVersionText(Transform parent, TMP_FontAsset fontAsset)
    {
        var existing = parent.Find("VersionText");
        if (existing != null)
        {
            return existing.GetComponent<TextMeshProUGUI>();
        }

        var textGo = new GameObject("VersionText", typeof(RectTransform), typeof(TextMeshProUGUI));
        var rectTransform = textGo.GetComponent<RectTransform>();
        rectTransform.SetParent(parent, false);
        rectTransform.anchorMin = new Vector2(0.5f, 0f);
        rectTransform.anchorMax = new Vector2(0.5f, 0f);
        rectTransform.anchoredPosition = new Vector2(0f, 40f);
        rectTransform.sizeDelta = new Vector2(400f, 40f);

        var text = textGo.GetComponent<TextMeshProUGUI>();
        text.font = fontAsset;
        text.fontSize = 24f;
        text.alignment = TextAlignmentOptions.Center;
        text.color = new Color(0.5f, 0.5f, 0.5f, 1f);
        text.text = Application.version;
        return text;
    }

    /// <summary> ボタンを作成する </summary>
    static Button CreateButton(Transform parent, string objectName, string label, Vector2 anchoredPosition, TMP_FontAsset fontAsset)
    {
        var existing = parent.Find(objectName);
        if (existing != null)
        {
            return existing.GetComponent<Button>();
        }

        var buttonGo = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        var rectTransform = buttonGo.GetComponent<RectTransform>();
        rectTransform.SetParent(parent, false);
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = new Vector2(400f, 80f);

        var image = buttonGo.GetComponent<Image>();
        image.color = Color.white;

        var labelGo = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        var labelRect = labelGo.GetComponent<RectTransform>();
        labelRect.SetParent(buttonGo.transform, false);
        StretchFull(labelRect);

        var labelText = labelGo.GetComponent<TextMeshProUGUI>();
        labelText.font = fontAsset;
        labelText.fontSize = 32f;
        labelText.alignment = TextAlignmentOptions.Center;
        labelText.text = label;

        return buttonGo.GetComponent<Button>();
    }

    /// <summary> 消費入力ポップアップを作成する </summary>
    static ExpenseInputPopupView CreateExpenseInputPopup(Transform canvasTransform, TMP_FontAsset fontAsset)
    {
        var popupRoot = FindOrCreatePopupRoot(canvasTransform, "ExpenseInputPopup");
        var popupView = GetOrAddComponent<ExpenseInputPopupView>(popupRoot);

        var panel = FindOrCreateChild(popupRoot.transform, "Panel");
        SetupPopupPanel(panel);

        var blackCover = FindOrCreateChild(popupRoot.transform, "BlackCover");
        SetupBlackCover(blackCover);

        var dateInput = CreateInputField(panel.transform, "DateInput", "日付 (yyyy-MM-dd)", new Vector2(0f, 120f), fontAsset);
        var amountInput = CreateInputField(panel.transform, "AmountInput", "金額", new Vector2(0f, 0f), fontAsset);
        var noteInput = CreateInputField(panel.transform, "NoteInput", "用途", new Vector2(0f, -120f), fontAsset);
        var errorText = CreatePopupLabel(panel.transform, "ErrorText", string.Empty, new Vector2(0f, -220f), 24f, fontAsset, Color.red);
        var registerButton = CreateButton(panel.transform, "RegisterButton", "登録する", new Vector2(-120f, -320f), fontAsset);
        var cancelButton = CreateButton(panel.transform, "CancelButton", "キャンセル", new Vector2(120f, -320f), fontAsset);
        var deleteButton = CreateButton(panel.transform, "DeleteButton", "履歴削除", new Vector2(0f, -420f), fontAsset);
        deleteButton.gameObject.SetActive(false);
        var registerButtonText = registerButton.transform.Find("Text").GetComponent<TextMeshProUGUI>();

        SetExpenseInputPopupReferences(
            popupView,
            panel,
            blackCover,
            dateInput,
            amountInput,
            noteInput,
            errorText,
            registerButton,
            registerButtonText,
            cancelButton,
            deleteButton);
        blackCover.SetActive(false);
        panel.SetActive(false);
        return popupView;
    }

    /// <summary> 消費累計ポップアップを作成する </summary>
    static ExpenseSummaryPopupView CreateExpenseSummaryPopup(Transform canvasTransform, TMP_FontAsset fontAsset)
    {
        var popupRoot = FindOrCreatePopupRoot(canvasTransform, "ExpenseSummaryPopup");
        var popupView = GetOrAddComponent<ExpenseSummaryPopupView>(popupRoot);

        var panel = FindOrCreateChild(popupRoot.transform, "Panel");
        SetupPopupPanel(panel);

        var blackCover = FindOrCreateChild(popupRoot.transform, "BlackCover");
        SetupBlackCover(blackCover);

        var monthlyTabButton = CreateButton(panel.transform, "MonthlyTabButton", "月間", new Vector2(-120f, 120f), fontAsset);
        var weeklyTabButton = CreateButton(panel.transform, "WeeklyTabButton", "週間", new Vector2(120f, 120f), fontAsset);
        var monthlyTabImage = monthlyTabButton.GetComponent<Image>();
        var weeklyTabImage = weeklyTabButton.GetComponent<Image>();
        var monthlyTabText = monthlyTabButton.transform.Find("Text").GetComponent<TextMeshProUGUI>();
        var weeklyTabText = weeklyTabButton.transform.Find("Text").GetComponent<TextMeshProUGUI>();
        var selectedTabSprite = AssetDatabase.LoadAssetAtPath<Sprite>(SelectedTabSpritePath);
        var periodText = CreatePopupLabel(panel.transform, "PeriodText", string.Empty, new Vector2(0f, 60f), 28f, fontAsset, Color.black);
        var totalText = CreatePopupLabel(panel.transform, "TotalText", "0円", new Vector2(0f, 0f), 48f, fontAsset, Color.black);
        var closeButton = CreateButton(panel.transform, "CloseButton", "閉じる", new Vector2(0f, -200f), fontAsset);

        SetExpenseSummaryPopupReferences(
            popupView,
            panel,
            blackCover,
            monthlyTabButton,
            weeklyTabButton,
            monthlyTabImage,
            weeklyTabImage,
            monthlyTabText,
            weeklyTabText,
            selectedTabSprite,
            periodText,
            totalText,
            closeButton);
        blackCover.SetActive(false);
        panel.SetActive(false);
        return popupView;
    }

    /// <summary> 履歴ポップアップを作成する </summary>
    static HistoryPopupView CreateHistoryPopup(Transform canvasTransform, TMP_FontAsset fontAsset)
    {
        var popupRoot = FindOrCreatePopupRoot(canvasTransform, "HistoryPopup");
        var popupView = GetOrAddComponent<HistoryPopupView>(popupRoot);

        var panel = FindOrCreateChild(popupRoot.transform, "Panel");
        SetupPopupPanel(panel);

        var blackCover = FindOrCreateChild(popupRoot.transform, "BlackCover");
        SetupBlackCover(blackCover);

        var scrollGo = FindOrCreateChild(panel.transform, "ScrollView");
        SetupScrollView(scrollGo);

        var contentRoot = scrollGo.transform.Find("Viewport/Content");
        var itemPrefabGo = FindOrCreateChild(contentRoot, "HistoryItemPrefab");
        SetupHistoryItemPrefab(itemPrefabGo, fontAsset);

        var closeButton = CreateButton(panel.transform, "CloseButton", "閉じる", new Vector2(0f, -320f), fontAsset);
        var itemPrefab = itemPrefabGo.GetComponent<HistoryItemView>();

        SetHistoryPopupReferences(popupView, panel, blackCover, contentRoot, itemPrefab, closeButton);
        itemPrefabGo.SetActive(false);
        blackCover.SetActive(false);
        panel.SetActive(false);
        return popupView;
    }

    /// <summary> 簡易ポップアップを作成する </summary>
    static SimplePopupView CreateSimplePopup(Transform canvasTransform, TMP_FontAsset fontAsset)
    {
        var popupRoot = FindOrCreatePopupRoot(canvasTransform, "SimplePopup");
        popupRoot.transform.SetAsLastSibling();
        var popupView = GetOrAddComponent<SimplePopupView>(popupRoot);

        var panel = FindOrCreateChild(popupRoot.transform, "Panel");
        SetupPopupPanel(panel);

        var blackCover = FindOrCreateChild(popupRoot.transform, "BlackCover");
        SetupBlackCover(blackCover);

        var messageText = CreatePopupLabel(panel.transform, "MessageText", string.Empty, new Vector2(0f, 80f), 32f, fontAsset, Color.black);
        var okButton = CreateButton(panel.transform, "OkButton", "OK", new Vector2(-120f, -200f), fontAsset);
        var cancelButton = CreateButton(panel.transform, "CancelButton", "キャンセル", new Vector2(120f, -200f), fontAsset);
        var okButtonText = okButton.transform.Find("Text").GetComponent<TextMeshProUGUI>();
        var cancelButtonText = cancelButton.transform.Find("Text").GetComponent<TextMeshProUGUI>();

        SetSimplePopupReferences(popupView, panel, blackCover, messageText, okButton, okButtonText, cancelButton, cancelButtonText);
        blackCover.SetActive(false);
        panel.SetActive(false);
        return popupView;
    }

    /// <summary> ScrollViewをセットアップする </summary>
    static void SetupScrollView(GameObject scrollGo)
    {
        var scrollRect = GetOrAddComponent<ScrollRect>(scrollGo);
        var scrollTransform = scrollGo.GetComponent<RectTransform>();
        scrollTransform.sizeDelta = new Vector2(800f, 500f);
        scrollTransform.anchoredPosition = new Vector2(0f, 40f);

        var viewport = FindOrCreateChild(scrollGo.transform, "Viewport");
        var viewportRect = viewport.GetComponent<RectTransform>();
        StretchFull(viewportRect);
        var viewportImage = GetOrAddComponent<Image>(viewport);
        viewportImage.color = new Color(1f, 1f, 1f, 0.1f);
        GetOrAddComponent<Mask>(viewport).showMaskGraphic = false;

        var content = FindOrCreateChild(viewport.transform, "Content");
        var contentRect = content.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0f, 1f);
        contentRect.anchorMax = new Vector2(1f, 1f);
        contentRect.pivot = new Vector2(0.5f, 1f);
        contentRect.anchoredPosition = Vector2.zero;
        contentRect.sizeDelta = new Vector2(0f, 0f);

        var layout = GetOrAddComponent<VerticalLayoutGroup>(content);
        layout.childControlHeight = true;
        layout.childControlWidth = true;
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = true;
        layout.spacing = 8f;
        GetOrAddComponent<ContentSizeFitter>(content).verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scrollRect.viewport = viewportRect;
        scrollRect.content = contentRect;
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
    }

    /// <summary> 履歴アイテムPrefabをセットアップする </summary>
    static void SetupHistoryItemPrefab(GameObject itemGo, TMP_FontAsset fontAsset)
    {
        var rectTransform = itemGo.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(0f, 80f);
        GetOrAddComponent<LayoutElement>(itemGo).preferredHeight = 80f;
        GetOrAddComponent<Image>(itemGo).color = new Color(1f, 1f, 1f, 0.5f);

        var itemView = GetOrAddComponent<HistoryItemView>(itemGo);
        var itemButton = GetOrAddComponent<Button>(itemGo);
        var itemImage = GetOrAddComponent<Image>(itemGo);
        itemButton.targetGraphic = itemImage;
        var dateText = CreatePopupLabel(itemGo.transform, "DateText", "2026-01-01", new Vector2(-250f, 0f), 24f, fontAsset, Color.black);
        var amountText = CreatePopupLabel(itemGo.transform, "AmountText", "0円", new Vector2(0f, 0f), 24f, fontAsset, Color.black);
        var noteText = CreatePopupLabel(itemGo.transform, "NoteText", "用途", new Vector2(250f, 0f), 24f, fontAsset, Color.black);
        DisableRaycastOnChildTexts(itemGo.transform);

        SetHistoryItemReferences(itemView, itemButton, dateText, amountText, noteText);
    }

    /// <summary> 子テキストのRaycastTargetを無効化する </summary>
    static void DisableRaycastOnChildTexts(Transform parent)
    {
        foreach (var text in parent.GetComponentsInChildren<TextMeshProUGUI>(true))
        {
            text.raycastTarget = false;
        }
    }

    /// <summary> InputFieldを作成する </summary>
    static TMP_InputField CreateInputField(Transform parent, string objectName, string placeholder, Vector2 anchoredPosition, TMP_FontAsset fontAsset)
    {
        var existing = parent.Find(objectName);
        if (existing != null)
        {
            return existing.GetComponent<TMP_InputField>();
        }

        var inputGo = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(TMP_InputField));
        var rectTransform = inputGo.GetComponent<RectTransform>();
        rectTransform.SetParent(parent, false);
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = new Vector2(600f, 60f);

        inputGo.GetComponent<Image>().color = Color.white;

        var textArea = new GameObject("Text Area", typeof(RectTransform));
        var textAreaRect = textArea.GetComponent<RectTransform>();
        textAreaRect.SetParent(inputGo.transform, false);
        StretchFull(textAreaRect);

        var placeholderGo = new GameObject("Placeholder", typeof(RectTransform), typeof(TextMeshProUGUI));
        var placeholderRect = placeholderGo.GetComponent<RectTransform>();
        placeholderRect.SetParent(textArea.transform, false);
        StretchFull(placeholderRect);
        var placeholderText = placeholderGo.GetComponent<TextMeshProUGUI>();
        placeholderText.font = fontAsset;
        placeholderText.fontSize = 24f;
        placeholderText.text = placeholder;
        placeholderText.color = new Color(0.5f, 0.5f, 0.5f, 1f);

        var textGo = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        var textRect = textGo.GetComponent<RectTransform>();
        textRect.SetParent(textArea.transform, false);
        StretchFull(textRect);
        var text = textGo.GetComponent<TextMeshProUGUI>();
        text.font = fontAsset;
        text.fontSize = 24f;
        text.color = Color.black;

        var inputField = inputGo.GetComponent<TMP_InputField>();
        inputField.textViewport = textAreaRect;
        inputField.textComponent = text;
        inputField.placeholder = placeholderText;

        var bridge = inputGo.AddComponent<WebGLNativeTmpInputField>();
        var serializedBridge = new SerializedObject(bridge);
        serializedBridge.FindProperty("_inputField").objectReferenceValue = inputField;
        serializedBridge.FindProperty("_dialogTitle").stringValue = placeholder;
        serializedBridge.ApplyModifiedPropertiesWithoutUndo();

        return inputField;
    }

    /// <summary> ポップアップ用ラベルを作成する </summary>
    static TextMeshProUGUI CreatePopupLabel(Transform parent, string objectName, string textValue, Vector2 anchoredPosition, float fontSize, TMP_FontAsset fontAsset, Color color)
    {
        var existing = parent.Find(objectName);
        if (existing != null)
        {
            return existing.GetComponent<TextMeshProUGUI>();
        }

        var labelGo = new GameObject(objectName, typeof(RectTransform), typeof(TextMeshProUGUI));
        var rectTransform = labelGo.GetComponent<RectTransform>();
        rectTransform.SetParent(parent, false);
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = new Vector2(700f, 60f);

        var text = labelGo.GetComponent<TextMeshProUGUI>();
        text.font = fontAsset;
        text.fontSize = fontSize;
        text.alignment = TextAlignmentOptions.Center;
        text.color = color;
        text.text = textValue;
        return text;
    }

    /// <summary> ポップアップPanelをセットアップする </summary>
    static void SetupPopupPanel(GameObject panel)
    {
        var panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = new Vector2(900f, 900f);
        GetOrAddComponent<Image>(panel).color = new Color(0.95f, 0.95f, 0.95f, 1f);
    }

    /// <summary> 黒背景カバーをセットアップする </summary>
    static void SetupBlackCover(GameObject blackCover)
    {
        blackCover.transform.SetAsFirstSibling();
        var blackCoverRect = blackCover.GetComponent<RectTransform>();
        blackCoverRect.anchorMin = new Vector2(0.5f, 0.5f);
        blackCoverRect.anchorMax = new Vector2(0.5f, 0.5f);
        blackCoverRect.anchoredPosition = Vector2.zero;
        blackCoverRect.sizeDelta = new Vector2(1500f, 2000f);
        GetOrAddComponent<Image>(blackCover).color = new Color(0f, 0f, 0f, 0.78431374f);
    }

    /// <summary> ポップアップRootを取得または作成する </summary>
    static GameObject FindOrCreatePopupRoot(Transform canvasTransform, string objectName)
    {
        var existing = canvasTransform.Find(objectName);
        if (existing != null)
        {
            return existing.gameObject;
        }

        var popupGo = new GameObject(objectName, typeof(RectTransform));
        var rectTransform = popupGo.GetComponent<RectTransform>();
        rectTransform.SetParent(canvasTransform, false);
        StretchFull(rectTransform);
        return popupGo;
    }

    /// <summary> 子GameObjectを取得または作成する </summary>
    static GameObject FindOrCreateChild(Transform parent, string objectName)
    {
        var existing = parent.Find(objectName);
        if (existing != null)
        {
            return existing.gameObject;
        }

        var childGo = new GameObject(objectName, typeof(RectTransform));
        childGo.GetComponent<RectTransform>().SetParent(parent, false);
        return childGo;
    }

    /// <summary> ルートGameObjectを取得または作成する </summary>
    static GameObject FindOrCreateRootGameObject(string objectName)
    {
        var existing = GameObject.Find(objectName);
        return existing != null ? existing : new GameObject(objectName, typeof(RectTransform));
    }

    /// <summary> 子階層のGameObjectを取得または作成する </summary>
    static GameObject FindOrCreateGameObject(string objectName, Transform parent)
    {
        var existing = parent.Find(objectName);
        if (existing != null)
        {
            return existing.gameObject;
        }

        var go = new GameObject(objectName, typeof(RectTransform));
        go.GetComponent<RectTransform>().SetParent(parent, false);
        StretchFull(go.GetComponent<RectTransform>());
        return go;
    }

    /// <summary> RectTransformを親いっぱいに広げる </summary>
    static void StretchFull(RectTransform rectTransform)
    {
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
    }

    /// <summary> コンポーネントを取得または追加する </summary>
    static T GetOrAddComponent<T>(GameObject target) where T : Component
    {
        var component = target.GetComponent<T>();
        return component != null ? component : target.AddComponent<T>();
    }

    /// <summary> 旧オブジェクト名をリネームする </summary>
    static void RenameLegacyObjects(GameObject homeViewGo, GameObject homePresenterGo)
    {
        if (homeViewGo.name == "MainView")
        {
            homeViewGo.name = "HomeView";
        }

        if (homePresenterGo.name == "MainPresenter")
        {
            homePresenterGo.name = "HomePresenter";
        }
    }

    /// <summary> HomeViewの参照を設定する </summary>
    static void SetHomeViewReferences(HomeView homeView, TextMeshProUGUI balanceText, Button expenseButton, Button summaryButton, Button historyButton, Button configButton, TextMeshProUGUI versionText)
    {
        var serializedObject = new SerializedObject(homeView);
        serializedObject.FindProperty("_balanceText").objectReferenceValue = balanceText;
        serializedObject.FindProperty("_expenseButton").objectReferenceValue = expenseButton;
        serializedObject.FindProperty("_summaryButton").objectReferenceValue = summaryButton;
        serializedObject.FindProperty("_historyButton").objectReferenceValue = historyButton;
        serializedObject.FindProperty("_configButton").objectReferenceValue = configButton;
        serializedObject.FindProperty("_versionText").objectReferenceValue = versionText;
        serializedObject.ApplyModifiedPropertiesWithoutUndo();
    }

    /// <summary> HomeViewのバージョン表示参照を設定する </summary>
    static void SetHomeViewVersionReference(HomeView homeView, TextMeshProUGUI versionText)
    {
        var serializedObject = new SerializedObject(homeView);
        serializedObject.FindProperty("_versionText").objectReferenceValue = versionText;
        serializedObject.ApplyModifiedPropertiesWithoutUndo();
    }

    /// <summary> UIManagerの参照を設定する </summary>
    static void SetUIManagerReferences(UIManager uiManager, HomeView homeView, ExpenseInputPopupView expensePopup, ExpenseSummaryPopupView summaryPopup, HistoryPopupView historyPopup, SimplePopupView simplePopup)
    {
        var serializedObject = new SerializedObject(uiManager);
        serializedObject.FindProperty("_homeView").objectReferenceValue = homeView;
        serializedObject.FindProperty("_expenseInputPopup").objectReferenceValue = expensePopup;
        serializedObject.FindProperty("_expenseSummaryPopup").objectReferenceValue = summaryPopup;
        serializedObject.FindProperty("_historyPopup").objectReferenceValue = historyPopup;
        serializedObject.FindProperty("_simplePopup").objectReferenceValue = simplePopup;
        serializedObject.ApplyModifiedPropertiesWithoutUndo();
    }

    /// <summary> HomePresenterの参照を設定する </summary>
    static void SetHomePresenterReference(HomePresenter homePresenter, UIManager uiManager)
    {
        var serializedObject = new SerializedObject(homePresenter);
        serializedObject.FindProperty("_uiManager").objectReferenceValue = uiManager;
        serializedObject.ApplyModifiedPropertiesWithoutUndo();
    }

    /// <summary> ExpenseInputPopupViewの参照を設定する </summary>
    static void SetExpenseInputPopupReferences(
        ExpenseInputPopupView popupView,
        GameObject root,
        GameObject blackCover,
        TMP_InputField dateInput,
        TMP_InputField amountInput,
        TMP_InputField noteInput,
        TextMeshProUGUI errorText,
        Button registerButton,
        TextMeshProUGUI registerButtonText,
        Button cancelButton,
        Button deleteButton)
    {
        var serializedObject = new SerializedObject(popupView);
        serializedObject.FindProperty("_root").objectReferenceValue = root;
        serializedObject.FindProperty("_blackCoverObject").objectReferenceValue = blackCover;
        serializedObject.FindProperty("_dateInput").objectReferenceValue = dateInput;
        serializedObject.FindProperty("_amountInput").objectReferenceValue = amountInput;
        serializedObject.FindProperty("_noteInput").objectReferenceValue = noteInput;
        serializedObject.FindProperty("_errorText").objectReferenceValue = errorText;
        serializedObject.FindProperty("_registerButton").objectReferenceValue = registerButton;
        serializedObject.FindProperty("_registerButtonText").objectReferenceValue = registerButtonText;
        serializedObject.FindProperty("_cancelButton").objectReferenceValue = cancelButton;
        serializedObject.FindProperty("_deleteButton").objectReferenceValue = deleteButton;
        serializedObject.ApplyModifiedPropertiesWithoutUndo();
    }

    /// <summary> 履歴削除ボタンの見た目を整える </summary>
    static void ConfigureExpenseDeleteButton(Button deleteButton)
    {
        var rectTransform = deleteButton.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0f, 1f);
        rectTransform.anchorMax = new Vector2(0f, 1f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = new Vector2(500f, -1240f);
        rectTransform.sizeDelta = new Vector2(400f, 160f);

        var panelRect = deleteButton.transform.parent as RectTransform;
        if (panelRect != null && panelRect.sizeDelta.y < 1400f)
        {
            panelRect.sizeDelta = new Vector2(panelRect.sizeDelta.x, 1400f);
        }
    }

    /// <summary> ExpenseSummaryPopupViewの参照を設定する </summary>
    static void SetExpenseSummaryPopupReferences(
        ExpenseSummaryPopupView popupView,
        GameObject root,
        GameObject blackCover,
        Button monthlyTabButton,
        Button weeklyTabButton,
        Image monthlyTabImage,
        Image weeklyTabImage,
        TextMeshProUGUI monthlyTabText,
        TextMeshProUGUI weeklyTabText,
        Sprite selectedTabSprite,
        TextMeshProUGUI periodText,
        TextMeshProUGUI totalText,
        Button closeButton)
    {
        var serializedObject = new SerializedObject(popupView);
        serializedObject.FindProperty("_root").objectReferenceValue = root;
        serializedObject.FindProperty("_blackCoverObject").objectReferenceValue = blackCover;
        serializedObject.FindProperty("_monthlyTabButton").objectReferenceValue = monthlyTabButton;
        serializedObject.FindProperty("_weeklyTabButton").objectReferenceValue = weeklyTabButton;
        serializedObject.FindProperty("_monthlyTabImage").objectReferenceValue = monthlyTabImage;
        serializedObject.FindProperty("_weeklyTabImage").objectReferenceValue = weeklyTabImage;
        serializedObject.FindProperty("_monthlyTabText").objectReferenceValue = monthlyTabText;
        serializedObject.FindProperty("_weeklyTabText").objectReferenceValue = weeklyTabText;
        serializedObject.FindProperty("_selectedTabSprite").objectReferenceValue = selectedTabSprite;
        serializedObject.FindProperty("_periodText").objectReferenceValue = periodText;
        serializedObject.FindProperty("_totalText").objectReferenceValue = totalText;
        serializedObject.FindProperty("_closeButton").objectReferenceValue = closeButton;
        serializedObject.ApplyModifiedPropertiesWithoutUndo();
    }

    /// <summary> HistoryPopupViewの参照を設定する </summary>
    static void SetHistoryPopupReferences(HistoryPopupView popupView, GameObject root, GameObject blackCover, Transform contentRoot, HistoryItemView itemPrefab, Button closeButton)
    {
        var serializedObject = new SerializedObject(popupView);
        serializedObject.FindProperty("_root").objectReferenceValue = root;
        serializedObject.FindProperty("_blackCoverObject").objectReferenceValue = blackCover;
        serializedObject.FindProperty("_contentRoot").objectReferenceValue = contentRoot;
        serializedObject.FindProperty("_itemPrefab").objectReferenceValue = itemPrefab;
        serializedObject.FindProperty("_closeButton").objectReferenceValue = closeButton;
        serializedObject.ApplyModifiedPropertiesWithoutUndo();
    }

    /// <summary> SimplePopupViewの参照を設定する </summary>
    static void SetSimplePopupReferences(SimplePopupView popupView, GameObject root, GameObject blackCover, TextMeshProUGUI messageText, Button okButton, TextMeshProUGUI okButtonText, Button cancelButton, TextMeshProUGUI cancelButtonText)
    {
        var serializedObject = new SerializedObject(popupView);
        serializedObject.FindProperty("_root").objectReferenceValue = root;
        serializedObject.FindProperty("_blackCoverObject").objectReferenceValue = blackCover;
        serializedObject.FindProperty("_messageText").objectReferenceValue = messageText;
        serializedObject.FindProperty("_okButton").objectReferenceValue = okButton;
        serializedObject.FindProperty("_okButtonText").objectReferenceValue = okButtonText;
        serializedObject.FindProperty("_cancelButton").objectReferenceValue = cancelButton;
        serializedObject.FindProperty("_cancelButtonText").objectReferenceValue = cancelButtonText;
        serializedObject.ApplyModifiedPropertiesWithoutUndo();
    }

    /// <summary> HistoryItemViewの参照を設定する </summary>
    static void SetHistoryItemReferences(HistoryItemView itemView, Button itemButton, TextMeshProUGUI dateText, TextMeshProUGUI amountText, TextMeshProUGUI noteText)
    {
        var serializedObject = new SerializedObject(itemView);
        serializedObject.FindProperty("_itemButton").objectReferenceValue = itemButton;
        serializedObject.FindProperty("_dateTimeText").objectReferenceValue = dateText;
        serializedObject.FindProperty("_amountText").objectReferenceValue = amountText;
        serializedObject.FindProperty("_noteText").objectReferenceValue = noteText;
        serializedObject.ApplyModifiedPropertiesWithoutUndo();
    }
}
