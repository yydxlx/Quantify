using ClientBase;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class QuantifyDlg : MonoBehaviour
{
    // UI组件 - 全部改为私有，在Awake中Find
    private InputField DateInputField;
    private InputField SingleStockInput;
    private InputField DaysInputField;
    private Toggle DataFetchToggle;
    private Toggle DataAnalyzeToggle;
    private Button FetchBasicButton;
    private Button AvgButton;
    private Button ConcentrationButton;
    private GameObject DataFetchPanel;
    private Button FetchAll1DaysButton;
    private Button FetchAll2DaysButton;
    private Button FetchAll5DaysButton;
    private Button FetchAll30DaysButton;
    private Button FetchAll60DaysButton;
    private Button UpdateAllDaysButton;
    private Button FetchAll30WeeksButton;
    private Button UpdateAllWeeksButton;

    private Button FetchSingleStockButton;
    private Button AnalyzeDrop5PercentButton;
    private Text StatusText;

    
    private Slider ProgressSlider;
    private GameObject ProgressPanel;
    private Text ProgressText;
    
    private GameObject DataAnalyzePanel;
    private Button OversoldReboundButton;
    private Button ExtremeWashoutButton;
    private Button ConcentrationAnalyzeButton;
    private ScrollRect ResultScrollRect;
    private Transform ResultContent;
    private Text ResultText;

    private bool IsProcessing = false;

    void Awake()
    {
        // 查找所有UI组件
        FindUIComponents();
    }
    private void FindUIComponents()
    {
        // 查找输入框
        DateInputField = transform.Find("InputGroup/DateInput").GetComponent<InputField>();
        SingleStockInput = transform.Find("InputGroup/StockInput").GetComponent<InputField>();
        DaysInputField = transform.Find("InputGroup/DaysInput").GetComponent<InputField>();

        // 查找Toggle
        DataFetchToggle = transform.Find("toggleGroup/DataFetchToggle").GetComponent<Toggle>();
        DataAnalyzeToggle = transform.Find("toggleGroup/DataAnalyzeToggle").GetComponent<Toggle>();

        // 查找面板
        DataFetchPanel = transform.Find("DataFetchPanel").gameObject;
        // 查找按钮
        FetchBasicButton = transform.Find("DataFetchPanel/buttonGroup/FetchBasicButton").GetComponent<Button>();
        AvgButton = transform.Find("DataFetchPanel/buttonGroup/AvgButton").GetComponent<Button>();
        ConcentrationButton = transform.Find("DataFetchPanel/buttonGroup/ConcentrationButton").GetComponent<Button>();

        FetchAll1DaysButton = transform.Find("DataFetchPanel/buttonGroup/FetchAll1DaysButton").GetComponent<Button>();
        FetchAll2DaysButton = transform.Find("DataFetchPanel/buttonGroup/FetchAll2DaysButton").GetComponent<Button>();
        FetchAll5DaysButton = transform.Find("DataFetchPanel/buttonGroup/FetchAll5DaysButton").GetComponent<Button>();
        
        FetchAll30DaysButton = transform.Find("DataFetchPanel/buttonGroup/FetchAll30DaysButton").GetComponent<Button>();
        FetchAll60DaysButton = transform.Find("DataFetchPanel/buttonGroup/FetchAll60DaysButton").GetComponent<Button>();
        UpdateAllDaysButton = transform.Find("DataFetchPanel/buttonGroup/UpdateAllDaysButton").GetComponent<Button>();
        FetchAll30WeeksButton = transform.Find("DataFetchPanel/buttonGroup/FetchAll30WeeksButton").GetComponent<Button>();
        UpdateAllWeeksButton = transform.Find("DataFetchPanel/buttonGroup/UpdateAllWeeksButton").GetComponent<Button>();

        FetchSingleStockButton = transform.Find("DataFetchPanel/buttonGroup/FetchSingleStockButton").GetComponent<Button>();
        AnalyzeDrop5PercentButton = transform.Find("DataAnalyzePanel/AnalyzeDrop5PercentButton").GetComponent<Button>();

        // 查找文本和状态
        StatusText = transform.Find("StatusText").GetComponent<Text>();


        
        // 查找进度条
        ProgressPanel = transform.Find("ProgressGroup").gameObject;
        ProgressSlider = transform.Find("ProgressGroup/ProgressSlider").GetComponent<Slider>();
        ProgressText = transform.Find("ProgressGroup/ProgressText").GetComponent<Text>();

        // 分析面板
        DataAnalyzePanel = transform.Find("DataAnalyzePanel").gameObject;
        OversoldReboundButton = transform.Find("DataAnalyzePanel/OversoldReboundButton").GetComponent<Button>();
        ExtremeWashoutButton = transform.Find("DataAnalyzePanel/ExtremeWashoutButton").GetComponent<Button>();
        ConcentrationAnalyzeButton = transform.Find("DataAnalyzePanel/ConcentrationAnalyzeButton").GetComponent<Button>();
        
        // 滚动视图
        ResultScrollRect = transform.Find("DataAnalyzePanel/ResultPanel/ResultContent").GetComponent<ScrollRect>();
        ResultContent = transform.Find("DataAnalyzePanel/ResultPanel/ResultContent/Viewport/Content");
        ResultText = transform.Find("DataAnalyzePanel/ResultPanel/ResultContent/Viewport/Content/Text").GetComponent<Text>();
    }
    void Start()
    {
        // 设置默认值
        DateInputField.text = DateTime.Now.ToString("yyyyMMdd");
        SingleStockInput.text = "000001.SZ";
        DaysInputField.text = "30";

        // 绑定事件
        BindEvents();

        // 初始化界面状态
        InitializeUI();
    }
    private void OnEnable()
    {
        //Debug.Log("QuantifyDlg OnEnable");
        EventManager.Ins.Regist("UpdateStatus", UpdateStatus);
        EventManager.Ins.Regist("OnFetchStart", OnFetchStart);
        EventManager.Ins.Regist("OnFetchFin", OnFetchFin);
    }
    private void OnDisable()
    {
        //Debug.Log("QuantifyDlg OnDisable");
        EventManager.Ins.Unregist("UpdateStatus", UpdateStatus);
        EventManager.Ins.Unregist("OnFetchStart", OnFetchStart);
        EventManager.Ins.Unregist("OnFetchFin", OnFetchFin);
    }

    private void BindEvents()
    {
        // 绑定Toggle事件
        DataFetchToggle.onValueChanged.AddListener(OnDataFetchToggleChanged);
        DataAnalyzeToggle.onValueChanged.AddListener(OnDataAnalyzeToggleChanged);

        // 绑定按钮事件
        FetchBasicButton.onClick.AddListener(OnFetchBasicClick);
        AvgButton.onClick.AddListener(OnAvgButtonClick);
        ConcentrationButton.onClick.AddListener(OnConcentrationButtonClick);
        FetchAll1DaysButton.onClick.AddListener(OnFetchAll1DaysClick);
        FetchAll2DaysButton.onClick.AddListener(OnFetchAll2DaysClick);
        FetchAll5DaysButton.onClick.AddListener(OnFetchAll5DaysClick);
        FetchAll30DaysButton.onClick.AddListener(OnFetchAll30DaysClick);
        FetchAll60DaysButton.onClick.AddListener(OnFetchAll60DaysClick);
        UpdateAllDaysButton.onClick.AddListener(OnUpdateAllDaysClick);
        FetchAll30WeeksButton.onClick.AddListener(OnFetchAll30WeeksClick);
        UpdateAllWeeksButton.onClick.AddListener(OnUpdateAllWeeksClick);
        
        OversoldReboundButton.onClick.AddListener(OnOversoldReboundClick);
        ExtremeWashoutButton.onClick.AddListener(OnExtremeWashoutClick);
        ConcentrationAnalyzeButton.onClick.AddListener(OnConcentrationAnalyzeClick);

        //FetchSingleStockButton.onClick.AddListener(OnFetchSingleStockClick);
        //AnalyzeDrop5PercentButton.onClick.AddListener(OnAnalyzeDrop5PercentClick);
    }

    private void InitializeUI()
    {
        // 默认显示数据获取面板
        DataFetchPanel.SetActive(true);
        DataAnalyzePanel.SetActive(false);
        ProgressPanel.SetActive(false);
    }

    private void OnDataFetchToggleChanged(bool isOn)
    {
        if (isOn)
        {
            DataFetchPanel.SetActive(true);
            DataAnalyzePanel.SetActive(false);
            //UpdateStatus("切换到数据获取模式");
        }
    }

    private void OnDataAnalyzeToggleChanged(bool isOn)
    {
        if (isOn)
        {
            DataFetchPanel.SetActive(false);
            DataAnalyzePanel.SetActive(true);
            //UpdateStatus("切换到数据分析模式");
        }
    }
    private void OnFetchBasicClick()
    {
        Mgrs.Ins.basicMgr.GetAllStocksBasicInfo();
    }
    private void OnAvgButtonClick()
    {
        Mgrs.Ins.dailyDataMgr.CalculateAndUpdateAverageData();
    }
    private void OnFetchAll1DaysClick()
    {
        OnFetchAllDaysClick(1);
    }
    private void OnFetchAll2DaysClick()
    {
        OnFetchAllDaysClick(2);
    }
    private void OnFetchAll5DaysClick()
    {
        OnFetchAllDaysClick(5);
    }
    private void OnFetchAll30DaysClick()
    {
        OnFetchAllDaysClick(30);
    }
    private void OnFetchAll60DaysClick()
    {
        OnFetchAllDaysClick(60);
    }
    private void OnUpdateAllDaysClick()
    {
        if (IsProcessing)
            return;
        Mgrs.Ins.dailyDataMgr.UpdateAllDailyStocksRecursive();
    }
    private void OnConcentrationButtonClick()
    {
        if (IsProcessing)
            return;
        Mgrs.Ins.chipMgr.GetAllStocksChipPeakData();
    }
    private void OnFetchAll30WeeksClick()
    {
        OnFetchAllWeeksClick(30);
    }
    private void OnUpdateAllWeeksClick()
    {
        if (IsProcessing)
            return;
        Mgrs.Ins.weekDataMgr.UpdateAllWeeklyStocksRecursive();
    }
    private void OnOversoldReboundClick()
    {
        Mgrs.Ins.stockDataMgr.AnalyzeOversoldRebound();
        ShowSelectedStocks();
    }
    private void OnExtremeWashoutClick()
    {
        Mgrs.Ins.stockDataMgr.AnalyzeExtremeWashout();
        ShowSelectedStocks();
    }
    private void OnConcentrationAnalyzeClick()
    {
        // 调用TechnicalAnalysisHelper.AnalyzeConcentration()方法获取分析结果
        var sortedData = TechnicalAnalysisHelper.AnalyzeConcentration();
        
        // 显示结果
        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"筹码集中度分析结果（共{sortedData.Count}只）");
        sb.AppendLine("======================================");
        sb.AppendLine("代码            名字            市盈率PE        市值(亿)        板块        90集中度        70集中度            主峰价格");
        sb.AppendLine("--------------------------------------");
        foreach (var item in sortedData)
        {
            sb.AppendLine($"{item.Stock.TsCode,-10} {item.Stock.Name,-10} {item.DailyData.Pe_ttm,-12:F2} {item.DailyData.Total_mv,-12:F2} {item.Stock.Market,-12} {item.DailyData.Concentration90,-12:F2} {item.DailyData.Concentration70,-12:F2} {item.DailyData.ConcentrationPrice,-12:F2}");
        }
        
        ResultText.text = sb.ToString();
    }
    public void ShowSelectedStocks()
    {
        List<SelectedStock> selectedStocksList = Mgrs.Ins.stockDataMgr.selectedStocksList;
        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"超跌反弹筛选结果（共{selectedStocksList.Count}只）");
        sb.AppendLine("======================================");
        sb.AppendLine("代码            名字            市盈率PE        市值(亿)        板块        超跌分数");
        sb.AppendLine("--------------------------------------");
        foreach (var selectedStock in selectedStocksList)
        {
            StockDailyData latestData = selectedStock.stockData.StockDailyDataList[0];
            sb.AppendLine($"{selectedStock.stockData.TsCode,-10} {selectedStock.stockData.Name,-10} {latestData.Pe_ttm,-12:F2} {latestData.Total_mv,-12:F2}{selectedStock.stockData.Market,-12:F2}{selectedStock.oversoldScore,-12:F2}");
        }

        ResultText.text = sb.ToString();
    }
    private void OnFetchAllDaysClick(int days)
    {
        if (IsProcessing) 
            return;
        Mgrs.Ins.dailyDataMgr.GetAllStocksLastNDaysRecursive(days);
    }
    private void OnFetchAllWeeksClick(int weeks)
    {
        if (IsProcessing)
            return;
        Mgrs.Ins.weekDataMgr.GetAllStocksLastNWeeksRecursive(weeks);
    }
    // 获取单个股票数据
    //private async void OnFetchSingleStockClick()
    //{
    //    string tsCode = SingleStockInput.text.Trim();
    //    string daysText = DaysInputField.text.Trim();

    //    if (string.IsNullOrEmpty(tsCode))
    //    {
    //        UpdateStatus("请输入股票代码");
    //        return;
    //    }

    //    if (!int.TryParse(daysText, out int days) || days <= 0)
    //    {
    //        UpdateStatus("请输入有效的天数");
    //        return;
    //    }

    //    if (IsProcessing) 
    //        return;
    //    Mgrs.Ins.stockDataMgr.GetSingleStockLastNDays(tsCode, days);
    //}

    // 分析跌幅超过5%的股票
    //private void OnAnalyzeDrop5PercentClick()
    //{
    //    try
    //    {
    //        UpdateStatus("正在分析跌幅超过5%的股票...");

    //        var droppedStocks = Mgrs.Ins.stockDataMgr.FindStocksDropOver5Percent();

    //        if (droppedStocks.Count > 0)
    //        {
    //            UpdateStatus($"找到 {droppedStocks.Count} 只跌幅超过5%的股票");
    //            DisplayDroppedStocks(droppedStocks);
    //        }
    //        else
    //        {
    //            UpdateStatus("未找到跌幅超过5%的股票");
    //            DisplayResult("当前数据中没有跌幅超过5%的股票");
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        UpdateStatus($"分析失败: {ex.Message}");
    //        DisplayResult($"错误: {ex.Message}");
    //    }
    //}

    private void DisplaySingleStockResult(string tsCode, List<StockDailyData> stockData)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"{tsCode} 历史数据 ({stockData.Count} 天)");
        sb.AppendLine("=================================");

        // 显示统计信息
        var firstDay = stockData.First();
        var lastDay = stockData.Last();
        double totalChange = ((lastDay.Close - firstDay.Close) / firstDay.Close) * 100;

        sb.AppendLine($"数据期间: {firstDay.TradeDate} 到 {lastDay.TradeDate}");
        sb.AppendLine($"期初价格: {firstDay.Close:F2}");
        sb.AppendLine($"期末价格: {lastDay.Close:F2}");
        sb.AppendLine($"累计涨幅: {totalChange:F2}%");
        sb.AppendLine($"最大涨幅: {stockData.Max(d => d.PctChg):F2}%");
        sb.AppendLine($"最大跌幅: {stockData.Min(d => d.PctChg):F2}%");
        sb.AppendLine();

        // 显示最近5个交易日
        sb.AppendLine("最近5个交易日:");
        sb.AppendLine("日期      收盘价   涨跌幅");
        sb.AppendLine("-------------------------");

        var recentData = stockData.TakeLast(5).ToList();
        foreach (var day in recentData)
        {
            sb.AppendLine($"{day.TradeDate}  {day.Close:F2}    {day.PctChg:+#.##%;-#.##%;0%}");
        }

        //DisplayResult(sb.ToString());
    }

    //private void DisplayDroppedStocks(List<StockDropInfo> droppedStocks)
    //{
    //    StringBuilder sb = new StringBuilder();
    //    sb.AppendLine($"跌幅超过5%的股票 (共{droppedStocks.Count}只)");
    //    sb.AppendLine("=================================");
    //    sb.AppendLine("股票代码   股票名称   最新跌幅   最新收盘价");
    //    sb.AppendLine("---------------------------------");

    //    foreach (var stock in droppedStocks.OrderByDescending(s => s.LatestDrop))
    //    {
    //        sb.AppendLine($"{stock.StockCode}   {stock.StockName}   {stock.LatestDrop:F2}%   {stock.LatestClose:F2}");
    //    }

    //    //DisplayResult(sb.ToString());
    //}

    //private void DisplayResult(string result)
    //{
    //    ResultText.text = result;
    //}

    private void UpdateStatus(object arg)
    {
        StatusText.text = $"[{DateTime.Now:HH:mm:ss}] {Convert.ToString(arg)}";
    }
    private void OnFetchStart(object arg)
    {
        IsProcessing = true;
        SetButtonsInteractable(false);
        //ShowProgress(true);
    }
    private void OnFetchFin(object arg)
    {
        IsProcessing = false;
        SetButtonsInteractable(true);
        //ShowProgress(false);
    }
    

    private void ShowProgress(bool show)
    {
        ProgressPanel.SetActive(show);
        if (show)
        {
            ProgressSlider.value = 0f;
            ProgressText.text = "0%";
        }
    }

    private void SetButtonsInteractable(bool interactable)
    {
        FetchBasicButton.interactable = interactable;
        AvgButton.interactable = interactable;
        ConcentrationButton.interactable = interactable;
        FetchAll1DaysButton.interactable = interactable;
        FetchAll2DaysButton.interactable = interactable;
        FetchAll5DaysButton.interactable = interactable;
        FetchAll30DaysButton.interactable = interactable;
        FetchAll60DaysButton.interactable = interactable;
        UpdateAllDaysButton.interactable = interactable;
        FetchAll30WeeksButton.interactable = interactable;
        UpdateAllWeeksButton.interactable = interactable;
        FetchSingleStockButton.interactable = interactable;
        AnalyzeDrop5PercentButton.interactable = interactable;
    }

    private void UpdateProgress(float progress)
    {
        ProgressSlider.value = progress;
        ProgressText.text = $"{(progress * 100):F0}%";
    }
}