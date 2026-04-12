using ClientBase;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class WeekDataMgr : MgrBase
{
    private string WeekDataDirectory = "AllWeekData";
    private string weeklyDataFullPath;

    public override void Init()
    {
        // 计算Assets同级目录的AllData路径
        string allDataPath = Path.Combine(Path.GetDirectoryName(Application.dataPath), "AllData");
        weeklyDataFullPath = Path.Combine(allDataPath, WeekDataDirectory);
        if (!Directory.Exists(weeklyDataFullPath))
            Directory.CreateDirectory(weeklyDataFullPath);
    }

    private string GetWeekFilePath(string tsCode)
    {
        return Path.Combine(weeklyDataFullPath, $"{tsCode}.json");
    }

    // 递归获取所有股票的每日数据
    public void UpdateAllWeeklyStocksRecursive()
    {
        var jsonFiles = Directory.GetFiles(weeklyDataFullPath, "*.json", SearchOption.TopDirectoryOnly);
        // 检查是否有 json 文件
        if (jsonFiles.Length == 0)
        {
            Debug.LogError("该路径下没有找到 json 文件");
            return;
        }
        // 获取第一个 json 文件的路径
        string firstJsonPath = jsonFiles.First();
        List<StockWeeklyData> stockWeeklyDatas = LoadWeeklyStockDataFromFile(firstJsonPath);
        string latestDateStr = stockWeeklyDatas[0].TradeDate;
        DateTime latestDate = DateTime.ParseExact(latestDateStr, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None);
        DateTime today = DateTime.Now;
        TimeSpan difference = today - latestDate;
        GetAllStocksLastNWeeksRecursive(difference.Days);
    }

    // 获取最近N周所有股票周线数据，并赋值到StockWeeklyDataList
    public async void GetAllStocksLastNWeeksRecursive(int weeks = 30)
    {
        EventManager.Ins.Emit("UpdateStatus", $"递归获取所有股票最近{weeks}周数据...");
        EventManager.Ins.Emit("OnFetchStart");
        DateTime endDate = DateTime.Now;
        List<StockWeeklyData> allWeekData = new List<StockWeeklyData>();
        await GetStocksByWeekRecursive(endDate, weeks, allWeekData);
        Dictionary<string, List<StockWeeklyData>> grouped = allWeekData.GroupBy(d => d.TsCode)
                                  .ToDictionary(g => g.Key, g => g.OrderByDescending(x => x.TradeDate).ToList());
        foreach (KeyValuePair<string, List<StockWeeklyData>> stockData in grouped)
        {
            SaveWeeklyStockData(stockData.Key, stockData.Value);
        }
        EventManager.Ins.Emit("UpdateStatus", $"获取完成");
        EventManager.Ins.Emit("OnFetchFin");
    }

    // 递归获取周线数据
    private async Task GetStocksByWeekRecursive(DateTime date, int remainingWeeks, List<StockWeeklyData> allWeekData)
    {
        if (remainingWeeks <= 0) return;

        string tradeDate = date.ToString("yyyyMMdd");
        List<StockWeeklyData> weekList = await Mgrs.Ins.tushareMgr.GetWeeklyStocksByDate(tradeDate);
        allWeekData.AddRange(weekList);

        // 递归获取前一周
        await GetStocksByWeekRecursive(date.AddDays(-7), remainingWeeks - 1, allWeekData);
    }

    public void SaveWeeklyStockData(string tsCode, List<StockWeeklyData> weekData)
    {
        if (weekData == null || weekData.Count == 0)
            return;
        string filePath = GetWeekFilePath(tsCode);


        List<StockWeeklyData> existingData = LoadWeeklyStockDataFromFile(filePath);

        // 合并数据（增量插入）
        List<StockWeeklyData> mergedData = MergeBaseDataOptimized(existingData, weekData);
        string json = JsonConvert.SerializeObject(mergedData);
        File.WriteAllText(filePath, json, System.Text.Encoding.UTF8);
    }

    private List<StockWeeklyData> LoadWeeklyStockDataFromFile(string filePath)
    {
        if (!File.Exists(filePath))
            return new List<StockWeeklyData>();
        string json = File.ReadAllText(filePath, System.Text.Encoding.UTF8);
        List<StockWeeklyData> weekDataList = JsonConvert.DeserializeObject<List<StockWeeklyData>>(json);
        return weekDataList;
    }

    private List<T> MergeBaseDataOptimized<T>(List<T> existingData, List<T> newData) where T : StockBaseData
    {
        if (existingData.Count == 0)
            return newData;

        var mergedData = new List<T>();
        int i = 0, j = 0;
        // 由于两个列表都是倒序排列，所以从前往后遍历（最新的日期在前）
        while (i < existingData.Count && j < newData.Count)
        {
            int comparison = string.Compare(existingData[i].TradeDate, newData[j].TradeDate);// 比较日期字符串（假设格式一致，如"yyyy-MM-dd"）
            if (comparison > 0)
            {
                mergedData.Add(existingData[i]);// existingData的日期较新，先加入结果
                i++;
            }
            else if (comparison < 0)
            {
                MacdCalculator.CalculateAndSetNewMacd(mergedData, newData[j]);
                // newData的日期较新，先加入结果
                mergedData.Add(newData[j]);
                //赋值新数据的macd值
                j++;
            }
            else
            {
                MacdCalculator.CalculateAndSetNewMacd(mergedData, newData[j]);
                // 日期相同，使用newData的数据，并跳过existingData中的对应数据
                mergedData.Add(newData[j]);
                //赋值新数据的macd值
                i++;
                j++;
            }
        }
        // 处理剩余的元素
        while (i < existingData.Count)
        {
            mergedData.Add(existingData[i]);
            i++;
        }
        while (j < newData.Count)
        {
            mergedData.Add(newData[j]);
            j++;
        }
        return mergedData;
    }

    public override void Release()
    {

    }

    // 获取周数据目录路径
    public string GetWeekDataDirectory()
    {
        return weeklyDataFullPath;
    }
}