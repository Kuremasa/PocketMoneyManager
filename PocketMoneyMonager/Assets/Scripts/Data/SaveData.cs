using System;

/// <summary> セーブデータ </summary>
[Serializable]
public class SaveData
{
    /// <summary> 現在の所持金 </summary>
    public int balance;

    /// <summary> 最後に月次付与した年月（yyyy-MM） </summary>
    public string lastAddedMonth = string.Empty;

    /// <summary> 消費履歴 </summary>
    public Transaction[] transactions = Array.Empty<Transaction>();
}
