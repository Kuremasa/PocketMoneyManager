using System;
using UnityEngine;

/// <summary> UI全体の表示制御 </summary>
public class UIManager : MonoBehaviour
{
    [SerializeField] HomeView _homeView;
    [SerializeField] ExpenseInputPopupView _expenseInputPopup;
    [SerializeField] ExpenseSummaryPopupView _expenseSummaryPopup;
    [SerializeField] HistoryPopupView _historyPopup;
    [SerializeField] SimplePopupView _simplePopup;

    /// <summary> コンフィグボタンクリック時 </summary>
    public event Action ConfigButtonClicked;

    /// <summary> 初期化処理 </summary>
    void Awake()
    {
        _homeView.ConfigButtonClicked += OnConfigButtonClicked;
        _expenseInputPopup.RegisterClicked += OnExpenseRegisterClicked;
        _expenseInputPopup.CancelClicked += HideExpenseInputPopup;
        _expenseSummaryPopup.TabChanged += OnSummaryTabChanged;
        _expenseSummaryPopup.CloseClicked += HideExpenseSummaryPopup;
        _historyPopup.CloseClicked += HideHistoryPopup;
    }

    /// <summary> 破棄時の処理 </summary>
    void OnDestroy()
    {
        _homeView.ConfigButtonClicked -= OnConfigButtonClicked;
        _expenseInputPopup.RegisterClicked -= OnExpenseRegisterClicked;
        _expenseInputPopup.CancelClicked -= HideExpenseInputPopup;
        _expenseSummaryPopup.TabChanged -= OnSummaryTabChanged;
        _expenseSummaryPopup.CloseClicked -= HideExpenseSummaryPopup;
        _historyPopup.CloseClicked -= HideHistoryPopup;
    }

    /// <summary> ホーム画面のボタンイベントを登録する </summary>
    public void BindHomeButtons(Action onExpense, Action onSummary, Action onHistory)
    {
        _homeView.ExpenseButtonClicked += onExpense;
        _homeView.SummaryButtonClicked += onSummary;
        _homeView.HistoryButtonClicked += onHistory;
    }

    /// <summary> ホーム画面のボタンイベントを解除する </summary>
    public void UnbindHomeButtons(Action onExpense, Action onSummary, Action onHistory)
    {
        _homeView.ExpenseButtonClicked -= onExpense;
        _homeView.SummaryButtonClicked -= onSummary;
        _homeView.HistoryButtonClicked -= onHistory;
    }

    /// <summary> ホーム画面を更新する </summary>
    public void RefreshHome() => _homeView.SetBalance(DataManager.Instance.Balance);

    /// <summary> 消費入力ポップアップを表示する </summary>
    public void ShowExpenseInputPopup() => _expenseInputPopup.Show();

    /// <summary> 消費入力ポップアップを非表示にする </summary>
    public void HideExpenseInputPopup() => _expenseInputPopup.Hide();

    /// <summary> 消費累計ポップアップを表示する </summary>
    public void ShowExpenseSummaryPopup() => _expenseSummaryPopup.Show();

    /// <summary> 消費累計ポップアップを非表示にする </summary>
    public void HideExpenseSummaryPopup() => _expenseSummaryPopup.Hide();

    /// <summary> 履歴ポップアップを表示する </summary>
    public void ShowHistoryPopup()
    {
        _historyPopup.SetHistory(DataManager.Instance.Transactions);
        _historyPopup.Show();
    }

    /// <summary> 履歴ポップアップを非表示にする </summary>
    public void HideHistoryPopup() => _historyPopup.Hide();

    /// <summary> 2ボタンタイプの簡易ポップアップを表示する </summary>
    public void ShowSimplePopupTwoButton(string message, string okText, string cancelText, Action<bool> onClosed)
    {
        void OnOk()
        {
            Unsubscribe();
            onClosed?.Invoke(true);
            HideSimplePopup();
        }

        void OnCancel()
        {
            Unsubscribe();
            onClosed?.Invoke(false);
            HideSimplePopup();
        }

        void Unsubscribe()
        {
            _simplePopup.OkClicked -= OnOk;
            _simplePopup.CancelClicked -= OnCancel;
        }

        _simplePopup.OkClicked += OnOk;
        _simplePopup.CancelClicked += OnCancel;
        _simplePopup.ShowTwoButton(message, okText, cancelText);
    }

    /// <summary> 1ボタンタイプの簡易ポップアップを表示する </summary>
    public void ShowSimplePopupOneButton(string message, string okText, Action onClosed = null)
    {
        void OnOk()
        {
            _simplePopup.OkClicked -= OnOk;
            onClosed?.Invoke();
            HideSimplePopup();
        }

        _simplePopup.OkClicked += OnOk;
        _simplePopup.ShowOneButton(message, okText);
    }

    /// <summary> 簡易ポップアップを非表示にする </summary>
    public void HideSimplePopup() => _simplePopup.Hide();

    /// <summary> 消費登録時の処理 </summary>
    void OnExpenseRegisterClicked(DateTime date, int amount, string note)
    {
        if (!DataManager.Instance.RegisterExpense(date, amount, note))
        {
            _expenseInputPopup.SetErrorMessage("金額は1以上の整数で入力してください");
            return;
        }

        RefreshHome();
        HideExpenseInputPopup();
        ShowSimplePopupOneButton("入力完了しました！", "OK");
    }

    /// <summary> 集計タブ切り替え時の処理 </summary>
    void OnSummaryTabChanged(ExpenseSummaryPopupView.SummaryTab tab)
    {
        var referenceDate = DateTime.Now;
        var total = tab == ExpenseSummaryPopupView.SummaryTab.Monthly
            ? DataManager.Instance.GetMonthlyTotal(referenceDate)
            : DataManager.Instance.GetWeeklyTotal(referenceDate);
        _expenseSummaryPopup.SetTotal(total);
    }

    /// <summary> コンフィグボタンクリック時の処理 </summary>
    void OnConfigButtonClicked() => ConfigButtonClicked?.Invoke();
}
