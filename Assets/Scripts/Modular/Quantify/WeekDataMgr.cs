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
        string allDataPath = Path.Combine(Path.GetDirectoryName(Application.dataPath), "AllData");
        weeklyDataFullPath = Path.Combine(allDataPath, WeekDataDirectory);
        if (!Directory.Exists(weeklyDataFullPath))
            Directory.CreateDirectory(weeklyDataFullPath);
    }

    private string GetWeekFilePath(string tsCode)
    {
        return Path.Combine(weeklyDataFullPath, $"{tsCode}.json");
    }

    // 自动更新所有股票周线（增量）
    public void UpdateAllWeeklyStocksRecursive()
    {
        try
        {
            var jsonFiles = Directory.GetFiles(weeklyDataFullPath, "*.json", SearchOption.TopDirectoryOnly);
            if (jsonFiles.Length == 0)
            {
                Debug.LogError("该路径下没有找到 json 文件");
                return;
            }

            string firstJsonPath = jsonFiles.First();
            List<StockWeeklyData> stockWeeklyDatas = LoadWeeklyStockDataFromFile(firstJsonPath);

            if (stockWeeklyDatas == null || stockWeeklyDatas.Count == 0)
            {
                Debug.LogError("无历史周线数据，直接获取最近30周");
                _ = GetAllStocksLastNWeeksAsync(30);
                return;
            }

            string latestDateStr = stockWeeklyDatas[0].TradeDate;
            DateTime latestDate = DateTime.ParseExact(latestDateStr, "yyyyMMdd", null);
            DateTime today = DateTime.Now;

            int days = (int)(today.Date - latestDate.Date).TotalDays;
            int weeks = (days + 6) / 7;

            if (weeks <= 0)
            {
                EventManager.Ins.Emit("UpdateStatus", "周线数据已是最新！");
                EventManager.Ins.Emit("OnFetchFin");
                return;
            }

            _ = GetAllStocksLastNWeeksAsync(weeks);
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }

    // 🔥 并发获取 N 周数据（超快、不卡）
    public async Task GetAllStocksLastNWeeksAsync(int weeks = 30)
    {
        try
        {
            EventManager.Ins.Emit("UpdateStatus", $"并发获取 {weeks} 周股票数据...");
            EventManager.Ins.Emit("OnFetchStart");

            DateTime endDate = DateTime.Now;
            List<DateTime> dateList = new List<DateTime>();

            for (int i = 0; i < weeks; i++)
                dateList.Add(endDate.AddDays(-i * 7));

            var allWeekData = new List<StockWeeklyData>();
            const int maxParallel = 6; // 周线接口更重，稍微低一点

            // 分批并发请求
            for (int i = 0; i < dateList.Count; i += maxParallel)
            {
                var batch = dateList.Skip(i).Take(maxParallel).ToList();
                var tasks = batch.Select(day => FetchWeekDataAsync(day)).ToList();
                var results = await Task.WhenAll(tasks);

                foreach (var data in results)
                    allWeekData.AddRange(data);

                EventManager.Ins.Emit("UpdateStatus", $"已获取 {i + tasks.Count}/{weeks} 周数据");
                await Task.Yield();
            }

            // 分组
            var grouped = allWeekData
                .GroupBy(d => d.TsCode)
                .ToDictionary(g => g.Key, g => g.OrderByDescending(x => x.TradeDate).ToList());

            // 并发保存
            var saveTasks = grouped.Select(kvp =>
                Task.Run(() => SaveWeeklyStockData(kvp.Key, kvp.Value))
            ).ToList();
            await Task.WhenAll(saveTasks);

            EventManager.Ins.Emit("UpdateStatus", $"✅ 周线数据获取完成！共 {allWeekData.Count} 条");
            EventManager.Ins.Emit("OnFetchFin");
        }
        catch (Exception ex)
        {
            EventManager.Ins.Emit("UpdateStatus", $"❌ 周线获取失败：{ex.Message}");
            Debug.LogError(ex);
        }
    }

    // 单日周线请求
    private async Task<List<StockWeeklyData>> FetchWeekDataAsync(DateTime day)
    {
        try
        {
            string tradeDate = day.ToString("yyyyMMdd");
            return await Mgrs.Ins.tushareMgr.GetWeeklyStocksByDate(tradeDate);
        }
        catch
        {
            return new List<StockWeeklyData>();
        }
    }

    public void SaveWeeklyStockData(string tsCode, List<StockWeeklyData> weekData)
    {
        try
        {
            if (weekData == null || weekData.Count == 0)
                return;

            string filePath = GetWeekFilePath(tsCode);
            List<StockWeeklyData> existingData = LoadWeeklyStockDataFromFile(filePath);
            List<StockWeeklyData> mergedData = MergeBaseDataOptimized(existingData, weekData);

            string json = JsonConvert.SerializeObject(mergedData, Formatting.Indented);
            File.WriteAllText(filePath, json, System.Text.Encoding.UTF8);
        }
        catch { }
    }

    private List<StockWeeklyData> LoadWeeklyStockDataFromFile(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
                return new List<StockWeeklyData>();

            string json = File.ReadAllText(filePath, System.Text.Encoding.UTF8);
            return JsonConvert.DeserializeObject<List<StockWeeklyData>>(json);
        }
        catch
        {
            return new List<StockWeeklyData>();
        }
    }

    private List<T> MergeBaseDataOptimized<T>(List<T> existingData, List<T> newData) where T : StockBaseData
    {
        if (existingData.Count == 0)
            return newData;

        var mergedData = new List<T>();
        int i = 0, j = 0;

        while (i < existingData.Count && j < newData.Count)
        {
            int comparison = string.Compare(existingData[i].TradeDate, newData[j].TradeDate);
            if (comparison > 0)
            {
                mergedData.Add(existingData[i]);
                i++;
            }
            else if (comparison < 0)
            {
                MacdCalculator.CalculateAndSetNewMacd(mergedData, newData[j]);
                mergedData.Add(newData[j]);
                j++;
            }
            else
            {
                MacdCalculator.CalculateAndSetNewMacd(mergedData, newData[j]);
                mergedData.Add(newData[j]);
                i++;
                j++;
            }
        }

        while (i < existingData.Count)
            mergedData.Add(existingData[i++]);

        while (j < newData.Count)
            mergedData.Add(newData[j++]);

        return mergedData;
    }

    public override void Release() { }

    public string GetWeekDataDirectory() => weeklyDataFullPath;
}