using ClientBase;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class DailyDataMgr : MgrBase
{
    private string DailyDataDirectory = "AllDailyData";
    private string dailyFullPath;

    public override void Init()
    {
        string allDataPath = Path.Combine(Path.GetDirectoryName(Application.dataPath), "AllData");
        dailyFullPath = Path.Combine(allDataPath, DailyDataDirectory);
        if (!Directory.Exists(dailyFullPath))
            Directory.CreateDirectory(dailyFullPath);
    }

    private string GetDailyFolderPath(string tsCode)
    {
        string stockId = tsCode.Split('.')[0];
        string stockFolderPath = Path.Combine(dailyFullPath, stockId);
        if (!Directory.Exists(stockFolderPath))
            Directory.CreateDirectory(stockFolderPath);
        return stockFolderPath;
    }

    private string GetQuarterlyFilePath(string stockFolderPath, string year, int quarter)
    {
        return Path.Combine(stockFolderPath, $"{year}-{quarter}.json");
    }

    public void UpdateAllDailyStocksRecursive()
    {
        try
        {
            var stockFolders = Directory.GetDirectories(dailyFullPath);
            if (stockFolders.Length == 0)
            {
                Debug.LogError("该路径下没有找到股票文件夹");
                return;
            }

            string firstStockFolder = stockFolders.First();
            string stockId = Path.GetFileName(firstStockFolder);
            string tsCode = $"{stockId}.SZ";

            List<StockDailyData> stockDailyDatas = LoadSingleDailyStockDataFromFile(tsCode);
            if (stockDailyDatas == null || stockDailyDatas.Count == 0)
            {
                Debug.LogError("无历史数据，直接获取最近30天");
                _ = GetAllStocksLastNDaysAsync(30);
                return;
            }

            string latestDateStr = stockDailyDatas[0].TradeDate;
            DateTime latestDate = DateTime.ParseExact(latestDateStr, "yyyyMMdd", null);
            DateTime today = DateTime.Now;
            if (today.Hour < 18) today = today.AddDays(-1);

            int days = (int)(today.Date - latestDate.Date).TotalDays;
            if (days <= 0)
            {
                EventManager.Ins.Emit("UpdateStatus", "数据已是最新！");
                EventManager.Ins.Emit("OnFetchFin");
                return;
            }

            _ = GetAllStocksLastNDaysAsync(days);
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }

    /// <summary>
    /// 并发获取 N 日数据（超快、不卡）
    /// </summary>
    public async Task GetAllStocksLastNDaysAsync(int days)
    {
        try
        {
            EventManager.Ins.Emit("UpdateStatus", $"获取 {days} 日数据...");
            EventManager.Ins.Emit("OnFetchStart");

            DateTime endDate = DateTime.Now;
            if (endDate.Hour < 18) endDate = endDate.AddDays(-1);

            List<DateTime> dateList = new List<DateTime>();
            for (int i = 0; i < days; i++)
            {
                DateTime d = endDate.AddDays(-i);
                if (d.DayOfWeek != DayOfWeek.Saturday && d.DayOfWeek != DayOfWeek.Sunday)
                    dateList.Add(d);
            }

            var allStockData = new List<StockDailyData>();

            // 单线程逐个处理每个日期
            for (int i = 0; i < dateList.Count; i++)
            {
                DateTime day = dateList[i];
                List<StockDailyData> data = await FetchDayData(day);
                allStockData.AddRange(data);
                
                // 更新状态
                int processedDays = i + 1;
                EventManager.Ins.Emit("UpdateStatus", $"已获取 {processedDays}/{dateList.Count} 天");
                
                // 让出线程，避免阻塞
                //await Task.Yield();
            }

            // 分组保存
            Dictionary<string, List<StockDailyData>> grouped = allStockData
                .GroupBy(d => d.TsCode)
                .ToDictionary(g => g.Key, g => g.OrderByDescending(x => x.TradeDate).ToList());

            // 并发保存文件
            List<Task> saveTasks = new List<Task>();
            foreach (var kvp in grouped)
            {
                string tsCode = kvp.Key;
                List<StockDailyData> stockData = kvp.Value;
                saveTasks.Add(Task.Run(() => SaveDailyStockData(tsCode, stockData)));
            }
            await Task.WhenAll(saveTasks);

            EventManager.Ins.Emit("UpdateStatus", $"✅ 获取完成！共 {allStockData.Count} 条");
            EventManager.Ins.Emit("OnFetchFin");
        }
        catch (Exception ex)
        {
            EventManager.Ins.Emit("UpdateStatus", $"❌ 失败：{ex.Message}");
            Debug.LogError(ex);
        }
    }

    /// <summary>
    /// 单日并发请求
    /// </summary>
    private async Task<List<StockDailyData>> FetchDayData(DateTime day)
    {
        try
        {
            string tradeDate = day.ToString("yyyyMMdd");
            // 串行执行，一个完成再开始另一个
            var dailyList = await Mgrs.Ins.tushareMgr.GetDailyStocksByDate(tradeDate);
            Debug.Log("GetDailyStocksByDate " + tradeDate);
            var basicDict = await Mgrs.Ins.tushareMgr.GetDailyBasicByDate(tradeDate);
            Debug.Log("GetDailyBasicByDate " + tradeDate);
            foreach (var d in dailyList)
            {
                if (basicDict.TryGetValue(d.TsCode, out var b))
                {
                    d.Pe = b.Pe;
                    d.Pe_ttm = b.Pe_ttm;
                    d.Total_mv = b.Total_mv;
                }
            }
            return dailyList;
        }
        catch
        {
            return new List<StockDailyData>();
        }
    }

    public string GetLatestStockDate()
    {
        try
        {
            string stockCode = "000001";
            string folder = GetDailyFolderPath(stockCode);
            var files = Directory.GetFiles(folder, "*.json").OrderByDescending(f => f).ToList();
            if (files.Count == 0) return "";
            string json = File.ReadAllText(files[0]);
            var data = JsonConvert.DeserializeObject<List<StockDailyData>>(json);
            return data.Max(d => d.TradeDate);
        }
        catch
        {
            return "";
        }
    }

    public (List<StockDailyData>, string) LoadNewestDailyStockDataFromFile(string tsCode)
    {
        try
        {
            string folder = GetDailyFolderPath(tsCode);
            var files = Directory.GetFiles(folder, "*.json").OrderByDescending(f => f).ToList();
            if (files.Count == 0) return (null, null);
            string json = File.ReadAllText(files[0]);
            var data = JsonConvert.DeserializeObject<List<StockDailyData>>(json);
            return (data, files[0]);
        }
        catch
        {
            return (null, null);
        }
    }

    public List<StockDailyData> LoadSingleDailyStockDataFromFile(string tsCode)
    {
        try
        {
            string folder = GetDailyFolderPath(tsCode);
            if (!Directory.Exists(folder)) return new List<StockDailyData>();
            var files = Directory.GetFiles(folder, "*.json").OrderByDescending(f => f).ToList();
            var all = new List<StockDailyData>();
            foreach (var f in files)
            {
                string json = File.ReadAllText(f);
                var d = JsonConvert.DeserializeObject<List<StockDailyData>>(json);
                all.AddRange(d);
            }
            return all;
        }
        catch
        {
            return new List<StockDailyData>();
        }
    }

    private (string Year, int Quarter) GetYearQuarter(string tradeDate)
    {
        int year = int.Parse(tradeDate.Substring(0, 4));
        int month = int.Parse(tradeDate.Substring(4, 2));
        int quarter = (month - 1) / 3 + 1;
        return (year.ToString(), quarter);
    }

    public void SaveDailyStockData(string tsCode, List<StockDailyData> dailyData)
    {
        string folder = GetDailyFolderPath(tsCode);
        if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
        var quarterly = new Dictionary<string, List<StockDailyData>>();

        foreach (var d in dailyData)
        {
            var (y, q) = GetYearQuarter(d.TradeDate);
            string k = $"{y}-{q}";
            if (!quarterly.ContainsKey(k)) quarterly[k] = new List<StockDailyData>();
            quarterly[k].Add(d);
        }

        foreach (var kv in quarterly)
        {
            var parts = kv.Key.Split('-');
            string path = GetQuarterlyFilePath(folder, parts[0], int.Parse(parts[1]));
            List<StockDailyData> existing = LoadQuarterlyDailyStockDataFromFile(path);
            var merged = MergeBaseDataOptimized(existing, kv.Value);
            string json = JsonConvert.SerializeObject(merged, Formatting.Indented);
            File.WriteAllText(path, json);
        }
    }

    private List<StockDailyData> LoadQuarterlyDailyStockDataFromFile(string filePath)
    {
        try
        {
            if (!File.Exists(filePath)) return new List<StockDailyData>();
            string json = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<List<StockDailyData>>(json) ?? new List<StockDailyData>();
        }
        catch
        {
            return new List<StockDailyData>();
        }
    }

    private List<T> MergeBaseDataOptimized<T>(List<T> existing, List<T> newData) where T : StockBaseData
    {
        if (existing.Count == 0) return newData;
        var merged = new List<T>();
        int i = 0, j = 0;

        while (i < existing.Count && j < newData.Count)
        {
            int cmp = string.Compare(existing[i].TradeDate, newData[j].TradeDate);
            if (cmp > 0) merged.Add(existing[i++]);
            else if (cmp < 0)
            {
                MacdCalculator.CalculateAndSetNewMacd(merged, newData[j]);
                merged.Add(newData[j++]);
            }
            else
            {
                MacdCalculator.CalculateAndSetNewMacd(merged, newData[j]);
                merged.Add(newData[j++]);
                i++;
            }
        }

        while (i < existing.Count) merged.Add(existing[i++]);
        while (j < newData.Count) merged.Add(newData[j++]);
        return merged;
    }

    [ContextMenu("Calculate and Update Average Data")]
    public void CalculateAndUpdateAverageData()
    {
        _ = CalculateAveragesAsync();
    }

    private async Task CalculateAveragesAsync()
    {
        try
        {
            var folders = Directory.GetDirectories(dailyFullPath);
            var tasks = folders.Select(f => Task.Run(() =>
            {
                try
                {
                    string tsCode = $"{Path.GetFileName(f)}.SZ";
                    var data = LoadSingleDailyStockDataFromFile(tsCode);
                    if (data.Count == 0) return;

                    data.Sort((a, b) => string.Compare(b.TradeDate, a.TradeDate)); // 降序排序，保持从新到旧的顺序
                    CalcAverages(data);

                    var quarterly = data.GroupBy(d =>
                    {
                        var (y, q) = GetYearQuarter(d.TradeDate);
                        return $"{y}-{q}";
                    });

                    foreach (var g in quarterly)
                    {
                        var parts = g.Key.Split('-');
                        string path = GetQuarterlyFilePath(f, parts[0], int.Parse(parts[1]));
                        string json = JsonConvert.SerializeObject(g.ToList(), Formatting.Indented);
                        File.WriteAllText(path, json);
                    }
                }
                catch { }
            })).ToList();

            await Task.WhenAll(tasks);
            Debug.Log("✅ 均线计算全部完成！");
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }

    private void CalcAverages(List<StockDailyData> data)
    {
        double sum5p = 0, sum5v = 0;
        double sum10p = 0, sum10v = 0;
        double sum20p = 0, sum20v = 0;

        for (int i = 0; i < data.Count; i++)
        {
            var c = data[i];
            sum5p += c.Close; sum5v += c.Vol;
            sum10p += c.Close; sum10v += c.Vol;
            sum20p += c.Close; sum20v += c.Vol;

            if (i >= 4)
            {
                c.FiveDayAvgPrice = sum5p / 5;
                c.FiveDayAvgVol = sum5v / 5;
                sum5p -= data[i - 4].Close;
                sum5v -= data[i - 4].Vol;
            }
            if (i >= 9)
            {
                c.TenDayAvgPrice = sum10p / 10;
                c.TenDayAvgVol = sum10v / 10;
                sum10p -= data[i - 9].Close;
                sum10v -= data[i - 9].Vol;
            }
            if (i >= 19)
            {
                c.TwentyDayAvgPrice = sum20p / 20;
                c.TwentyDayAvgVol = sum20v / 20;
                sum20p -= data[i - 19].Close;
                sum20v -= data[i - 19].Vol;
            }
        }
    }

    public void GetAllStocksChipPeakData()
    {
        Mgrs.Ins.chipMgr.GetAllStocksChipPeakData();
    }

    public override void Release() { }
    public string GetDailyDataDirectory() => dailyFullPath;
}