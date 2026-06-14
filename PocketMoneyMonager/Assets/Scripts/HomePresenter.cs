using UnityEngine;

/// <summary> ホーム画面Presenter </summary>
public class HomePresenter : MonoBehaviour
{
    [SerializeField] UIManager _uiManager;

    /// <summary> 初期化処理 </summary>
    void Awake()
    {
        DataManager.Instance.Load();
        DataManager.Instance.TryApplyMonthlyAllowance();
        _uiManager.BindHomeButtons(OnExpenseButton, OnSummaryButton, OnHistoryButton);
        _uiManager.ConfigButtonClicked += OnConfigButtonClicked;
        _uiManager.RefreshHome();
    }

    /// <summary> 破棄時の処理 </summary>
    void OnDestroy()
    {
        _uiManager.UnbindHomeButtons(OnExpenseButton, OnSummaryButton, OnHistoryButton);
        _uiManager.ConfigButtonClicked -= OnConfigButtonClicked;
    }

    /// <summary> 消費ボタン押下時の処理 </summary>
    void OnExpenseButton() => _uiManager.ShowExpenseInputPopup();

    /// <summary> 消費累計ボタン押下時の処理 </summary>
    void OnSummaryButton() => _uiManager.ShowExpenseSummaryPopup();

    /// <summary> 履歴ボタン押下時の処理 </summary>
    void OnHistoryButton() => _uiManager.ShowHistoryPopup();

    /// <summary> コンフィグボタンクリック時の処理 </summary>
    void OnConfigButtonClicked() => Debug.Log("Config: not implemented");
}
