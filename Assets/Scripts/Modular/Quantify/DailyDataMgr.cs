using ClientBase;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class DailyDataMgr : MgrBase
{
    private string DailyDataDirectory = "AllDailyData";
    private string dailyFullPath;

    public override void Init()
    {
        // 计算Assets同级目录的AllData路径
        string allDataPath = Path.Combine(Path.GetDirectoryName(Application.dataPath), "AllData");
        dailyFullPath = Path.Combine(allDataPath, DailyDataDirectory);
        if (!Directory.Exists(dailyFullPath))
            Directory.CreateDirectory(dailyFullPath);
    }

    private string GetDailyFolderPath(string tsCode)
    {
        // 提取股票代码的数字部分，如000001.SZ -> 000001
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

    // 递归获取所有股票的每日数据
    public void UpdateAllDailyStocksRecursive()
    {
        // 查找所有股票文件夹
        var stockFolders = Directory.GetDirectories(dailyFullPath);
        // 检查是否有股票文件夹
        if (stockFolders.Length == 0)
        {
            Debug.LogError("该路径下没有找到股票文件夹");
            return;
        }
        // 获取第一个股票文件夹
        string firstStockFolder = stockFolders.First();
        // 从文件夹名称获取股票代码
        string stockId = Path.GetFileName(firstStockFolder);
        // 构造完整的股票代码（假设是SZ交易所）
        string tsCode = $"{stockId}.SZ";
        // 加载该股票的所有数据
        List<StockDailyData> stockDailyDatas = LoadSingleDailyStockDataFromFile(tsCode);
        string latestDateStr = stockDailyDatas[0].TradeDate;
        Debug.Log(latestDateStr);
        DateTime latestDate = DateTime.ParseExact(latestDateStr, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None);
        DateTime today = DateTime.Now;
        TimeSpan difference = today - latestDate;
        Debug.Log((int)difference.TotalDays);
        GetAllStocksLastNDaysRecursive((int)difference.TotalDays);
    }

    // 递归获取所有股票指定天数的每日数据
    public async void GetAllStocksLastNDaysRecursive(int days = 30)
    {
        EventManager.Ins.Emit("UpdateStatus", $"递归获取所有股票最近{days}日数据...");
        EventManager.Ins.Emit("OnFetchStart");
        DateTime endDate = DateTime.Now;
        List<StockDailyData> allStockData = new List<StockDailyData>();
        if (DateTime.Now.Hour < 18)
        {
            endDate = endDate.AddDays(-1);
        }
        await GetStocksByDayRecursive(endDate, days, allStockData);
        // 全部获取完成后，按股票代码分组并保存，每日数据按日期倒序排列
        Dictionary<string, List<StockDailyData>> grouped = allStockData.GroupBy(d => d.TsCode)
                                  .ToDictionary(g => g.Key, g => g.OrderByDescending(x => x.TradeDate).ToList());
        foreach (KeyValuePair<string, List<StockDailyData>> stockData in grouped)
        {
            SaveDailyStockData(stockData.Key, stockData.Value);
        }
        EventManager.Ins.Emit("UpdateStatus", $"获取完成");
        EventManager.Ins.Emit("OnFetchFin");
    }

    // 递归获取每日数据
    private async Task GetStocksByDayRecursive(DateTime date, int remainingDays, List<StockDailyData> allStockData)
    {
        if (remainingDays <= 0) return;
        string tradeDate = date.ToString("yyyyMMdd");
        // 获取 daily 数据
        List<StockDailyData> dailyList = await Mgrs.Ins.tushareMgr.GetDailyStocksByDate(tradeDate);
        // 获取 daily_basic 数据
        Dictionary<string, StockDailyData> basicDict = await Mgrs.Ins.tushareMgr.GetDailyBasicByDate(tradeDate);
        // 合并 daily 和 daily_basic 数据
        foreach (var daily in dailyList)
        {
            if (basicDict.TryGetValue(daily.TsCode, out var basic))
            {
                daily.Pe = basic.Pe;
                daily.Pe_ttm = basic.Pe_ttm;
                daily.Total_mv = basic.Total_mv;
            }
            allStockData.Add(daily);
        }
        // 递归获取前一天
        // await GetStocksByDayRecursive(date.AddDays(-1), remainingDays - 1, allStockData);
    }

    // 获取000001股票的最新数据日期
    public string GetLatestStockDate()
    {
        string stockCode = "000001"; // 使用000001股票作为参考
        string stockFolderPath = GetDailyFolderPath(stockCode);       
        // 获取所有季度文件，按文件名降序排序
        var quarterlyFiles = Directory.GetFiles(stockFolderPath, "*.json")
            .OrderByDescending(f => Path.GetFileName(f))
            .ToList();
        if (quarterlyFiles.Count == 0)
        {
            return "";
        }
        string latestFile = quarterlyFiles[0];
        string json = File.ReadAllText(latestFile, System.Text.Encoding.UTF8);
        List<StockDailyData> data = JsonConvert.DeserializeObject<List<StockDailyData>>(json);
        return data.Max(d => d.TradeDate);
    }

    // 读取最新的每日数据文件
    public (List<StockDailyData>, string) LoadNewestDailyStockDataFromFile(string tsCode)
    {
        string stockFolderPath = GetDailyFolderPath(tsCode);        
        // 获取所有季度文件，按文件名降序排序
        var quarterlyFiles = Directory.GetFiles(stockFolderPath, "*.json")
            .OrderByDescending(f => Path.GetFileName(f))
            .ToList();
        // 读取最新的季度文件
        string latestFile = quarterlyFiles[0];
        string json = File.ReadAllText(latestFile, System.Text.Encoding.UTF8);
        List<StockDailyData> data = JsonConvert.DeserializeObject<List<StockDailyData>>(json);
        return (data, latestFile);
    }

    // 读取每日数据（AllDailyData）
    public List<StockDailyData> LoadSingleDailyStockDataFromFile(string tsCode)
    {
        // 获取股票文件夹路径
        string stockFolderPath = GetDailyFolderPath(tsCode);
        
        if (!Directory.Exists(stockFolderPath))
            return new List<StockDailyData>();
        
        // 获取所有季度文件，按文件名排序（年-季度）
        List<string> quarterlyFiles = Directory.GetFiles(stockFolderPath, "*.json")
            .OrderByDescending(f => Path.GetFileName(f)) // 按文件名降序排序，最新的季度在前
            .ToList();
        List<StockDailyData> allData = new List<StockDailyData>();
        // 按顺序读取每个季度文件
        for (int i = 0; i < quarterlyFiles.Count; i++)
        {
            string file = quarterlyFiles[i];
            string json = File.ReadAllText(file, System.Text.Encoding.UTF8);
            List<StockDailyData> quarterlyData = JsonConvert.DeserializeObject<List<StockDailyData>>(json);
            allData.AddRange(quarterlyData);
        }
        return allData;
    }

    // 根据日期获取年和季度
    private (string Year, int Quarter) GetYearQuarter(string tradeDate)
    {
        // 假设tradeDate格式为yyyyMMdd
        int year = int.Parse(tradeDate.Substring(0, 4));
        int month = int.Parse(tradeDate.Substring(4, 2));
        int quarter = (month - 1) / 3 + 1;
        return (year.ToString(), quarter);
    }

    // 保存daily的json数据
    public void SaveDailyStockData(string tsCode, List<StockDailyData> dailyData)
    {
        // 获取股票文件夹路径
        string stockFolderPath = GetDailyFolderPath(tsCode);
        // 按季度分组数据
        var quarterlyData = new Dictionary<string, List<StockDailyData>>();
        foreach (StockDailyData data in dailyData)
        {
            var (year, quarter) = GetYearQuarter(data.TradeDate);
            string key = $"{year}-{quarter}";
            if (!quarterlyData.ContainsKey(key))
                quarterlyData[key] = new List<StockDailyData>();
            quarterlyData[key].Add(data);
        }
        
        // 处理每个季度的数据
        foreach (var kvp in quarterlyData)
        {
            string[] parts = kvp.Key.Split('-');
            string year = parts[0];
            int quarter = int.Parse(parts[1]);
            // 获取季度文件路径
            string quarterlyFilePath = GetQuarterlyFilePath(stockFolderPath, year, quarter);
            // 加载现有数据
            List<StockDailyData> existingData = LoadSingleDailyStockDataFromFile(tsCode);
            // 合并数据（增量插入）
            List<StockDailyData> mergedData = MergeBaseDataOptimized(existingData, kvp.Value);
            // 保存数据
            string json = JsonConvert.SerializeObject(mergedData);
            File.WriteAllText(quarterlyFilePath, json, System.Text.Encoding.UTF8);
        }
    }

    private List<T> MergeBaseDataOptimized<T>(List<T> existingData, List<T> newData) where T : StockBaseData
    {
        if (existingData.Count == 0)
            return newData;
        
        // 确保existingData按日期倒序排序，最新的日期在最上面
        //existingData.Sort((a, b) => string.Compare(b.TradeDate, a.TradeDate));
        
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

    // 计算并更新平均值数据
    [ContextMenu("Calculate and Update Average Data")]
    public void CalculateAndUpdateAverageData()
    {
        // Get all daily data files
        string[] dailyFiles = Directory.GetFiles(dailyFullPath, "*.json", SearchOption.TopDirectoryOnly);
        
        if (dailyFiles.Length == 0)
        {
            Debug.LogError("No daily data files found in " + dailyFullPath);
            return;
        }
        
        Debug.Log("Starting to calculate and update average data for " + dailyFiles.Length + " stock files...");
        
        foreach (string filePath in dailyFiles)
        {
            // Load daily data
            List<StockDailyData> dailyDataList = LoadSingleDailyStockDataFromFile(filePath);
            
            if (dailyDataList.Count == 0)
            {
                Debug.LogWarning("Empty data in file: " + filePath);
                continue;
            }
            
            // Sort data by date (oldest first) to calculate averages correctly
            dailyDataList.Sort((a, b) => string.Compare(a.TradeDate, b.TradeDate));
            
            // Calculate average values for each day using sliding window algorithm
            double sumPrice5 = 0, sumVol5 = 0;
            double sumPrice10 = 0, sumVol10 = 0;
            double sumPrice20 = 0, sumVol20 = 0;
            
            for (int i = 0; i < dailyDataList.Count; i++)
            {
                StockDailyData currentData = dailyDataList[i];
                
                // Add current day's data to sums
                sumPrice5 += currentData.Close;
                sumVol5 += currentData.Vol;
                
                sumPrice10 += currentData.Close;
                sumVol10 += currentData.Vol;
                
                sumPrice20 += currentData.Close;
                sumVol20 += currentData.Vol;
                
                // Calculate 5-day averages
                if (i >= 4) // Need at least 5 days of data
                {
                    currentData.FiveDayAvgPrice = sumPrice5 / 5;
                    currentData.FiveDayAvgVol = sumVol5 / 5;
                    
                    // Subtract the value from 5 days ago for next iteration
                    if (i >= 4)
                    {
                        sumPrice5 -= dailyDataList[i - 4].Close;
                        sumVol5 -= dailyDataList[i - 4].Vol;
                    }
                }
                
                // Calculate 10-day averages
                if (i >= 9) // Need at least 10 days of data
                {
                    currentData.TenDayAvgPrice = sumPrice10 / 10;
                    currentData.TenDayAvgVol = sumVol10 / 10;
                    
                    // Subtract the value from 10 days ago for next iteration
                    if (i >= 9)
                    {
                        sumPrice10 -= dailyDataList[i - 9].Close;
                        sumVol10 -= dailyDataList[i - 9].Vol;
                    }
                }
                
                // Calculate 20-day averages
                if (i >= 19) // Need at least 20 days of data
                {
                    currentData.TwentyDayAvgPrice = sumPrice20 / 20;
                    currentData.TwentyDayAvgVol = sumVol20 / 20;
                    
                    // Subtract the value from 20 days ago for next iteration
                    if (i >= 19)
                    {
                        sumPrice20 -= dailyDataList[i - 19].Close;
                        sumVol20 -= dailyDataList[i - 19].Vol;
                    }
                }
            }
            
            // Save the updated data back to the file
            string json = JsonConvert.SerializeObject(dailyDataList);
            File.WriteAllText(filePath, json, System.Text.Encoding.UTF8);
            
            Debug.Log("Updated average data for: " + Path.GetFileName(filePath));
        }
        
        Debug.Log("Average data calculation and update completed!");
    }

    // 获取所有股票当日的筹码峰数据 并且同时开启多线程处理
    public void GetAllStocksChipPeakData()
    {
        // 调用ChipMgr中的方法
        Mgrs.Ins.chipMgr.GetAllStocksChipPeakData();
    }

    public override void Release()
    {

    }

    // 获取每日数据目录路径
    public string GetDailyDataDirectory()
    {
        return dailyFullPath;
    }
}