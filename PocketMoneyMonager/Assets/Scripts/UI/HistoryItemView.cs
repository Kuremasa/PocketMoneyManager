using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary> 履歴一覧の1行View </summary>
public class HistoryItemView : MonoBehaviour
{
    [SerializeField] Button _itemButton;
    [SerializeField] TextMeshProUGUI _dateTimeText;
    [SerializeField] TextMeshProUGUI _amountText;
    [SerializeField] TextMeshProUGUI _noteText;

    Transaction _transaction;

    /// <summary> リストアイテムクリック時 </summary>
    public event Action<Transaction> ItemClicked;

    /// <summary> 初期化処理 </summary>
    void Awake() => _itemButton.onClick.AddListener(OnItemButtonClicked);

    /// <summary> 破棄時の処理 </summary>
    void OnDestroy() => _itemButton.onClick.RemoveListener(OnItemButtonClicked);

    /// <summary> 履歴1件分の表示を更新する </summary>
    public void SetData(Transaction transaction)
    {
        _transaction = transaction;
        _dateTimeText.text = transaction.DisplayText;
        _amountText.text = $"{CurrencyFormatter.Format(transaction.amount)}円";
        _noteText.text = transaction.note;
    }

    /// <summary> リストアイテムクリック時の処理 </summary>
    void OnItemButtonClicked() => ItemClicked?.Invoke(_transaction);
}
