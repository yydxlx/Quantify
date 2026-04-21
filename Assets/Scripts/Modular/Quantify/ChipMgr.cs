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
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(8);
    private const int MAX_RETRY = 3;
    private const int DELAY_MS = 1500;

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
                if (latest.Total_mv <= 20000000) continue;

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
        while (_queue.TryDequeue(out var item))
        {
                
            //if (item.Stock.TsCode == "000001.SH" || item.Stock.TsCode == "000001")
            //{
                Debug.Log(item.Stock.TsCode);
                foreach (var c in item.Chips)
                {
                    Debug.Log($"筹码：{c.Price}，{c.Percent}");
                }
            //}
                
            tempQueue.Enqueue(item);
        }
        // 将项目放回原始队列
        while (tempQueue.TryDequeue(out var item))
            {
                _queue.Enqueue(item);
            }
            
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
                if (chips != null && chips.Count > 0)
                {
                    _queue.Enqueue(new ChipProcessItem
                    {
                        Stock = stock,
                        Date = date,
                        Chips = chips,
                        DataAndFile = (data, file)
                    });
                    Loom.Ins.QueueOnMainThread(() =>
                    {
                        Debug.Log($"添加 {stock.TsCode} 到队列，筹码数量：{chips.Count}");
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

                        // 计算集中度
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

    // ==================== 筹码计算 ====================
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

        // 3. 计算标准集中度
        double c70 = CalcMinRangeConcentration(sorted, cum, 70);
        double c90 = CalcMinRangeConcentration(sorted, cum, 90);

        // 4. 筹码峰
        double peak = chips.OrderByDescending(x => x.per).First().p;

        return (c70, c90, peak);
    }

    /// <summary>
    /// 计算包含 targetPct% 筹码的最小区间集中度（同花顺/通达信算法）
    /// </summary>
    private double CalcMinRangeConcentration(List<(double p, double per)> sorted, List<(double p, double sum)> cum, double targetPct)
    {
        double minWidth = double.MaxValue;
        double bestLow = 0;
        double bestHigh = 0;

        // 滑动窗口找最小区间
        for (int i = 0; i < cum.Count; i++)
        {
            // 当前窗口起点的累积值
            double startSum = i == 0 ? 0 : cum[i - 1].sum;
            double target = startSum + targetPct;

            // 找刚好超过 target 的位置
            int j = FindIndex(cum, target);
            if (j >= cum.Count) break;

            // 区间宽度
            double width = cum[j].p - cum[i].p;
            if (width < minWidth)
            {
                minWidth = width;
                bestLow = cum[i].p;
                bestHigh = cum[j].p;
            }
        }

        if (bestHigh <= bestLow) return 0;
        return (bestHigh - bestLow) / (bestHigh + bestLow) * 100;
    }

    /// <summary>
    /// 找到第一个累积和 >= target 的索引
    /// </summary>
    private int FindIndex(List<(double p, double sum)> cum, double target)
    {
        for (int i = 0; i < cum.Count; i++)
        {
            if (cum[i].sum >= target)
                return i;
        }
        return cum.Count - 1;
    }

    public override void Release() { }
}