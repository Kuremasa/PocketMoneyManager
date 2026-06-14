using System.Globalization;

/// <summary> 金額表示用フォーマッタ </summary>
public static class CurrencyFormatter
{
    /// <summary> 金額を3桁カンマ区切りの文字列に変換する </summary>
    public static string Format(int amount) => amount.ToString("N0", CultureInfo.InvariantCulture);
}
