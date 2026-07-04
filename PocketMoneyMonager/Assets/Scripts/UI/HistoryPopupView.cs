using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary> 履歴一覧ポップアップView </summary>
public class HistoryPopupView : MonoBehaviour
{
    [SerializeField] GameObject _root;
    [SerializeField] Transform _contentRoot;
    [SerializeField] HistoryItemView _itemPrefab;
    [SerializeField] Button _closeButton;

    readonly List<HistoryItemView> _activeItems = new List<HistoryItemView>();

    /// <summary> 閉じるボタンクリック時 </summary>
    public event Action CloseClicked;

    /// <summary> 初期化処理 </summary>
    void Awake() => _closeButton.onClick.AddListener(OnCloseButtonClicked);

    /// <summary> 破棄時の処理 </summary>
    void OnDestroy() => _closeButton.onClick.RemoveListener(OnCloseButtonClicked);

    /// <summary> ポップアップを表示する </summary>
    public void Show() => _root.SetActive(true);

    /// <summary> ポップアップを非表示にする </summary>
    public void Hide() => _root.SetActive(false);

    /// <summary> 履歴一覧を表示する </summary>
    public void SetHistory(IReadOnlyList<Transaction> transactions)
    {
        ClearItems();

        var sortedTransactions = transactions.OrderByDescending(transaction => transaction.DateTime).ToList();
        foreach (var transaction in sortedTransactions)
        {
            var itemView = Instantiate(_itemPrefab, _contentRoot);
            itemView.gameObject.SetActive(true);
            itemView.SetData(transaction);
            _activeItems.Add(itemView);
        }
    }

    /// <summary> 閉じるボタンクリック時の処理 </summary>
    void OnCloseButtonClicked() => CloseClicked?.Invoke();

    /// <summary> 表示中の履歴アイテムをクリアする </summary>
    void ClearItems()
    {
        foreach (var itemView in _activeItems)
        {
            Destroy(itemView.gameObject);
        }

        _activeItems.Clear();
    }
}
