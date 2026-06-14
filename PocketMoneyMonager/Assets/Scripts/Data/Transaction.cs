using System;
using System.Globalization;

/// <summary> 消費履歴1件分のデータ </summary>
[Serializable]
public struct Transaction
{
    /// <summary> 日付文字列（yyyy-MM-dd） </summary>
    public string date;

    /// <summary> 消費金額 </summary>
    public int amount;

    /// <summary> 用途 </summary>
    public string note;

    /// <summary> 日付 </summary>
    public DateTime Date => DateTime.ParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture);

    /// <summary> Transactionを生成する </summary>
    public static Transaction Create(DateTime transactionDate, int transactionAmount, string transactionNote) => new Transaction
    {
        date = transactionDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
        amount = transactionAmount,
        note = transactionNote ?? string.Empty
    };
}
