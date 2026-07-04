using System;
using System.Globalization;

/// <summary> 消費履歴1件分のデータ </summary>
[Serializable]
public struct Transaction
{
    const string DateFormat = "yyyy-MM-dd";
    const string TimeFormat = "HH:mm";
    const string DateTimeFormat = "yyyy-MM-dd HH:mm";

    /// <summary> 日付文字列（yyyy-MM-dd） </summary>
    public string date;

    /// <summary> 時刻文字列（HH:mm） </summary>
    public string time;

    /// <summary> 消費金額 </summary>
    public int amount;

    /// <summary> 用途 </summary>
    public string note;

    /// <summary> 日付 </summary>
    public DateTime Date => System.DateTime.ParseExact(date, DateFormat, CultureInfo.InvariantCulture);

    /// <summary> 日付と時刻 </summary>
    public DateTime DateTime =>
        string.IsNullOrEmpty(time)
            ? Date
            : System.DateTime.ParseExact($"{date} {time}", DateTimeFormat, CultureInfo.InvariantCulture);

    /// <summary> 履歴表示用の日時文字列 </summary>
    public string DisplayText => string.IsNullOrEmpty(time) ? date : $"{date} {time}";

    /// <summary> Transactionを生成する </summary>
    public static Transaction Create(DateTime transactionDateTime, int transactionAmount, string transactionNote) => new Transaction
    {
        date = transactionDateTime.ToString(DateFormat, CultureInfo.InvariantCulture),
        time = transactionDateTime.ToString(TimeFormat, CultureInfo.InvariantCulture),
        amount = transactionAmount,
        note = transactionNote ?? string.Empty
    };
}
