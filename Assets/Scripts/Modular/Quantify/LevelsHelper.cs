using ClientBase;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;

public class LevelsHelper
{
    private const double RangeThreshold = 0.03; // 价格波动幅度阈值（3%）
    private const int ConsolidationDays = 5;   // 横盘最少天数

    /// <summary>
    /// 计算最近30天的压力位和支撑位
    /// </summary>
    /// <param name="stockDataList">完整的股票日线数据（已按日期从新到旧排序）</param>
    /// <param name="resistance1">横盘压力位1</param>
    /// <param name="resistance2">冲高回落压力位2</param>
    /// <param name="support1">横盘支撑位1</param>
    /// <param name="support2">低点连线支撑位2</param>
    public static void CalculateLevels(List<StockBaseData> stockDataList,out double resistance1,out double resistance2,out double support1,out double support2)
    {
        // 初始化默认值
        resistance1 = -1;
        resistance2 = -1;
        support1 = -1;
        support2 = -1;

        if (stockDataList == null || stockDataList.Count == 0)
            return;

        // 获取最近30天数据（从新到旧，下标0-29）
        var recentData = stockDataList.Take(30).ToList();
        int dataCount = recentData.Count;
        if (dataCount < 10) // 数据量不足无法计算
            return;

        // 当前股价（最新一天收盘价）
        double currentPrice = recentData[0].Close;

        FindConsolidationZones(recentData, currentPrice, out resistance1, out support1);
        // 计算压力位2：大幅冲高回落日的收盘价
        resistance2 = FindPullbackResistance(recentData);
        // 计算支撑位2：低点连线与当前的交点（低于今日股价）
        support2 = CalculateLowPointSupport(recentData, currentPrice);
    }

    /// <summary>
    /// 寻找横盘区域
    /// </summary>
    /// <param name="data">目标数据</param>
    /// <param name="isResistance">是否寻找压力位（true=压力位，false=支撑位）</param>
    /// <param name="currentPrice">当前股价</param>
    private static void FindConsolidationZones(List<StockBaseData> data, double currentPrice, out double resistance1, out double support1)
    {
        List<double> resistanceZones = new List<double>();
        List<double> supportZones = new List<double>();
        int dataCount = data.Count;

        // 滑动窗口检测横盘区域
        for (int i = 0; i <= dataCount - ConsolidationDays; i++)
        {
            List<StockBaseData> window = data.GetRange(i, ConsolidationDays);
            double maxPrice = window.Max(d => d.High);
            double minPrice = window.Min(d => d.Low);
            double midPrice = (maxPrice + minPrice) / 2; // 横盘区域中间价

            // 检测是否符合横盘特征（波动幅度小于阈值）
            double rangeRatio = (maxPrice - minPrice) / minPrice;
            if (rangeRatio <= RangeThreshold)
            {
                // 压力位需要横盘区域高于当前价，支撑位需要低于当前价
                if (midPrice > currentPrice)
                    resistanceZones.Add(midPrice);
                else if (midPrice < currentPrice)
                    supportZones.Add(midPrice);
            }
        }
        resistance1 = resistanceZones.Any() ? resistanceZones.Max() : -1;
        support1 = supportZones.Any() ? supportZones.Min() : -1;
    }

    /// <summary>
    /// 寻找大幅冲高回落的压力位（回落日收盘价）
    /// </summary>
    private static double FindPullbackResistance(List<StockBaseData> data)
    {
        // 冲高回落定义：当日最高价较开盘价上涨5%以上，且收盘价低于开盘价
        const double SurgeThreshold = 0.05; // 冲高幅度阈值

        foreach (var day in data.Skip(1)) // 跳过最新一天（避免当天数据）
        {
            double surgeRatio = (day.High - day.Open) / day.Open;
            if (surgeRatio >= SurgeThreshold && day.Close < day.Open)
            {
                return day.Close; // 返回符合条件的最近一天收盘价
            }
        }
        return -1;
    }

    /// <summary>
    /// 计算低点连线的支撑位（线性回归拟合最近低点）
    /// </summary>
    private static double CalculateLowPointSupport(List<StockBaseData> data, double currentPrice)
    {
        // 提取最近的有效低点（至少需要2个点才能连线）
        var lowPoints = new List<Tuple<double, double>>(); // (时间索引, 最低价)
        for (int i = 0; i < data.Count; i++)
        {
            // 只保留相对低点（比前后一天都低）
            if ((i == 0 || data[i].Low < data[i - 1].Low) &&
                (i == data.Count - 1 || data[i].Low < data[i + 1].Low))
            {
                lowPoints.Add(Tuple.Create((double)i, data[i].Low));
            }
        }

        if (lowPoints.Count < 2)
            return -1;

        // 线性回归计算低点趋势线（y = ax + b）
        double n = lowPoints.Count;
        double sumX = lowPoints.Sum(p => p.Item1);
        double sumY = lowPoints.Sum(p => p.Item2);
        double sumXY = lowPoints.Sum(p => p.Item1 * p.Item2);
        double sumX2 = lowPoints.Sum(p => p.Item1 * p.Item1);

        double denominator = n * sumX2 - sumX * sumX;
        if (denominator == 0)
            return -1;

        double a = (n * sumXY - sumX * sumY) / denominator; // 斜率
        double b = (sumY - a * sumX) / n;                   // 截距

        // 计算当前时间点（索引-1，因为最新数据是0，下一个时间点是-1）的趋势线价格
        double supportPrice = a * (-1) + b;

        // 确保支撑位低于当前股价
        return supportPrice < currentPrice ? supportPrice : -1;
    }

}
/// <summary>
/// MACD增量计算工具（适配StockBaseData结构）
/// </summary>
public static class MacdCalculator
{
    // MACD标准周期参数（可根据需求调整）
    private const int Ema12Period = 12;
    private const int Ema26Period = 26;
    private const int DeaPeriod = 9;

    // 预计算平滑系数（避免重复计算）
    private static readonly double Ema12Smoothing = 2.0 / (Ema12Period + 1);
    private static readonly double Ema26Smoothing = 2.0 / (Ema26Period + 1);
    private static readonly double DeaSmoothing = 2.0 / (DeaPeriod + 1);

    /// <summary>
    /// 给新的StockBaseData计算并设置MacdData
    /// </summary>
    /// <param name="historyData">历史数据列表（必须包含至少1条完整数据，且最后一条有MacdData）</param>
    /// <param name="newData">新数据（Close等字段已赋值，macdData为null）</param>
    /// <exception cref="ArgumentException">历史数据不满足计算条件时抛出</exception>
    public static void CalculateAndSetNewMacd<T>(List<T> historyData, T newData) where T : StockBaseData
    {
        // 核心逻辑：少于26条数据无法计算MACD，直接返回；等于26条时先初始化历史MACD
        if (historyData == null || historyData.Count < Ema26Period)
            return;
        if (historyData.Count == Ema26Period)
            InitHistoryMacd(historyData); // 初始化26条历史数据的MACD（适配下标0最新的存储逻辑）
        // 校验：初始化后，历史最后一条数据（最新数据，下标0）必须有完整的MacdData
        var latestHistoryData = historyData[0]; // 下标0是最新数据
        if (latestHistoryData.macdData == null)
        {
            UnityEngine.Debug.LogError("历史数据初始化MACD失败，最新记录（下标0）无MacdData");
            return;
        }

        // 校验新数据Close有效性
        if (newData == null || newData.Close <= 0)
        {
            UnityEngine.Debug.LogError("新数据无效（Close未赋值或小于等于0）");
            return;
        }

        // 1. 从历史最新数据（下标0）获取前一天的关键中间值（无反向推导，无递归）
        double prevEma12 = latestHistoryData.macdData.EMA12;
        double prevEma26 = latestHistoryData.macdData.EMA26;
        double prevDea = latestHistoryData.macdData.DEA;
        double currentClose = newData.Close;

        // 2. 按标准EMA公式递推新数据的EMA12、EMA26（新数据是最新的，基于前一个最新数据递推）
        double currentEma12 = currentClose * Ema12Smoothing + prevEma12 * (1 - Ema12Smoothing);
        double currentEma26 = currentClose * Ema26Smoothing + prevEma26 * (1 - Ema26Smoothing);

        // 3. 计算DIF、DEA、MACD（标准公式）
        double currentDif = currentEma12 - currentEma26;
        double currentDea = currentDif * DeaSmoothing + prevDea * (1 - DeaSmoothing);
        double currentMacd = 2 * (currentDif - currentDea); // 标准MACD柱状线公式

        // 4. 给新数据赋值（包含EMA12/EMA26，供下一条新数据使用）
        newData.macdData = new MacdData
        {
            EMA12 = Math.Round(currentEma12, 4),
            EMA26 = Math.Round(currentEma26, 4),
            DIF = Math.Round(currentDif, 4),
            DEA = Math.Round(currentDea, 4),
            MACD = Math.Round(currentMacd, 4)
        };
        // 提示：新增数据后，需将newData插入到historyData的下标0位置（保持下标0是最新数据）
        // 示例：historyData.Insert(0, newData);
    }
    /// <summary>
    /// 从历史最后一条数据的DIF和EMA26反向推导EMA12（因为 DIF = EMA12 - EMA26 → EMA12 = DIF + EMA26）
    /// </summary>
    private static double CalculateEma12FromDifAndEma26(StockBaseData lastData)
    {
        double prevEma26 = GetPreviousEma26(new List<StockBaseData> { lastData });
        return lastData.macdData.DIF + prevEma26;
    }
    /// <summary>
    /// 从数据中推导EMA26（如果数据中没有直接存储EMA26，通过前一天的MACD中间值反向计算）
    /// 核心逻辑：EMA26(t-1) = [EMA26(t) - Close(t)×平滑系数] / (1 - 平滑系数)
    /// 但由于我们没有直接存储EMA26，这里通过历史数据的递推关系反向推导最后一条的EMA26
    /// </summary>
    private static double GetPreviousEma26<T>(List<T> historyData) where T : StockBaseData
    {
        // 从历史最后一条数据反向推导EMA26（假设历史数据的MACD计算逻辑正确）
        var lastData = historyData.Last();
        if (historyData.Count == 1)
        {
            // 若历史只有1条数据（首次计算后的第一条），EMA26 = EMA12 - DIF（与EMA12推导逻辑一致）
            return CalculateEma12FromDifAndEma26(lastData) - lastData.macdData.DIF;
        }
        else
        {
            // 若历史有多条数据，取倒数第二条数据的EMA26，通过递推公式计算最后一条的EMA26
            var secondLastData = historyData[historyData.Count - 2];
            double secondLastEma26 = secondLastData.macdData.DIF + CalculateEma12FromDifAndEma26(secondLastData);
            return lastData.Close * Ema26Smoothing + secondLastEma26 * (1 - Ema26Smoothing);
        }
    }

    /// <summary>
    /// （可选）初始化历史数据的MACD（若历史数据未计算MACD，首次全量计算）
    /// 适用于历史数据为空或未计算MACD的场景
    /// </summary>
    public static void InitHistoryMacd<T>(List<T> historyData) where T : StockBaseData
    {
        if (historyData == null || historyData.Count != Ema26Period)
            throw new ArgumentException($"初始化MACD需传入正好{Ema26Period}条历史数据");

        // 关键修正1：按TradeDate降序排序（确保下标0是最新数据，最后一个元素是最旧数据，与存储逻辑一致）
        historyData.Sort((a, b) => DateTime.Compare(
            DateTime.ParseExact(b.TradeDate, "yyyyMMdd", null), // 后一个数据的日期在前，实现降序
            DateTime.ParseExact(a.TradeDate, "yyyyMMdd", null)));

        // 关键修正2：从最旧数据开始计算（历史数据最后一个元素是最旧的，下标=historyData.Count-1）
        // 初始化：最旧数据的EMA12=前12天最旧数据的SMA，EMA26=26条最旧数据的SMA（即自身所在的26条数据）
        int oldestIndex = historyData.Count - 1; // 最旧数据的下标
        var oldestData = historyData[oldestIndex];

        // 计算初始SMA（基于最旧的N条数据，按时间从远到近取）
        List<T> sortedOldToNew = historyData.AsEnumerable().Reverse().ToList(); // 临时列表：下标0=最旧，最后=最新（仅用于计算初始SMA）
        double initialEma12 = sortedOldToNew.Take(Ema12Period).Average(d => d.Close); // 前12条最旧数据的SMA
        double initialEma26 = sortedOldToNew.Take(Ema26Period).Average(d => d.Close); // 26条最旧数据的SMA（即全部数据）

        // 步骤1：从最旧数据到最新数据，递推计算EMA12、EMA26、DIF（时间顺序：旧→新）
        double currentEma12 = initialEma12;
        double currentEma26 = initialEma26;
        // 从最旧数据（oldestIndex）向前遍历到最新数据（index=0）
        for (int i = oldestIndex; i >= 0; i--)
        {
            var data = historyData[i];
            double close = data.Close;

            if (i != oldestIndex) // 非最旧数据，按EMA公式递推（基于前一个更旧数据的EMA）
            {
                // 前一个数据是更旧的（i+1下标比i下标更旧）
                var prevOlderData = historyData[i + 1];
                currentEma12 = close * Ema12Smoothing + prevOlderData.macdData.EMA12 * (1 - Ema12Smoothing);
                currentEma26 = close * Ema26Smoothing + prevOlderData.macdData.EMA26 * (1 - Ema26Smoothing);
            }

            // 计算DIF（EMA12 - EMA26）
            double dif = currentEma12 - currentEma26;
            data.macdData = new MacdData
            {
                EMA12 = Math.Round(currentEma12, 4),
                EMA26 = Math.Round(currentEma26, 4),
                DIF = Math.Round(dif, 4),
                DEA = 0, // 暂不赋值，后续统一计算
                MACD = 0
            };
        }

        // 步骤2：初始化DEA（基于最早的9个DIF，即最旧的9条数据的DIF的SMA）
        // 取最旧的9条数据（下标从oldestIndex到oldestIndex-DeaPeriod+1）
        List<double> initialDifs = new List<double>();
        for (int i = oldestIndex; i >= oldestIndex - DeaPeriod + 1; i--)
        {
            if (i >= 0) // 避免下标越界（确保取够9条）
                initialDifs.Add(historyData[i].macdData.DIF);
        }
        double initialDea = initialDifs.Average(); // 前9个DIF的SMA

        // 步骤3：从第10条旧数据开始，递推计算DEA和MACD（时间顺序：旧→新）
        for (int i = oldestIndex - DeaPeriod; i >= 0; i--)
        {
            var data = historyData[i];
            var prevOlderData = historyData[i + 1]; // 前一个更旧数据的DEA

            // 递推DEA（基于前一个更旧数据的DEA）
            double dea = data.macdData.DIF * DeaSmoothing + prevOlderData.macdData.DEA * (1 - DeaSmoothing);
            // 计算MACD柱状线（2*(DIF-DEA)）
            double macd = 2 * (data.macdData.DIF - dea);

            data.macdData.DEA = Math.Round(dea, 4);
            data.macdData.MACD = Math.Round(macd, 4);
        }

        // 给最早的9条数据中的最后一条（第9条旧数据）赋值初始DEA和MACD
        var ninthOldestData = historyData[oldestIndex - DeaPeriod + 1];
        ninthOldestData.macdData.DEA = Math.Round(initialDea, 4);
        ninthOldestData.macdData.MACD = Math.Round(2 * (ninthOldestData.macdData.DIF - initialDea), 4);
    }
}
