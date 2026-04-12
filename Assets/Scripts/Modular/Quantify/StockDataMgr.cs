using ClientBase;
using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StockData
{
    public string TsCode;
    public string Name; //接口：stock_basic
    public string Market;//市场类别 （主板/创业板/科创板/CDR/北交所）接口：stock_basic
    public string Industry;//所属行业 接口：stock_basic
    public double Total_mv;//总市值 接口：daily_basic
    public double Pe;//市盈率（总市值/净利润， 亏损的PE为空）接口：daily_basic
    public double Pe_ttm;//市盈率（TTM，亏损的PE为空）接口：daily_basic
    
    public string AnnDate;//公告日期 接口：stk_holdernumber
    public string EndDate;//截止日期 接口：stk_holdernumber
    public double HolderNum;//股东人数 接口：stk_holdernumber
    public List<StockDailyData> StockDailyDataList;//每日数据
    public List<StockWeeklyData> StockWeeklyDataList;//每周数据
    public double SupportLevel;//支撑位
    public double ResistanceLevel;//压力位
    public double ReliabilityScore;//支撑位和压力位的可靠性评分
}
[System.Serializable]
public class StockBaseData
{
    public string TsCode;
    public string TradeDate;
    public double Open;
    public double High;
    public double Low;
    public double Close;
    public double Vol;
    public double Amount;
    public double PctChg;
    public MacdData macdData;
    public KdjData kdjData;
    public double FiveDayAvgPrice;//5日均线
    public double TenDayAvgPrice;//10日均线
    public double TwentyDayAvgPrice;//20日均线
    public double FiveDayAvgVol;//5日均量
    public double TenDayAvgVol;//10日均量
    public double TwentyDayAvgVol;//20日均量
    public double Concentration70;//70% 筹码集中度
    public double Concentration90;//90% 筹码集中度
    public double ConcentrationPrice;// 筹码主峰价格
}

[System.Serializable]
public class StockDailyData: StockBaseData
{
    public double Total_mv;//从最新每日数据读取
    public double Pe;//从最新每日数据读取
    public double Pe_ttm;//从最新每日数据读取
    
}
[System.Serializable]
public class StockWeeklyData : StockBaseData
{

}

public class SelectedStock
{
    public StockData stockData;
    public int oversoldScore;
}
public class StockDataMgr : MgrBase
{
    public TechnicalAnalysisHelper technicalAnalysisHelper = new TechnicalAnalysisHelper();
    public TechnicalDataHelper technicalDataHelper = new TechnicalDataHelper();
    public List<StockData> allStocksDataList = new List<StockData>();//所有股票数据列表
    public List<SelectedStock> selectedStocksList = new List<SelectedStock>();//选中的股票列表
    private int lookbackPeriod = 20;//技术分析的天数
    //public override void Init()
    //{
    //    // 初始化三个子管理器
    //    //Mgrs.Ins.basicMgr.Init();
    //    //Mgrs.Ins.dailyDataMgr.Init();
    //    //Mgrs.Ins.weekDataMgr.Init();
    //}

    //// 基础数据相关方法
    //public void GetAllStocksBasicInfo()
    //{
    //    Mgrs.Ins.basicMgr.GetAllStocksBasicInfo();
    //}

    //public StockData LoadBasicStockDataFromFile(string filePath)
    //{
    //    return Mgrs.Ins.basicMgr.LoadBasicStockDataFromFile(filePath);
    //}

    //// 每日数据相关方法
    //public void UpdateAllDailyStocksRecursive()
    //{
    //    Mgrs.Ins.dailyDataMgr.UpdateAllDailyStocksRecursive();
    //}

    //public void GetAllStocksLastNDaysRecursive(int days = 30)
    //{
    //    Mgrs.Ins.dailyDataMgr.GetAllStocksLastNDaysRecursive(days);
    //}

    //public async System.Threading.Tasks.Task GetAllStocksChipPeakData()
    //{
    //    await Mgrs.Ins.dailyDataMgr.GetAllStocksChipPeakData();
    //}

    //public void SaveDailyStockData(string tsCode, List<StockDailyData> dailyData)
    //{
    //    Mgrs.Ins.dailyDataMgr.SaveDailyStockData(tsCode, dailyData);
    //}

    //public List<StockDailyData> LoadDailyStockDataFromFile(string filePath)
    //{
    //    return Mgrs.Ins.dailyDataMgr.LoadDailyStockDataFromFile(filePath);
    //}

    //public void CalculateAndUpdateAverageData()
    //{
    //    Mgrs.Ins.dailyDataMgr.CalculateAndUpdateAverageData();
    //}

    //// 每周数据相关方法
    //public void UpdateAllWeeklyStocksRecursive()
    //{
    //    Mgrs.Ins.weekDataMgr.UpdateAllWeeklyStocksRecursive();
    //}

    //public void GetAllStocksLastNWeeksRecursive(int weeks = 30)
    //{
    //    Mgrs.Ins.weekDataMgr.GetAllStocksLastNWeeksRecursive(weeks);
    //}

    //public void SaveWeeklyStockData(string tsCode, List<StockWeeklyData> weekData)
    //{
    //    Mgrs.Ins.weekDataMgr.SaveWeeklyStockData(tsCode, weekData);
    //}

    //public List<StockWeeklyData> LoadWeeklyStockDataFromFile(string filePath)
    //{
    //    return Mgrs.Ins.weekDataMgr.LoadWeeklyStockDataFromFile(filePath);
    //}

    // 获取所有已保存的股票代码
    //public List<StockData> GetAllSavedStockCodes()
    //{
    //    return Mgrs.Ins.basicMgr.GetAllSavedStockCodes();
    //}

    //// 从json加载单只股票数据
    //private StockData LoadSingleStockDataFromFile(string filePath)
    //{
    //    StockData stockData = Mgrs.Ins.basicMgr.LoadBasicStockDataFromFile(filePath);
    //    string dailyFile = System.IO.Path.Combine(Mgrs.Ins.dailyDataMgr.GetDailyDataDirectory(), System.IO.Path.GetFileName(filePath));
    //    stockData.StockDailyDataList = Mgrs.Ins.dailyDataMgr.LoadDailyStockDataFromFile(dailyFile);
    //    //string weeklyFile = System.IO.Path.Combine(Mgrs.Ins.weekDataMgr.GetWeekDataDirectory(), System.IO.Path.GetFileName(filePath));
    //    //stockData.StockWeeklyDataList = Mgrs.Ins.weekDataMgr.LoadWeeklyStockDataFromFile(weeklyFile);
    //    return stockData;
    //}

    public void AnalyzeOversoldRebound()
    {
        allStocksDataList = Mgrs.Ins.basicMgr.LoadAllBasicStockCodes();
        for (int i = 0; i < allStocksDataList.Count; i++)
        {
            technicalDataHelper.CalculateSupportResistance(allStocksDataList[i], lookbackPeriod);
            //============================所有的计分逻辑============================
            int oversoldScore = technicalAnalysisHelper.OversoldRebound(allStocksDataList[i], lookbackPeriod);
            //======================================================================
            if (oversoldScore > 0)//新加的逻辑往后或
            {
                SelectedStock selectedStock = new SelectedStock
                {
                    stockData = allStocksDataList[i],
                    oversoldScore = oversoldScore
                };
                selectedStocksList.Add(selectedStock);
            }
        }
    }

    public void AnalyzeExtremeWashout()
    {
        allStocksDataList = Mgrs.Ins.basicMgr.LoadAllBasicStockCodes();
    }

    public override void Release()
    {

    }
}