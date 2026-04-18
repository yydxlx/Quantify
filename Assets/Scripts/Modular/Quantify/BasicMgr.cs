using ClientBase;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class BasicMgr : MgrBase
{
    private string BasicDataDirectory = "AllBasicData";
    private string basicFullPath;

    public override void Init()
    {
        // 计算Assets同级目录的AllData路径
        string allDataPath = Path.Combine(Path.GetDirectoryName(Application.dataPath), "AllData");
        basicFullPath = Path.Combine(allDataPath, BasicDataDirectory);
        if (!Directory.Exists(basicFullPath))
            Directory.CreateDirectory(basicFullPath);
    }

    private string GetBasicFilePath(string tsCode)
    {
        return Path.Combine(basicFullPath, $"{tsCode}.json");
    }

    // 获取所有股票基础数据（只保存基础信息到AllBasicData）
    public async void GetAllStocksBasicInfo()
    {
        EventManager.Ins.Emit("UpdateStatus", "获取所有股票基础数据...");
        EventManager.Ins.Emit("OnFetchStart");
        //try
        //{
            List<StockData> stockBasicList = await Mgrs.Ins.tushareMgr.GetStocksBasicInfo();
            Dictionary<string, StockData> holderNumberDict = await Mgrs.Ins.tushareMgr.GetStocksHolderNumber();
            MergeStockBasicData(stockBasicList, holderNumberDict);

            SaveStocksBasicInfoSeparated(stockBasicList);

            EventManager.Ins.Emit("UpdateStatus", $"基础数据获取完成！共 {stockBasicList.Count} 只股票");
            EventManager.Ins.Emit("OnFetchFin");
        //}
        //catch (Exception ex)
        //{
        //    Debug.LogError($"获取股票基础数据失败: {ex.Message}");
        //    EventManager.Ins.Emit("UpdateStatus", $"获取失败: {ex.Message}");
        //    EventManager.Ins.Emit("OnFetchFin");
        //}
    }

    // 保存基础信息到AllBasicData（不含每日数据）
    private void SaveStocksBasicInfoSeparated(List<StockData> stockBasicList)
    {
        foreach (var stock in stockBasicList)
        {
            string filePath = GetBasicFilePath(stock.TsCode);
            string json = JsonConvert.SerializeObject(stock);
            File.WriteAllText(filePath, json, System.Text.Encoding.UTF8);
        }
    }

    // 合并基础数据和股东人数数据
    private void MergeStockBasicData(List<StockData> stockBasicList, Dictionary<string, StockData> holderNumberDict)
    {
        foreach (var stock in stockBasicList)
        {
            if (holderNumberDict.TryGetValue(stock.TsCode, out var holderData))
            {
                stock.AnnDate = holderData.AnnDate;
                stock.EndDate = holderData.EndDate;
                stock.HolderNum = holderData.HolderNum;
            }
            else
            {
                // 如果没有股东人数数据，设置为默认值
                stock.AnnDate = "";
                stock.EndDate = "";
                stock.HolderNum = 0;
            }
        }
    }

    // 读取基础信息（AllBasicData）
    public StockData LoadBasicStockDataFromFile(string filePath)
    {
        if (!File.Exists(filePath))
            return new StockData();
        string json = File.ReadAllText(filePath, System.Text.Encoding.UTF8);
        StockData stockData = JsonConvert.DeserializeObject<StockData>(json);
        return stockData;
    }

    // 获取所有已保存的股票代码
    public List<StockData> LoadAllBasicStockCodes()
    {
        string[] files = Directory.GetFiles(basicFullPath, "*.json");
        List<StockData> allStockData = new List<StockData>();
        for (int i = 0; i < files.Length; i++)
        {
            allStockData.Add(LoadBasicStockDataFromFile(files[i]));
        }
        return allStockData;
    }

    public override void Release()
    {

    }

}