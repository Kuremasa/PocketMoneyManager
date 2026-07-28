using System;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary> 消費入力ポップアップView </summary>
public class ExpenseInputPopupView : MonoBehaviour
{
    const int NoteMaxLength = 50;
    const string RegisterButtonLabel = "登録する";
    const string EditButtonLabel = "修正する";

    [SerializeField] GameObject _root;
    [SerializeField] TMP_InputField _dateInput;
    [SerializeField] TMP_InputField _amountInput;
    [SerializeField] TMP_InputField _noteInput;
    [SerializeField] TextMeshProUGUI _errorText;
    [SerializeField] Button _registerButton;
    [SerializeField] TextMeshProUGUI _registerButtonText;
    [SerializeField] Button _cancelButton;
    [SerializeField] Button _deleteButton;
    [SerializeField] GameObject _blackCoverObject;

    /// <summary> 登録ボタンクリック時 </summary>
    public event Action<DateTime, int, string> RegisterClicked;

    /// <summary> キャンセルボタンクリック時 </summary>
    public event Action CancelClicked;

    /// <summary> 履歴削除ボタンクリック時 </summary>
    public event Action DeleteClicked;

    /// <summary> 初期化処理 </summary>
    void Awake()
    {
        _noteInput.characterLimit = NoteMaxLength;
        _registerButton.onClick.AddListener(OnRegisterButtonClicked);
        _cancelButton.onClick.AddListener(OnCancelButtonClicked);
        _deleteButton.onClick.AddListener(OnDeleteButtonClicked);
    }

    /// <summary> 破棄時の処理 </summary>
    void OnDestroy()
    {
        _registerButton.onClick.RemoveListener(OnRegisterButtonClicked);
        _cancelButton.onClick.RemoveListener(OnCancelButtonClicked);
        _deleteButton.onClick.RemoveListener(OnDeleteButtonClicked);
    }

    /// <summary> ポップアップを新規登録用に表示する </summary>
    public void Show()
    {
        _blackCoverObject.SetActive(true);
        _root.SetActive(true);
        _dateInput.text = DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        _amountInput.text = string.Empty;
        _noteInput.text = string.Empty;
        _registerButtonText.text = RegisterButtonLabel;
        _deleteButton.gameObject.SetActive(false);
        SetErrorMessage(string.Empty);
    }

    /// <summary> ポップアップを履歴編集用に表示する </summary>
    public void ShowForEdit(Transaction transaction)
    {
        _blackCoverObject.SetActive(true);
        _root.SetActive(true);
        _dateInput.text = transaction.date;
        _amountInput.text = transaction.amount.ToString(CultureInfo.InvariantCulture);
        _noteInput.text = transaction.note;
        _registerButtonText.text = EditButtonLabel;
        _deleteButton.gameObject.SetActive(true);
        SetErrorMessage(string.Empty);
    }

    /// <summary> ポップアップを非表示にする </summary>
    public void Hide()
    {
        _blackCoverObject.SetActive(false);
        _root.SetActive(false);
    }

    /// <summary> エラーメッセージを表示する </summary>
    public void SetErrorMessage(string message) => _errorText.text = message;

    /// <summary> 登録ボタンクリック時の処理 </summary>
    void OnRegisterButtonClicked()
    {
        if (!DateTime.TryParseExact(_dateInput.text, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
        {
            SetErrorMessage("日付は yyyy-MM-dd 形式で入力してください");
            return;
        }

        if (!int.TryParse(_amountInput.text, out var amount) || amount <= 0)
        {
            SetErrorMessage("金額は1以上の整数で入力してください");
            return;
        }

        RegisterClicked?.Invoke(date, amount, _noteInput.text);
    }

    /// <summary> キャンセルボタンクリック時の処理 </summary>
    void OnCancelButtonClicked() => CancelClicked?.Invoke();

    /// <summary> 履歴削除ボタンクリック時の処理 </summary>
    void OnDeleteButtonClicked() => DeleteClicked?.Invoke();
}
