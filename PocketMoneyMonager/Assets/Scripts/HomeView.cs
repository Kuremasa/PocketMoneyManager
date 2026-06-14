using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary> ホーム画面View </summary>
public class HomeView : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _balanceText;
    [SerializeField] Button _expenseButton;
    [SerializeField] Button _summaryButton;
    [SerializeField] Button _historyButton;
    [SerializeField] Button _configButton;

    /// <summary> 消費ボタンクリック時 </summary>
    public event Action ExpenseButtonClicked;

    /// <summary> 消費累計ボタンクリック時 </summary>
    public event Action SummaryButtonClicked;

    /// <summary> 履歴ボタンクリック時 </summary>
    public event Action HistoryButtonClicked;

    /// <summary> コンフィグボタンクリック時 </summary>
    public event Action ConfigButtonClicked;

    /// <summary> 所持金表示を更新する </summary>
    public void SetBalance(int balance) =>
        _balanceText.text = $"今月のおこづかいは残り {CurrencyFormatter.Format(balance)}円 です";

    /// <summary> 初期化処理 </summary>
    void Start()
    {
        _expenseButton.onClick.AddListener(OnExpenseButtonClicked);
        _summaryButton.onClick.AddListener(OnSummaryButtonClicked);
        _historyButton.onClick.AddListener(OnHistoryButtonClicked);
        _configButton.onClick.AddListener(OnConfigButtonClicked);
    }

    /// <summary> 破棄時の処理 </summary>
    void OnDestroy()
    {
        _expenseButton.onClick.RemoveListener(OnExpenseButtonClicked);
        _summaryButton.onClick.RemoveListener(OnSummaryButtonClicked);
        _historyButton.onClick.RemoveListener(OnHistoryButtonClicked);
        _configButton.onClick.RemoveListener(OnConfigButtonClicked);
    }

    /// <summary> 消費ボタンクリック時の処理 </summary>
    void OnExpenseButtonClicked() => ExpenseButtonClicked?.Invoke();

    /// <summary> 消費累計ボタンクリック時の処理 </summary>
    void OnSummaryButtonClicked() => SummaryButtonClicked?.Invoke();

    /// <summary> 履歴ボタンクリック時の処理 </summary>
    void OnHistoryButtonClicked() => HistoryButtonClicked?.Invoke();

    /// <summary> コンフィグボタンクリック時の処理 </summary>
    void OnConfigButtonClicked() => ConfigButtonClicked?.Invoke();
}
