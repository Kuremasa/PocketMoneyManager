using System;
using System.Collections.Generic;
using System.Linq;

/// <summary> 集計対象期間 </summary>
public struct SummaryDateRange
{
    /// <summary> 開始日 </summary>
    public DateTime Start { get; }

    /// <summary> 終了日 </summary>
    public DateTime End { get; }

    /// <summary> 集計対象期間を生成する </summary>
    public SummaryDateRange(DateTime start, DateTime end)
    {
        Start = start;
        End = end;
    }
}

/// <summary> 消費金額の集計ロジック </summary>
public static class ExpenseAggregator
{
    /// <summary> 指定月の集計対象期間を取得する </summary>
    public static SummaryDateRange GetMonthlyDateRange(DateTime referenceDate)
    {
        var monthStart = new DateTime(referenceDate.Year, referenceDate.Month, 1);
        var monthEnd = monthStart.AddMonths(1).AddDays(-1);
        return new SummaryDateRange(monthStart, monthEnd);
    }

    /// <summary> 指定週（月曜始まり）の集計対象期間を取得する </summary>
    public static SummaryDateRange GetWeeklyDateRange(DateTime referenceDate)
    {
        var weekStart = GetWeekStart(referenceDate);
        var weekEnd = weekStart.AddDays(6);
        return new SummaryDateRange(weekStart, weekEnd);
    }

    /// <summary> 指定月の消費合計を計算する </summary>
    public static int CalculateMonthlyTotal(IEnumerable<Transaction> transactions, DateTime referenceDate)
    {
        var range = GetMonthlyDateRange(referenceDate);
        return SumInDateRange(transactions, range.Start, range.End);
    }

    /// <summary> 指定週（月曜始まり）の消費合計を計算する </summary>
    public static int CalculateWeeklyTotal(IEnumerable<Transaction> transactions, DateTime referenceDate)
    {
        var range = GetWeeklyDateRange(referenceDate);
        return SumInDateRange(transactions, range.Start, range.End);
    }

    /// <summary> 週の開始日（月曜）を取得する </summary>
    static DateTime GetWeekStart(DateTime referenceDate)
    {
        var daysFromMonday = ((int)referenceDate.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
        return referenceDate.Date.AddDays(-daysFromMonday);
    }

    /// <summary> 期間内の消費合計を計算する </summary>
    static int SumInDateRange(IEnumerable<Transaction> transactions, DateTime rangeStart, DateTime rangeEnd) =>
        transactions
            .Where(transaction => transaction.Date.Date >= rangeStart.Date && transaction.Date.Date <= rangeEnd.Date)
            .Sum(transaction => transaction.amount);
}
