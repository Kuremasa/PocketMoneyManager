using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary> 簡易ポップアップView </summary>
public class SimplePopupView : MonoBehaviour
{
    [SerializeField] GameObject _root;
    [SerializeField] TextMeshProUGUI _messageText;
    [SerializeField] Button _okButton;
    [SerializeField] TextMeshProUGUI _okButtonText;
    [SerializeField] Button _cancelButton;
    [SerializeField] TextMeshProUGUI _cancelButtonText;

    /// <summary> OKボタンクリック時 </summary>
    public event Action OkClicked;

    /// <summary> キャンセルボタンクリック時 </summary>
    public event Action CancelClicked;

    /// <summary> 初期化処理 </summary>
    void Awake()
    {
        _okButton.onClick.AddListener(OnOkButtonClicked);
        _cancelButton.onClick.AddListener(OnCancelButtonClicked);
    }

    /// <summary> 破棄時の処理 </summary>
    void OnDestroy()
    {
        _okButton.onClick.RemoveListener(OnOkButtonClicked);
        _cancelButton.onClick.RemoveListener(OnCancelButtonClicked);
    }

    /// <summary> 2ボタンタイプを表示する </summary>
    public void ShowTwoButton(string message, string okText, string cancelText)
    {
        _messageText.text = message;
        _okButtonText.text = okText;
        _cancelButtonText.text = cancelText;
        _cancelButton.gameObject.SetActive(true);
        _root.SetActive(true);
    }

    /// <summary> 1ボタンタイプを表示する </summary>
    public void ShowOneButton(string message, string okText)
    {
        _messageText.text = message;
        _okButtonText.text = okText;
        _cancelButton.gameObject.SetActive(false);
        _root.SetActive(true);
    }

    /// <summary> ポップアップを非表示にする </summary>
    public void Hide() => _root.SetActive(false);

    /// <summary> OKボタンクリック時の処理 </summary>
    void OnOkButtonClicked() => OkClicked?.Invoke();

    /// <summary> キャンセルボタンクリック時の処理 </summary>
    void OnCancelButtonClicked() => CancelClicked?.Invoke();
}
