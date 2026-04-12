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

public class ChipMgr : MgrBase
{
    /// <summary>
    /// 筹码峰数据处理项
    /// </summary>
    private class ChipDataProcessingItem
    {
        public StockData Stock { get; set; }
        public string CurrentDate { get; set; }
        public List<(double Price, double Percent)> Chips { get; set; }
        public (List<StockDailyData>, string) NewestDataAndFile { get; set; }
    }

    // 用于跟踪筹码峰数据获取的状态
    private bool isGettingChipData = false;
    private int currentStockIndex = 0;
    private List<StockData> allStocksForChipData;
    private string currentDateForChipData;
    private ConcurrentQueue<ChipDataProcessingItem> chipProcessingQueue;
    private int totalStocksForChipData;
    private int processedStocksForChipData = 0;
    private object chipDataLockObject = new object();

    public override void Init()
    {
    }

    /// <summary>
    /// 获取所有股票当日的筹码峰数据
    /// </summary>
    public void GetAllStocksChipPeakData()
    {
        // 设置Unity项目为固定60帧
        Application.targetFrameRate = 60;
        
        EventManager.Ins.Emit("UpdateStatus", "获取所有股票当日筹码峰数据...");
        EventManager.Ins.Emit("OnFetchStart");

        // 获取000001股票的最新数据日期
        string currentDate = Mgrs.Ins.dailyDataMgr.GetLatestStockDate();
            
        // 获取所有已保存的股票代码
        List<StockData> allStocks = Mgrs.Ins.basicMgr.LoadAllBasicStockCodes();
            
        int total = allStocks.Count;
        processedStocksForChipData = 0;
        currentStockIndex = 0;
        isGettingChipData = true;
        currentDateForChipData = currentDate;
        allStocksForChipData = allStocks;
        totalStocksForChipData = total;
        
        // 创建队列来存储需要处理的数据
        chipProcessingQueue = new ConcurrentQueue<ChipDataProcessingItem>();
        
        // 启动处理线程
        HandleChipData(total);
    }

    /// <summary>
    /// 每帧发送一个请求
    /// </summary>
    public override void FixedUpdate()
    {
        // 条件：不获取中 / 索引越界 / 正在请求中 → 直接返回
        if (!isGettingChipData || currentStockIndex >= allStocksForChipData.Count)
        {
            return;
        }

        var stock = allStocksForChipData[currentStockIndex];
        var newestDataAndFile = Mgrs.Ins.dailyDataMgr.LoadNewestDailyStockDataFromFile(stock.TsCode);

        if (newestDataAndFile.Item1 == null || newestDataAndFile.Item2 == null)
        {
            lock (chipDataLockObject)
                processedStocksForChipData++;
            currentStockIndex++;
            return;
        }

        StockDailyData todayData = newestDataAndFile.Item1.FirstOrDefault(d => d.TradeDate == currentDateForChipData);
        if (todayData == null)
        {
            lock (chipDataLockObject)
                processedStocksForChipData++;
            currentStockIndex++;
            return;
        }

        // ==================== 核心修复 ====================
        // 异步发起请求，不等待！请求完成后自动回调
        _ = RequestChipAndEnqueue(stock, newestDataAndFile);
    }
    private async Task RequestChipAndEnqueue(StockData stock, (List<StockDailyData>, string) newestDataAndFile)
    {
        try
        {
            // 异步等待，不卡主线程
            var chips = await Mgrs.Ins.tushareMgr.GetCyqChips(stock.TsCode, currentDateForChipData);

            // 网络返回结果后 → 这里才入队（你要的回调时机）
            if (chips != null && chips.Count > 0)
            {
                chipProcessingQueue.Enqueue(new ChipDataProcessingItem
                {
                    Stock = stock,
                    CurrentDate = currentDateForChipData,
                    Chips = chips,
                    NewestDataAndFile = newestDataAndFile
                });
            }
            else
            {
                lock (chipDataLockObject) processedStocksForChipData++;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"处理股票 {stock.TsCode} 筹码数据失败: {e}");
            lock (chipDataLockObject) processedStocksForChipData++;
        }
        finally
        {
            // 无论成功失败，索引+1，解除请求标记
            currentStockIndex++;
            // 全部处理完成
            if (currentStockIndex >= allStocksForChipData.Count)
            {
                isGettingChipData = false;
            }
        }
    }
    /// <summary>
    /// 处理筹码数据
    /// </summary>
    public void HandleChipData(int total)
    {
        Task.Run(() =>
        {
            while (isGettingChipData || !chipProcessingQueue.IsEmpty)
            {
                if (chipProcessingQueue.TryDequeue(out var item))
                {
                    try
                    {
                        var (newestData, latestFile) = item.NewestDataAndFile;
                        
                        // 查找当日的数据记录
                        StockDailyData todayData = newestData.FirstOrDefault(d => d.TradeDate == item.CurrentDate);
                        if (todayData == null)
                        {
                            lock (chipDataLockObject)
                            {
                                processedStocksForChipData++;
                            }
                            continue;
                        }
                        
                        // 计算筹码集中度和主峰
                        var result = CalculateChipConcentration(item.Chips);
                        
                        // 更新筹码相关字段
                        todayData.Concentration70 = Math.Round(result.Concentration70, 2);
                        todayData.Concentration90 = Math.Round(result.Concentration90, 2);
                        todayData.ConcentrationPrice = Math.Round(result.ConcentrationPrice, 2);
                        
                        // 直接写回原路径
                        string json = JsonConvert.SerializeObject(newestData);
                        File.WriteAllText(latestFile, json, System.Text.Encoding.UTF8);
                    }
                    catch (Exception ex)
                    {
                        // 在主线程中记录错误
                        string errorMsg = $"Error processing chip data for stock {item.Stock.TsCode}: {ex.Message}";
                        Loom.Ins.QueueOnMainThread(() =>
                        {
                            Debug.LogError(errorMsg);
                        });
                    }
                    finally
                    {
                        lock (chipDataLockObject)
                        {
                            processedStocksForChipData++;
                            if (processedStocksForChipData % 100 == 0)
                            {
                                // 在主线程中更新UI
                                int currentProcessed = processedStocksForChipData;
                                Loom.Ins.QueueOnMainThread(() =>
                                {
                                    EventManager.Ins.Emit("UpdateStatus", $"获取筹码峰数据中... {currentProcessed}/{total}");
                                });
                            }
                            
                            // 检查是否完成所有处理
                            if (processedStocksForChipData >= total)
                            {
                                Loom.Ins.QueueOnMainThread(() =>
                                {
                                    EventManager.Ins.Emit("UpdateStatus", "所有股票筹码峰数据获取完成！");
                                    EventManager.Ins.Emit("OnFetchFin");
                                });
                            }
                        }
                    }
                }
                else
                {
                    Thread.Sleep(10); // 避免忙等
                }
            }
        });
    }

    /// <summary>
    /// 同花顺同款算法：计算 70% / 90% 筹码集中度 & 主峰价格
    /// </summary>
    /// <param name="chips">筹码分布数据 (价格, 百分比)</param>
    /// <returns>筹码集中度和主峰价格</returns>
    public (double Concentration70, double Concentration90, double ConcentrationPrice) CalculateChipConcentration(List<(double Price, double Percent)> chips)
    {
        if (chips == null || chips.Count == 0)
        {
            return (0, 0, 0);
        }

        // 按价格排序（从低到高）
        var sortedChips = chips.OrderBy(c => c.Price).ToList();

        // 计算累计百分比
        double totalPercent = 0;
        var cumulativeChips = new List<(double Price, double CumulativePercent)>();
        foreach (var chip in sortedChips)
        {
            totalPercent += chip.Percent;
            cumulativeChips.Add((chip.Price, totalPercent));
        }

        // 计算 70% 筹码集中度
        double concentration70 = CalculateConcentration(cumulativeChips, 70);

        // 计算 90% 筹码集中度
        double concentration90 = CalculateConcentration(cumulativeChips, 90);

        // 计算主峰价格（找到百分比最高的价格）
        double concentrationPrice = chips.OrderByDescending(c => c.Percent).First().Price;

        return (concentration70, concentration90, concentrationPrice);
    }

    /// <summary>
    /// 计算指定百分比的筹码集中度
    /// </summary>
    /// <param name="cumulativeChips">累计百分比筹码数据</param>
    /// <param name="percent">目标百分比</param>
    /// <returns>筹码集中度</returns>
    private double CalculateConcentration(List<(double Price, double CumulativePercent)> cumulativeChips, double percent)
    {
        double targetPercent = percent / 100;

        // 找到累计百分比达到 50% - targetPercent/2 的价格
        double lowerBound = FindPriceAtPercent(cumulativeChips, 0.5 - targetPercent / 2);

        // 找到累计百分比达到 50% + targetPercent/2 的价格
        double upperBound = FindPriceAtPercent(cumulativeChips, 0.5 + targetPercent / 2);

        // 计算集中度
        if (upperBound > lowerBound)
        {
            return (upperBound - lowerBound) / ((upperBound + lowerBound) / 2) * 100;
        }

        return 0;
    }

    /// <summary>
    /// 找到指定累计百分比对应的价格
    /// </summary>
    /// <param name="cumulativeChips">累计百分比筹码数据</param>
    /// <param name="targetPercent">目标累计百分比</param>
    /// <returns>对应的价格</returns>
    private double FindPriceAtPercent(List<(double Price, double CumulativePercent)> cumulativeChips, double targetPercent)
    {
        for (int i = 0; i < cumulativeChips.Count; i++)
        {
            if (cumulativeChips[i].CumulativePercent >= targetPercent)
            {
                // 线性插值
                if (i == 0)
                {
                    return cumulativeChips[i].Price;
                }
                else
                {
                    var prev = cumulativeChips[i - 1];
                    var curr = cumulativeChips[i];
                    double ratio = (targetPercent - prev.CumulativePercent) / (curr.CumulativePercent - prev.CumulativePercent);
                    return prev.Price + (curr.Price - prev.Price) * ratio;
                }
            }
        }

        return cumulativeChips.Last().Price;
    }
    public override void Release()
    {

    }
}
