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
        FindUIComponents();
    }

    private void FindUIComponents()
    {
        DateInputField = transform.Find("InputGroup/DateInput").GetComponent<InputField>();
        SingleStockInput = transform.Find("InputGroup/StockInput").GetComponent<InputField>();
        DaysInputField = transform.Find("InputGroup/DaysInput").GetComponent<InputField>();

        DataFetchToggle = transform.Find("toggleGroup/DataFetchToggle").GetComponent<Toggle>();
        DataAnalyzeToggle = transform.Find("toggleGroup/DataAnalyzeToggle").GetComponent<Toggle>();

        DataFetchPanel = transform.Find("DataFetchPanel").gameObject;
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

        StatusText = transform.Find("StatusText").GetComponent<Text>();

        ProgressPanel = transform.Find("ProgressGroup").gameObject;
        ProgressSlider = transform.Find("ProgressGroup/ProgressSlider").GetComponent<Slider>();
        ProgressText = transform.Find("ProgressGroup/ProgressText").GetComponent<Text>();

        DataAnalyzePanel = transform.Find("DataAnalyzePanel").gameObject;
        OversoldReboundButton = transform.Find("DataAnalyzePanel/OversoldReboundButton").GetComponent<Button>();
        ExtremeWashoutButton = transform.Find("DataAnalyzePanel/ExtremeWashoutButton").GetComponent<Button>();
        ConcentrationAnalyzeButton = transform.Find("DataAnalyzePanel/ConcentrationAnalyzeButton").GetComponent<Button>();

        ResultScrollRect = transform.Find("DataAnalyzePanel/ResultPanel/ResultContent").GetComponent<ScrollRect>();
        ResultContent = transform.Find("DataAnalyzePanel/ResultPanel/ResultContent/Viewport/Content");
        ResultText = transform.Find("DataAnalyzePanel/ResultPanel/ResultContent/Viewport/Content/Text").GetComponent<Text>();
    }

    void Start()
    {
        DateInputField.text = DateTime.Now.ToString("yyyyMMdd");
        SingleStockInput.text = "000001.SZ";
        DaysInputField.text = "30";

        BindEvents();
        InitializeUI();
    }

    private void OnEnable()
    {
        EventManager.Ins.Regist("UpdateStatus", UpdateStatus);
        EventManager.Ins.Regist("OnFetchStart", OnFetchStart);
        EventManager.Ins.Regist("OnFetchFin", OnFetchFin);
    }

    private void OnDisable()
    {
        EventManager.Ins.Unregist("UpdateStatus", UpdateStatus);
        EventManager.Ins.Unregist("OnFetchStart", OnFetchStart);
        EventManager.Ins.Unregist("OnFetchFin", OnFetchFin);
    }

    private void BindEvents()
    {
        DataFetchToggle.onValueChanged.AddListener(OnDataFetchToggleChanged);
        DataAnalyzeToggle.onValueChanged.AddListener(OnDataAnalyzeToggleChanged);

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
    }

    private void InitializeUI()
    {
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
        }
    }

    private void OnDataAnalyzeToggleChanged(bool isOn)
    {
        if (isOn)
        {
            DataFetchPanel.SetActive(false);
            DataAnalyzePanel.SetActive(true);
        }
    }

    private void OnFetchBasicClick()
    {
        if (IsProcessing) return;
        Mgrs.Ins.basicMgr.GetAllStocksBasicInfo();
    }

    private void OnAvgButtonClick()
    {
        if (IsProcessing) return;
        Mgrs.Ins.dailyDataMgr.CalculateAndUpdateAverageData();
    }

    private void OnFetchAll1DaysClick() => OnFetchAllDaysClick(1);
    private void OnFetchAll2DaysClick() => OnFetchAllDaysClick(2);
    private void OnFetchAll5DaysClick() => OnFetchAllDaysClick(5);
    private void OnFetchAll30DaysClick() => OnFetchAllDaysClick(30);
    private void OnFetchAll60DaysClick() => OnFetchAllDaysClick(60);

    private void OnUpdateAllDaysClick()
    {
        if (IsProcessing) return;
        Mgrs.Ins.dailyDataMgr.UpdateAllDailyStocksRecursive();
    }

    private void OnConcentrationButtonClick()
    {
        if (IsProcessing) return;
        Mgrs.Ins.chipMgr.GetAllStocksChipPeakData();
    }

    private void OnFetchAll30WeeksClick() => OnFetchAllWeeksClick(30);

    private void OnUpdateAllWeeksClick()
    {
        if (IsProcessing) return;
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
        var sortedData = TechnicalAnalysisHelper.AnalyzeConcentration();
        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"筹码集中度分析结果（共{sortedData.Count}只）");
        sb.AppendLine("======================================");
        sb.AppendLine("代码\t\t名字\t\tPE-TTM\t\t市值(亿)\t板块\t90集中度\t70集中度\t主峰价");
        sb.AppendLine("--------------------------------------");

        foreach (var item in sortedData)
        {
            sb.AppendLine($"{item.Stock.TsCode}\t{item.Stock.Name}\t{item.DailyData.Pe_ttm:F1}\t{item.DailyData.Total_mv:F1}\t{item.Stock.Market}\t{item.DailyData.Concentration90:F1}\t{item.DailyData.Concentration70:F1}\t{item.DailyData.ConcentrationPrice:F2}");
        }

        ResultText.text = sb.ToString();
    }

    public void ShowSelectedStocks()
    {
        List<SelectedStock> selectedStocksList = Mgrs.Ins.stockDataMgr.selectedStocksList;
        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"筛选结果（共{selectedStocksList.Count}只）");
        sb.AppendLine("======================================");
        sb.AppendLine("代码\t\t名字\t\tPE\t\t市值(亿)\t市场\t得分");
        sb.AppendLine("--------------------------------------");

        foreach (var s in selectedStocksList)
        {
            var d = s.stockData.StockDailyDataList[0];
            sb.AppendLine($"{s.stockData.TsCode}\t{s.stockData.Name}\t{d.Pe_ttm:F1}\t{d.Total_mv:F1}\t{s.stockData.Market}\t{s.oversoldScore:F1}");
        }

        ResultText.text = sb.ToString();
    }

    // 🔥 适配并发版接口
    private async void OnFetchAllDaysClick(int days)
    {
        if (IsProcessing) return;
        await Mgrs.Ins.dailyDataMgr.GetAllStocksLastNDaysAsync(days);
    }

    private async void OnFetchAllWeeksClick(int weeks)
    {
        if (IsProcessing) return;
        await Mgrs.Ins.weekDataMgr.GetAllStocksLastNWeeksAsync(weeks);
    }

    private void UpdateStatus(object arg)
    {
        StatusText.text = $"[{DateTime.Now:HH:mm:ss}] {Convert.ToString(arg)}";
    }

    private void OnFetchStart(object arg)
    {
        IsProcessing = true;
        SetButtonsInteractable(false);
    }

    private void OnFetchFin(object arg)
    {
        IsProcessing = false;
        SetButtonsInteractable(true);
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
    }

    private void UpdateProgress(float progress)
    {
        ProgressSlider.value = progress;
        ProgressText.text = $"{(progress * 100):F0}%";
    }
}