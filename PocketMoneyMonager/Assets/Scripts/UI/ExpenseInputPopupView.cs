using System;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary> 消費入力ポップアップView </summary>
public class ExpenseInputPopupView : MonoBehaviour
{
    const int NoteMaxLength = 50;

    [SerializeField] GameObject _root;
    [SerializeField] TMP_InputField _dateInput;
    [SerializeField] TMP_InputField _amountInput;
    [SerializeField] TMP_InputField _noteInput;
    [SerializeField] TextMeshProUGUI _errorText;
    [SerializeField] Button _registerButton;
    [SerializeField] Button _cancelButton;
    [SerializeField] GameObject _blackCoverObject;

    /// <summary> 登録ボタンクリック時 </summary>
    public event Action<DateTime, int, string> RegisterClicked;

    /// <summary> キャンセルボタンクリック時 </summary>
    public event Action CancelClicked;

    /// <summary> 初期化処理 </summary>
    void Awake()
    {
        _noteInput.characterLimit = NoteMaxLength;
        _registerButton.onClick.AddListener(OnRegisterButtonClicked);
        _cancelButton.onClick.AddListener(OnCancelButtonClicked);
    }

    /// <summary> 破棄時の処理 </summary>
    void OnDestroy()
    {
        _registerButton.onClick.RemoveListener(OnRegisterButtonClicked);
        _cancelButton.onClick.RemoveListener(OnCancelButtonClicked);
    }

    /// <summary> ポップアップを表示する </summary>
    public void Show()
    {
        _blackCoverObject.SetActive(true);
        _root.SetActive(true);
        _dateInput.text = DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        _amountInput.text = string.Empty;
        _noteInput.text = string.Empty;
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
}
