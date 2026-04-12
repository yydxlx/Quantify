using System;
using System.Collections.Generic;
using System.Linq;

// MACD指标数据结构
[System.Serializable]
public class MacdData
{
    public double DIF;   // 差离值
    public double DEA;   // 信号线
    public double MACD;  // 柱状线（DIF-DEA）
    public double EMA12;
    public double EMA26;
}
// KDJ指标数据结构
[System.Serializable]
public class KdjData
{
    public double K;
    public double D;
    public double J;
}
public class TechnicalDataHelper
{
    
    /// <summary>
    /// 计算短期压力位和支撑位（结合量价）
    /// </summary>
    /// <param name="dailyDataList">股票每日数据（需按时间升序排列，至少包含20个交易日）</param>
    /// <param name="lookbackPeriod">短期回溯周期（默认20个交易日）</param>
    /// <param name="volumeMultiple">放量阈值（默认1.3倍均值）</param>
    /// <param name="pctChgThreshold">涨跌阈值（默认2%）</param>
    /// <returns>支撑位、压力位结果（带可靠性评分：0-100分）</returns>
    public void CalculateSupportResistance(StockData stockData, int checkDays = 20)
    {
        List<StockDailyData> dailyDataList = stockData.StockDailyDataList;
        // 校验输入（新增指标字段非空校验）
        if (dailyDataList == null || dailyDataList.Count < checkDays)
            return;
        if (dailyDataList.Any(data => data.macdData == null || data.kdjData == null))
            return;
        double volumeMultiple = 1.3;
        double pctChgThreshold = 2.0;
        if (dailyDataList[0].Total_mv >= 50000000000)
        {
            volumeMultiple = 1.3; // 大市值股票放量标准降低
            pctChgThreshold = 1.5;
        }
        else if (dailyDataList[0].Total_mv < 5000000000 && dailyDataList[0].Total_mv >= 1000000000)
        {
            volumeMultiple = 1.5; // 小市值股票放量标准提高
            pctChgThreshold = 1.8;
        }
        else//(< 1000000000)
        {
            volumeMultiple = 1.8;
            pctChgThreshold = 2;
        }
        // 按时间排序，取最近N个交易日
        var recentData = dailyDataList.Take(checkDays).ToList();

        // 计算近N日成交量均值（放量判定标准）
        double avgVolume = recentData.Average(data => data.Vol);
        double heavyVolumeThreshold = avgVolume * volumeMultiple;

        // -------------------------- 第一步：筛选支撑位（量价+指标共振） --------------------------
        List<(double Price, int Weight)> supportCandidates = new List<(double, int)>();
        for (int i = 1; i < recentData.Count; i++) // i从1开始，需要前一日数据判断MACD交叉
        {
            var current = recentData[i];
            var prev = recentData[i - 1];

            // 基础条件：放量
            bool isHeavyVolume = current.Vol >= heavyVolumeThreshold;
            if (!isHeavyVolume) continue;

            // 量价支撑场景（同之前，但新增指标筛选）
            bool isSupportScenario1 = current.PctChg < -pctChgThreshold && (current.Close - current.Low) / (current.High - current.Low) < 0.3; // 放量下跌承接
            bool isSupportScenario2 = current.PctChg > pctChgThreshold && (current.Open - current.Low) / (current.High - current.Low) < 0.3; // 放量上涨启动

            if (isSupportScenario1 || isSupportScenario2)
            {
                // 指标筛选：支撑位有效性判定
                int weight = 1; // 基础权重

                // 1. MACD金叉共振（前一日DIF<DEA，当日DIF>=DEA → 金叉，支撑位更可靠）
                bool isMacdGoldenCross = prev.macdData.DIF < prev.macdData.DEA && current.macdData.DIF >= current.macdData.DEA;
                if (isMacdGoldenCross) weight *= 2;

                // 计算支撑价（最低价+收盘价均值，加入权重）
                double supportPrice = (current.Low + current.Close) / 2;
                supportCandidates.Add((supportPrice, (int)weight));
            }
        }

        // 补充：无共振支撑时，取近N日成交密集区下沿（中位数），权重1
        if (!supportCandidates.Any())
        {
            var sortedLows = recentData.OrderBy(data => data.Low).Select(data => data.Low).ToList();
            double defaultSupport = sortedLows[sortedLows.Count / 2];
            supportCandidates.Add((defaultSupport, 1));
        }

        // 计算最终支撑位（加权平均，权重越高影响越大）
        double totalSupportWeight = supportCandidates.Sum(c => c.Weight);
        stockData.SupportLevel = supportCandidates.Sum(c => c.Price * c.Weight) / totalSupportWeight;

        // -------------------------- 第二步：筛选压力位（量价+指标共振） --------------------------
        List<(double Price, int Weight)> resistanceCandidates = new List<(double, int)>();
        for (int i = 1; i < recentData.Count; i++) // i从1开始，需要前一日数据判断MACD交叉
        {
            var current = recentData[i];
            var prev = recentData[i - 1];

            // 基础条件：放量
            bool isHeavyVolume = current.Vol >= heavyVolumeThreshold;
            if (!isHeavyVolume) continue;

            // 量价压力场景（同之前，新增指标筛选）
            bool isResistanceScenario1 = current.PctChg > pctChgThreshold && (current.High - current.Close) / (current.High - current.Low) < 0.3; // 放量滞涨
            bool isResistanceScenario2 = current.PctChg < -pctChgThreshold && (current.High - current.Open) / (current.High - current.Low) < 0.3; // 放量下跌启动

            if (isResistanceScenario1 || isResistanceScenario2)
            {
                // 指标筛选：压力位有效性判定
                int weight = 1; // 基础权重

                // 1. MACD死叉共振（前一日DIF>DEA，当日DIF<=DEA → 死叉，压力位更可靠）
                bool isMacdDeathCross = prev.macdData.DIF > prev.macdData.DEA && current.macdData.DIF <= current.macdData.DEA;
                if (isMacdDeathCross) weight *= 2;

                // 计算压力价（最高价+收盘价均值，加入权重）
                double resistancePrice = (current.High + current.Close) / 2;
                resistanceCandidates.Add((resistancePrice, (int)weight));
            }
        }

        // 补充：无共振压力时，取近N日成交密集区上沿（中位数），权重1
        if (!resistanceCandidates.Any())
        {
            var sortedHighs = recentData.OrderBy(data => data.High).Select(data => data.High).ToList();
            double defaultResistance = sortedHighs[sortedHighs.Count / 2];
            resistanceCandidates.Add((defaultResistance, 1));
        }

        // 计算最终压力位（加权平均）
        double totalResistanceWeight = resistanceCandidates.Sum(c => c.Weight);
        stockData.ResistanceLevel = resistanceCandidates.Sum(c => c.Price * c.Weight) / totalResistanceWeight;

        // -------------------------- 第三步：结果优化（安全校验+可靠性评分） --------------------------
        // 安全校验：确保支撑位 < 压力位
        if (stockData.SupportLevel >= stockData.ResistanceLevel)
        {
            double midPrice = (stockData.SupportLevel + stockData.ResistanceLevel) / 2;
            stockData.SupportLevel = midPrice * 0.995;
            stockData.ResistanceLevel = midPrice * 1.005;
        }

        // 计算可靠性评分（0-100分）：权重总和/最大可能权重（单场景最大权重=2*1.5=3，取前3个最高权重求和）
        int maxPossibleWeight = 3 * 3; // 假设取3个最高权重场景，每个最大3分
        int actualTotalWeight = supportCandidates.Sum(c => c.Weight) + resistanceCandidates.Sum(c => c.Weight);

        // 四舍五入保留2位小数
        stockData.SupportLevel = Math.Round(stockData.SupportLevel, 2);
        stockData.ResistanceLevel = Math.Round(stockData.ResistanceLevel, 2);
        stockData.ReliabilityScore = Math.Clamp((int)(actualTotalWeight / (double)maxPossibleWeight * 100), 0, 100);
    }

    /// <summary>
    /// 辅助方法：判断支撑/压力位是否有效突破（结合实时量价+指标）
    /// </summary>
    /// <returns>突破状态（-1=跌破支撑，0=区间震荡，1=突破压力）</returns>
    public int JudgeBreakthrough(StockData stockData)
    {
        if (stockData.SupportLevel == 0)
            return 0;
        StockDailyData lastDailyData = stockData.StockDailyDataList[0];
        // 跌破支撑位：收盘价 < 支撑位 * 0.995（有效跌破，带放量+MACD死叉更确认）
        bool isBreakSupport = lastDailyData.Close < stockData.SupportLevel;
        if (isBreakSupport)
        {
            bool isHeavyVolume = lastDailyData.Vol > lastDailyData.Vol * 1.2; // 放量跌破
            bool isMacdWeak = lastDailyData.macdData.DIF < lastDailyData.macdData.DEA; // MACD空头
            return (isHeavyVolume && isMacdWeak) ? -1 : 0; // 放量+空头 → 确认跌破
        }

        // 突破压力位：收盘价 > 压力位 * 1.005（有效突破，带放量+MACD金叉更确认）
        bool isBreakResistance = lastDailyData.Close > stockData.ResistanceLevel * 1.005;
        if (isBreakResistance)
        {
            bool isHeavyVolume = lastDailyData.Vol > lastDailyData.Vol * 1.2; // 放量突破
            bool isMacdStrong = lastDailyData.macdData.DIF > lastDailyData.macdData.DEA; // MACD多头
            return (isHeavyVolume && isMacdStrong) ? 1 : 0; // 放量+多头 → 确认突破
        }

        // 区间震荡
        return 0;
    }
}