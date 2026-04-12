using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TechnicalAnalysisHelper
{
    //=======================================超跌反弹相关逻辑==============================================
    public int OversoldRebound(StockData stock, int checkDays = 20)
    {
        if (stock.TsCode == "000066.SZ")
            Debug.Log("Debug OversoldRebound");
        int score = 50;
        // 1. 检查是否有3连阴且量能递增
        int ThreeDownIndex = CheckThreeDownWithIncreasingVolume(stock.StockDailyDataList, checkDays);
        if (ThreeDownIndex == -1 || ThreeDownIndex < 3) //3连阳至少有3天
            return 0;
        // 2. 检查3连阴后是否有3连阳
        int ThreeUpIndex = CheckThreeUpAfterDown(stock.StockDailyDataList, ThreeDownIndex);
        if (ThreeUpIndex == -1)
            return 0;
        // 3. 检查试盘K线
        int TestK = CheckTestK(stock, checkDays);
        if (TestK == -1)
            return 0;
        // 4. 良性回调
        if (CheckHealthyPullback(stock, TestK, checkDays) == true)
            score += 10;
        return score;
    }

    // 1. 检查是否有3连阴且量能递增
    private int CheckThreeDownWithIncreasingVolume(List<StockDailyData> dailyData, int checkDays = 20)
    {
        // 取最近checkDays日数据
        var recentData = dailyData.Take(checkDays).ToList();

        for (int i = recentData.Count - 1; i >= 2; i--)
        {
            var day1 = recentData[i];
            var day2 = recentData[i - 1];
            var day3 = recentData[i - 2];

            // 检查是否3连阴（收盘价低于开盘价）
            bool isThreeDown = day1.Close < day1.Open &&
                              day2.Close < day1.Close &&
                              day3.Close < day2.Close;

            // 检查量能是否递增
            bool isVolumeIncreasing = day1.Vol < day2.Vol && day2.Vol < day3.Vol;

            if (isThreeDown && isVolumeIncreasing)
            {
                return i - 2;//返回3连阴的最后位置
            }
        }

        return -1;
    }

    // 2. 检查3连阴后是否有3连阳
    private int CheckThreeUpAfterDown(List<StockDailyData> dailyData, int ThreeDownIndex)
    {
        List<StockDailyData> recentData = dailyData.Take(ThreeDownIndex).ToList();

        for (int i = ThreeDownIndex - 1; i >= 2; i--)
        {
            var day1 = recentData[i];
            var day2 = recentData[i - 1];
            var day3 = recentData[i - 2];

            bool isThreeUp = day1.Close > day1.Open &&
                              day2.Close > day1.Close &&
                              day3.Close > day2.Close;

            if (isThreeUp)
            {
                return i - 2;//返回3连阳的最后位置
            }

        }

        return -1;
    }
    // 检查试盘K线
    private int CheckTestK(StockData stock, int checkDays = 20)
    {
        List<StockDailyData> recentData = stock.StockDailyDataList.Take(checkDays).ToList(); 

        // 计算平均成交量
        double avgVolume = recentData.Average(d => d.Vol);
        for (int i = 0; i < recentData.Count; i++)
        {
            StockDailyData stockDailyData = recentData[i];
            // 棉查是否是阳线
            bool isUpDay = stockDailyData.Close > stockDailyData.Open;
            // 检查是否有上影线（最高价大于收盘价）
            bool hasUpperShadow = stockDailyData.High > stockDailyData.Close;
            // 检查成交量是否达到平均的2倍
            bool isVolumeDouble = stockDailyData.Vol >= avgVolume * 2;
            // 检查上影线是否触及压力线
            bool touchesPressureLine = stockDailyData.High >= stock.ResistanceLevel;
            if (isUpDay && hasUpperShadow && isVolumeDouble && touchesPressureLine)
            {
                return i;
            }
        }
        return -1;
    }
    // 检查良性回调
    private bool CheckHealthyPullback(StockData stock, int TestKIndex, int checkDays = 20)
    {
        List<StockDailyData> recentData = stock.StockDailyDataList.Take(checkDays).ToList();
        // 计算平均成交量
        double avgVolume = recentData.Average(d => d.Vol);
        for (int i = 0; i < TestKIndex; i++)
        {
            StockDailyData stockDailyData = recentData[i];
            //// 检查是否为阴线
            //bool isDownDay = stockDailyData.Close < stockDailyData.Open;
            //// 检查回调幅度是否在5%以内
            //double pullbackPercentage = (peakPrice - stockDailyData.Close) / peakPrice * 100;
            if (stockDailyData.PctChg < -2 && stockDailyData.Vol < avgVolume*1.3)
            {
                return true;
            }
        }
        return false;
    }
    //=======================================缩量大跌==============================================
    public bool SmallVolumeDown(StockData stock, int checkDays = 20)
    {
        List<StockDailyData> recentData = stock.StockDailyDataList.Take(checkDays).ToList();
        if (recentData.Count < checkDays)
            return false;
        
        // 计算20日平均成交量
        double avgVolume = recentData.Average(d => d.Vol);
        
        // 获取最新一天的数据（列表最后一个元素）
        StockDailyData latestData = recentData[recentData.Count - 1];
        
        // 检查股价跌幅是否超过4%
        bool isPriceDropOver4Percent = latestData.PctChg < -4;
        // 检查成交量是否低于20日平均成交量的0.6倍
        bool isVolumeLow = latestData.Vol < avgVolume * 0.6;
        
        // 如果同时满足两个条件，返回true，否则返回false
        return isPriceDropOver4Percent && isVolumeLow;
    }
    // 1. 检查是否有缩量大跌
    //=======================================缩量大跌==============================================
    /// <summary>
    /// 分析股票筹码集中度，返回按90%筹码集中度倒序排列的前50只股票
    /// </summary>
    /// <returns>股票及其每日数据的列表</returns>
    public static List<(StockData Stock, StockDailyData DailyData)> AnalyzeConcentration()
    {
        // 读取所有的股票的最新一天的筹码峰数据
        List<StockData> allStocks = Mgrs.Ins.basicMgr.LoadAllBasicStockCodes();
        List<(StockData Stock, StockDailyData DailyData)> stockConcentrationData = new List<(StockData, StockDailyData)>();
        
        foreach (var stock in allStocks)
        {
            // 读取最新的每日数据文件
            var (newestData, _) = Mgrs.Ins.dailyDataMgr.LoadNewestDailyStockDataFromFile(stock.TsCode);
            if (newestData != null && newestData.Count > 0)
            {
                // 获取最新一天的数据
                StockDailyData latestData = newestData[0];
                // 检查是否有筹码集中度数据
                if (latestData.Concentration90 > 0)
                {
                    stockConcentrationData.Add((stock, latestData));
                }
            }
        }
        
        // 按90%筹码集中度倒序排列
        var sortedData = stockConcentrationData.OrderBy(item => item.DailyData.Concentration90).Take(50).ToList();
        
        return sortedData;
    }
}