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

        try
        {
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

                // 4. 市值 > 20 亿
                if (latest.Total_mv <= 20) continue;

                targetStocks.Add(stock);
            }

            _total = targetStocks.Count;
            _done = 0;
            _queue = new ConcurrentQueue<ChipProcessItem>();

            EventManager.Ins.Emit("UpdateStatus", $"符合条件股票：{_total} 只，开始获取筹码...");

            // 3. 并发获取筹码
            var tasks = targetStocks.Select(s => FetchSingleChipAsync(s, todayDate));
            await Task.WhenAll(tasks);

            // 4. 后台批量写入
            StartSaveTask();

            // 5. 等待全部完成
            await WaitFinish();
        }
        catch (Exception ex)
        {
            Debug.LogError($"筹码异常：{ex}");
            EventManager.Ins.Emit("UpdateStatus", $"失败：{ex.Message}");
        }
        finally
        {
            _isRunning = false;
            EventManager.Ins.Emit("OnFetchFin");
        }
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
                }

                Interlocked.Increment(ref _done);
                UpdateProgress();
                return;
            }
            catch
            {
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
                        if (today == null || item.Chips == null) continue;

                        // 计算集中度
                        var (c70, c90, peak) = CalculateChip(item.Chips);
                        today.Concentration70 = Math.Round(c70, 2);
                        today.Concentration90 = Math.Round(c90, 2);
                        today.ConcentrationPrice = Math.Round(peak, 2);

                        string json = JsonConvert.SerializeObject(data, Formatting.Indented);
                        await File.WriteAllTextAsync(path, json, System.Text.Encoding.UTF8);
                    }
                    catch { }
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
        var sorted = chips.OrderBy(x => x.p).ToList();
        var cum = new List<(double p, double sum)>();
        double total = 0;
        foreach (var c in sorted)
        {
            total += c.per;
            cum.Add((c.p, total));
        }
        double c70 = Calc(cum, 70);
        double c90 = Calc(cum, 90);
        double peak = chips.OrderByDescending(x => x.per).First().p;
        return (c70, c90, peak);
    }

    private double Calc(List<(double p, double sum)> cum, double pct)
    {
        double t = pct / 100d;
        double low = Find(cum, 0.5 - t / 2);
        double high = Find(cum, 0.5 + t / 2);
        return high > low ? (high - low) / ((high + low) / 2) * 100 : 0;
    }

    private double Find(List<(double p, double sum)> cum, double target)
    {
        for (int i = 0; i < cum.Count; i++)
        {
            if (cum[i].sum >= target)
            {
                if (i == 0) return cum[i].p;
                var a = cum[i - 1];
                var b = cum[i];
                double r = (target - a.sum) / (b.sum - a.sum);
                return a.p + (b.p - a.p) * r;
            }
        }
        return cum.Last().p;
    }

    public override void Release() { }
}