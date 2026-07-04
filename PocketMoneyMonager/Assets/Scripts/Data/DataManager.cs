using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEngine;

/// <summary> セーブデータと資産管理を行うシングルトン </summary>
public sealed class DataManager
{
    /// <summary> シングルトンインスタンス </summary>
    public static DataManager Instance { get; } = new DataManager();

    const int MonthlyAllowance = 17000;
    const int NoteMaxLength = 50;
    const string SaveFileName = "save_data.json";
    const string MonthFormat = "yyyy-MM";
    const string DateFormat = "yyyy-MM-dd";

    readonly List<Transaction> _transactions = new List<Transaction>();
    SaveData _saveData = new SaveData();

    /// <summary> 現在の所持金 </summary>
    public int Balance => _saveData.balance;

    /// <summary> 消費履歴 </summary>
    public IReadOnlyList<Transaction> Transactions => _transactions;

    DataManager() { }

    /// <summary> セーブデータを読み込む </summary>
    public void Load()
    {
        var savePath = GetSavePath();
        if (!File.Exists(savePath))
        {
            ResetToDefault();
            return;
        }

        var json = File.ReadAllText(savePath);
        _saveData = JsonUtility.FromJson<SaveData>(json) ?? new SaveData();
        _transactions.Clear();
        if (_saveData.transactions != null)
        {
            _transactions.AddRange(_saveData.transactions);
        }
    }

    /// <summary> セーブデータを保存する </summary>
    public void Save()
    {
        _saveData.transactions = _transactions.ToArray();
        var json = JsonUtility.ToJson(_saveData, true);
        File.WriteAllText(GetSavePath(), json);
    }

    /// <summary> 毎月1日分のお小遣い付与を判定して適用する </summary>
    public void TryApplyMonthlyAllowance()
    {
        var currentMonth = DateTime.Now.ToString(MonthFormat, CultureInfo.InvariantCulture);

        if (string.IsNullOrEmpty(_saveData.lastAddedMonth))
        {
            ApplyAllowanceForMonth(currentMonth);
            Save();
            return;
        }

        if (_saveData.lastAddedMonth == currentMonth)
        {
            return;
        }

        var lastAdded = ParseMonthStart(_saveData.lastAddedMonth);
        var targetMonth = lastAdded.AddMonths(1);
        var currentMonthStart = ParseMonthStart(currentMonth);
        var allowanceApplied = false;

        while (targetMonth <= currentMonthStart)
        {
            var monthKey = targetMonth.ToString(MonthFormat, CultureInfo.InvariantCulture);
            ApplyAllowanceForMonth(monthKey);
            targetMonth = targetMonth.AddMonths(1);
            allowanceApplied = true;
        }

        if (allowanceApplied)
        {
            Save();
        }
    }

    /// <summary> 消費を登録する </summary>
    public bool RegisterExpense(DateTime date, int amount, string note)
    {
        if (amount <= 0)
        {
            return false;
        }

        var trimmedNote = TrimNote(note);
        var now = DateTime.Now;
        var transactionDateTime = new DateTime(date.Year, date.Month, date.Day, now.Hour, now.Minute, 0);
        _saveData.balance -= amount;
        _transactions.Add(Transaction.Create(transactionDateTime, amount, trimmedNote));
        Save();
        return true;
    }

    /// <summary> 今月の消費合計を取得する </summary>
    public int GetMonthlyTotal(DateTime referenceDate) =>
        ExpenseAggregator.CalculateMonthlyTotal(_transactions, referenceDate);

    /// <summary> 今週の消費合計を取得する </summary>
    public int GetWeeklyTotal(DateTime referenceDate) =>
        ExpenseAggregator.CalculateWeeklyTotal(_transactions, referenceDate);

    /// <summary> セーブファイルのパスを取得する </summary>
    static string GetSavePath() => Path.Combine(Application.persistentDataPath, SaveFileName);

    /// <summary> 初期状態にリセットする </summary>
    void ResetToDefault()
    {
        _saveData = new SaveData();
        _transactions.Clear();
    }

    /// <summary> 指定月のお小遣いを付与する </summary>
    void ApplyAllowanceForMonth(string monthKey)
    {
        _saveData.balance += MonthlyAllowance;
        _saveData.lastAddedMonth = monthKey;
    }

    /// <summary> 年月文字列から月初日を取得する </summary>
    static DateTime ParseMonthStart(string monthKey) =>
        DateTime.ParseExact($"{monthKey}-01", DateFormat, CultureInfo.InvariantCulture);

    /// <summary> 用途文字列を最大文字数に切り詰める </summary>
    static string TrimNote(string note)
    {
        if (string.IsNullOrEmpty(note))
        {
            return string.Empty;
        }

        return note.Length <= NoteMaxLength ? note : note.Substring(0, NoteMaxLength);
    }
}
