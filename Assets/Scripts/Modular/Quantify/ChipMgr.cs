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
    private class ChipProcessItem
    {
        public StockData Stock { get; set; }
        public string Date { get; set; }
        public List<(double Price, double Percent)> Chips { get; set; }
        public (List<StockDailyData>, string) DataAndFile { get; set; }
    }

    // 并发限流（根据你的积分调整：免费8，中级12，高级20）
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(2); // 降低并发数
    private const int MAX_RETRY = 3;
    private const int DELAY_MS = 2000; // 增加重试延迟
    private const int REQUEST_DELAY = 500; // 每次请求后延迟

    private bool _isRunning;
    private ConcurrentQueue<ChipProcessItem> _queue;
    private int _total;
    private int _done;
    private readonly object _lock = new();

    public override void Init() { }

    /// <summary>
    /// 🔥 新版：只拉主板 + 市值>20亿 + 排除双创+北交所
    /// </summary>
    public async void GetAllStocksChipPeakData()
    {
        if (_isRunning) return;
        _isRunning = true;

        //try
        //{
            EventManager.Ins.Emit("UpdateStatus", "开始筛选主板股票（市值>20亿）...");
            EventManager.Ins.Emit("OnFetchStart");

            // 1. 获取最新日期
            string todayDate = Mgrs.Ins.dailyDataMgr.GetLatestStockDate();
            if (string.IsNullOrEmpty(todayDate))
            {
                EventManager.Ins.Emit("UpdateStatus", "请先获取日线数据！");
                EventManager.Ins.Emit("OnFetchFin");
                _isRunning = false;
                return;
            }

            // 2. 全量股票 + 🔥 超级过滤
            List<StockData> allStocks = Mgrs.Ins.basicMgr.LoadAllBasicStockCodes();
            var targetStocks = new List<StockData>();

            foreach (var stock in allStocks)
            {
                // ==================== 你的核心过滤规则 ====================
                // 1. 排除创业板 300/301
                if (stock.TsCode.StartsWith("300") || stock.TsCode.StartsWith("301")) continue;
                // 2. 排除科创板 688/689
                if (stock.TsCode.StartsWith("688") || stock.TsCode.StartsWith("689")) continue;
                // 3. 排除北交所 80/43/42
                if (stock.TsCode.StartsWith("8") || stock.TsCode.StartsWith("43") || stock.TsCode.StartsWith("42")) continue;
                // ==========================================================

                // 加载日线
                var (dayData, _) = Mgrs.Ins.dailyDataMgr.LoadNewestDailyStockDataFromFile(stock.TsCode);
                var latest = dayData?.FirstOrDefault(d => d.TradeDate == todayDate);
                if (latest == null) continue;

                // 4. 市值 > 200 亿，单位万
                if (latest.Total_mv <= 200000) continue;

                targetStocks.Add(stock);
            }

            _total = targetStocks.Count;
            _done = 0;
            _queue = new ConcurrentQueue<ChipProcessItem>();

            EventManager.Ins.Emit("UpdateStatus", $"符合条件股票：{_total} 只，开始获取筹码...");

            // 3. 并发获取筹码
            var tasks = targetStocks.Select(s => FetchSingleChipAsync(s, todayDate));
            await Task.WhenAll(tasks);
            Debug.Log(_queue.Count); 
            // 遍历队列，打印股票代码和筹码数据
            var tempQueue = new ConcurrentQueue<ChipProcessItem>();
            
            // 4. 后台批量写入
            StartSaveTask();

            // 5. 等待全部完成
            await WaitFinish();
        //}
        //catch (Exception ex)
        //{
        //    Debug.LogError($"筹码异常：{ex}");
        //    EventManager.Ins.Emit("UpdateStatus", $"失败：{ex.Message}");
        //}
        //finally
        //{
            _isRunning = false;
            EventManager.Ins.Emit("OnFetchFin");
        //}
    }

    // 限流 + 重试
    private async Task FetchSingleChipAsync(StockData stock, string date)
    {
        int retry = 0;
        while (retry < MAX_RETRY)
        {
            try
            {
                await _semaphore.WaitAsync();

                // 本地已有筹码 → 跳过
                var (data, file) = Mgrs.Ins.dailyDataMgr.LoadNewestDailyStockDataFromFile(stock.TsCode);
                var today = data?.FirstOrDefault(d => d.TradeDate == date);
                if (today != null && today.Concentration90 > 0)
                {
                    Loom.Ins.QueueOnMainThread(() =>
                    {
                        Debug.Log($"跳过 {stock.TsCode}：已有筹码数据");
                    });
                    Interlocked.Increment(ref _done);
                    UpdateProgress();
                    return;
                }

                // 请求接口
                var chips = await Mgrs.Ins.tushareMgr.GetCyqChips(stock.TsCode, date);
                
                // 添加请求延迟，避免API限制
                await Task.Delay(REQUEST_DELAY);
                
                if (chips != null && chips.Count > 0)
                {
                    _queue.Enqueue(new ChipProcessItem
                    {
                        Stock = stock,
                        Date = date,
                        Chips = chips,
                        DataAndFile = (data, file)
                    });
                }
                else
                {
                    Loom.Ins.QueueOnMainThread(() =>
                    {
                        Debug.Log($"跳过 {stock.TsCode}：筹码数据为空，数量：{chips?.Count}");
                    });
                }

                Interlocked.Increment(ref _done);
                UpdateProgress();
                return;
            }
            catch (Exception ex)
            {
                Loom.Ins.QueueOnMainThread(() =>
                {
                    Debug.LogError($"{stock.TsCode} 获取筹码失败：{ex.Message}");
                });
                retry++;
                await Task.Delay(DELAY_MS * retry);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        Interlocked.Increment(ref _done);
        UpdateProgress();
    }

    // 后台写文件（不卡主线程）
    private void StartSaveTask()
    {
        Task.Run(async () =>
        {
            while (_done < _total || !_queue.IsEmpty)
            {
                if (_queue.TryDequeue(out var item))
                {
                    try
                    {
                        var (data, path) = item.DataAndFile;
                        var today = data?.FirstOrDefault(d => d.TradeDate == item.Date);
                        if (today == null || item.Chips == null)
                        {
                            Loom.Ins.QueueOnMainThread(() =>
                            {
                                Debug.Log($"跳过：today={today}, Chips={item.Chips?.Count}, Date={item.Date}");
                            });
                            continue;
                        }
                        
                        // 只打印 601857.SH 的筹码数据
                        if (item.Stock.TsCode == "601857.SH")
                        {
                            string chipsStr = string.Join(", ", item.Chips.Select(c => $"{c.Price}:{c.Percent}"));
                            Loom.Ins.QueueOnMainThread(() =>
                            {
                                Debug.Log($"{item.Stock.TsCode} 筹码：{chipsStr}");
                            });
                        }
                        
                        var (c70, c90, peak) = CalculateChip(item.Chips);
                        Loom.Ins.QueueOnMainThread(() =>
                        {
                            Debug.Log($"计算结果：{item.Stock.TsCode}，c70={c70}，c90={c90}，peak={peak}");
                        });
                        
                        // 更新数据
                        today.Concentration70 = Math.Round(c70, 2);
                        today.Concentration90 = Math.Round(c90, 2);
                        today.ConcentrationPrice = Math.Round(peak, 2);
                        Loom.Ins.QueueOnMainThread(() =>
                        {
                            Debug.Log($"更新后：Concentration70={today.Concentration70}，Concentration90={today.Concentration90}，ConcentrationPrice={today.ConcentrationPrice}");
                        });

                        // 写入文件
                        string json = JsonConvert.SerializeObject(data, Formatting.Indented);
                        await File.WriteAllTextAsync(path, json, System.Text.Encoding.UTF8);
                        Loom.Ins.QueueOnMainThread(() =>
                        {
                            Debug.Log($"写入文件：{path}");
                        });
                    }
                    catch (Exception ex)
                    {
                        Loom.Ins.QueueOnMainThread(() =>
                        {
                            Debug.LogError($"保存失败：{ex.Message}");
                        });
                    }
                }
                else await Task.Delay(50);
            }
        });
    }

    // 等待完成
    private async Task WaitFinish()
    {
        while (_done < _total || !_queue.IsEmpty)
            await Task.Delay(200);

        EventManager.Ins.Emit("UpdateStatus", $"✅ 筹码全部完成！共 {_total} 只主板股票");
    }

    // 进度条
    private void UpdateProgress()
    {
        int curr = _done;
        Loom.Ins.QueueOnMainThread(() =>
        {
            EventManager.Ins.Emit("UpdateStatus",
                $"筹码获取中：{curr}/{_total} ({curr * 100f / _total:F0}%)");
        });
    }

    // ==================== 筹码计算（修复版） ====================
    public (double c70, double c90, double peakPrice) CalculateChip(List<(double p, double per)> chips)
    {
        if (chips == null || chips.Count == 0) return (0, 0, 0);

        // 1. 按价格从小到大排序
        var sorted = chips.OrderBy(x => x.p).ToList();

        // 2. 计算累积分布（不归一化，直接用原始百分比）
        List<(double p, double sum)> cum = new List<(double p, double sum)>();
        double current = 0;
        foreach (var item in sorted)
        {
            current += item.per;
            cum.Add((item.p, current));
        }

        double totalSum = cum.Last().sum;
        Loom.Ins.QueueOnMainThread(() =>
        {
            Debug.Log($"总筹码占比: {totalSum:F2}%");
        });

        // 3. 检查总筹码是否足够
        double c70 = 0;
        double c90 = 0;
        if (totalSum >= 70)
        {
            c70 = CalcMinRangeConcentration(sorted, cum, 70);
            if (totalSum >= 90)
            {
                c90 = CalcMinRangeConcentration(sorted, cum, 90);
            }
            else
            {
                Loom.Ins.QueueOnMainThread(() =>
                {
                    Debug.LogWarning($"总筹码 {totalSum:F2}% 不足90%，无法计算C90！");
                });
            }
        }
        else
        {
            Loom.Ins.QueueOnMainThread(() =>
            {
                Debug.LogError($"总筹码 {totalSum:F2}% 不足70%，无法计算C70和C90！");
            });
        }

        // 4. 筹码峰
        double peak = chips.OrderByDescending(x => x.per).First().p;

        return (c70, c90, peak);
    }

    /// <summary>
    /// 计算包含 targetPct% 筹码的最小区间集中度（修复版）
    /// </summary>
    private double CalcMinRangeConcentration(List<(double p, double per)> sorted, List<(double p, double sum)> cum, double targetPct)
    {
        double minWidth = double.MaxValue;
        double bestLow = 0;
        double bestHigh = 0;

        // 滑动窗口找最小区间
        for (int i = 0; i < cum.Count; i++)
        {
            double startSum = i == 0 ? 0 : cum[i - 1].sum;
            double target = startSum + targetPct;

            // 找刚好超过 target 的位置（修复版）
            int j = FindIndex(cum, target);
            if (j == -1) continue; // 如果没找到，跳过当前窗口

            // 区间宽度
            double width = cum[j].p - cum[i].p;
            if (width < minWidth)
            {
                minWidth = width;
                bestLow = cum[i].p;
                bestHigh = cum[j].p;
            }
        }

        if (bestHigh <= bestLow)
        {
            Loom.Ins.QueueOnMainThread(() =>
            {
                Debug.LogError($"无法找到有效的{targetPct}%筹码区间！");
            });
            return 0;
        }

        double concentration = (bestHigh - bestLow) / (bestHigh + bestLow) * 100;
        Loom.Ins.QueueOnMainThread(() =>
        {
            Debug.Log($"{targetPct}%集中度: {concentration:F2}%, 区间: {bestLow:F2} ~ {bestHigh:F2}");
        });
        return concentration;
    }

    /// <summary>
    /// 找到第一个累积和 >= target 的索引（修复版）
    /// </summary>
    private int FindIndex(List<(double p, double sum)> cum, double target)
    {
        for (int i = 0; i < cum.Count; i++)
        {
            if (cum[i].sum >= target)
                return i;
        }
        // 如果target超过总和，返回-1表示未找到
        return -1;
    }

    public override void Release() { }
}