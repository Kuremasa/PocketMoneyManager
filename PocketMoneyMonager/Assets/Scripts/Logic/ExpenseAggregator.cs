using System;
using System.Collections.Generic;
using System.Linq;

/// <summary> 消費金額の集計ロジック </summary>
public static class ExpenseAggregator
{
    /// <summary> 指定月の消費合計を計算する </summary>
    public static int CalculateMonthlyTotal(IEnumerable<Transaction> transactions, DateTime referenceDate)
    {
        var monthStart = new DateTime(referenceDate.Year, referenceDate.Month, 1);
        var monthEnd = monthStart.AddMonths(1).AddDays(-1);
        return SumInDateRange(transactions, monthStart, monthEnd);
    }

    /// <summary> 指定週（月曜始まり）の消費合計を計算する </summary>
    public static int CalculateWeeklyTotal(IEnumerable<Transaction> transactions, DateTime referenceDate)
    {
        var weekStart = GetWeekStart(referenceDate);
        var weekEnd = weekStart.AddDays(6);
        return SumInDateRange(transactions, weekStart, weekEnd);
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
