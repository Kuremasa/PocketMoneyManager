using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary> 消費累計ポップアップView </summary>
public class ExpenseSummaryPopupView : MonoBehaviour
{
    /// <summary> 集計タブ種別 </summary>
    public enum SummaryTab
    {
        /// <summary> 月間 </summary>
        Monthly,

        /// <summary> 週間 </summary>
        Weekly
    }

    [SerializeField] GameObject _root;
    [SerializeField] Button _monthlyTabButton;
    [SerializeField] Button _weeklyTabButton;
    [SerializeField] TextMeshProUGUI _totalText;
    [SerializeField] Button _closeButton;

    SummaryTab _currentTab = SummaryTab.Monthly;

    /// <summary> タブ切り替え時 </summary>
    public event Action<SummaryTab> TabChanged;

    /// <summary> 閉じるボタンクリック時 </summary>
    public event Action CloseClicked;

    /// <summary> 初期化処理 </summary>
    void Awake()
    {
        _monthlyTabButton.onClick.AddListener(OnMonthlyTabButtonClicked);
        _weeklyTabButton.onClick.AddListener(OnWeeklyTabButtonClicked);
        _closeButton.onClick.AddListener(OnCloseButtonClicked);
    }

    /// <summary> 破棄時の処理 </summary>
    void OnDestroy()
    {
        _monthlyTabButton.onClick.RemoveListener(OnMonthlyTabButtonClicked);
        _weeklyTabButton.onClick.RemoveListener(OnWeeklyTabButtonClicked);
        _closeButton.onClick.RemoveListener(OnCloseButtonClicked);
    }

    /// <summary> ポップアップを表示する </summary>
    public void Show()
    {
        _root.SetActive(true);
        _currentTab = SummaryTab.Monthly;
        TabChanged?.Invoke(_currentTab);
    }

    /// <summary> ポップアップを非表示にする </summary>
    public void Hide() => _root.SetActive(false);

    /// <summary> 合計金額を表示する </summary>
    public void SetTotal(int total) => _totalText.text = $"{CurrencyFormatter.Format(total)}円";

    /// <summary> 月間タブボタンクリック時の処理 </summary>
    void OnMonthlyTabButtonClicked()
    {
        _currentTab = SummaryTab.Monthly;
        TabChanged?.Invoke(_currentTab);
    }

    /// <summary> 週間タブボタンクリック時の処理 </summary>
    void OnWeeklyTabButtonClicked()
    {
        _currentTab = SummaryTab.Weekly;
        TabChanged?.Invoke(_currentTab);
    }

    /// <summary> 閉じるボタンクリック時の処理 </summary>
    void OnCloseButtonClicked() => CloseClicked?.Invoke();
}
