using TMPro;
using UnityEngine;

/// <summary> 履歴一覧の1行View </summary>
public class HistoryItemView : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _dateText;
    [SerializeField] TextMeshProUGUI _amountText;
    [SerializeField] TextMeshProUGUI _noteText;

    /// <summary> 履歴1件分の表示を更新する </summary>
    public void SetData(Transaction transaction)
    {
        _dateText.text = transaction.date;
        _amountText.text = $"{CurrencyFormatter.Format(transaction.amount)}円";
        _noteText.text = transaction.note;
    }
}
