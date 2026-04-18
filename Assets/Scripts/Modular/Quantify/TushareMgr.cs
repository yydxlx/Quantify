using ClientBase;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;

public class TushareMgr : MgrBase
{
    public const string API_URL = "http://api.tushare.pro";
    public const string TOKEN = "5e0369e3370196c1d5fa191a3f444bcf26bcde8309d93ee1db8db8b5";

    public override void Init()
    {
    }

    // ==================== 股票基础信息 ====================
    public async Task<List<StockData>> GetStocksBasicInfo()
    {
        var request = new TushareRequest
        {
            api_name = "stock_basic",
            token = TOKEN,
            parameters = new Dictionary<string, string>
            {
                { "exchange", "" },
                { "list_status", "L" }
            },
            fields = "ts_code,name,market,industry,list_date"
        };

        string jsonData = JsonConvert.SerializeObject(request);
        UnityWebRequest webRequest = new UnityWebRequest(API_URL, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);

        webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
        webRequest.downloadHandler = new DownloadHandlerBuffer();
        webRequest.SetRequestHeader("Content-Type", "application/json");

        var localTcs = new TaskCompletionSource<string>();
        var asyncOp = webRequest.SendWebRequest();

        asyncOp.completed += _ =>
        {
            if (webRequest.result == UnityWebRequest.Result.Success)
                localTcs.SetResult(webRequest.downloadHandler.text);
            else
                localTcs.SetException(new Exception($"网络请求失败: {webRequest.error}"));

            webRequest.Dispose();
        };

        string jsonResponse = await localTcs.Task;
        return ParseStockBasicInfo(jsonResponse);
    }

    public List<StockData> ParseStockBasicInfo(string jsonResponse)
    {
        var result = new List<StockData>();
        TushareResponse response = JsonConvert.DeserializeObject<TushareResponse>(jsonResponse);

        if (response.code == 0 && response.data != null)
        {
            var data = response.data;
            int tsCodeIndex = data.fields.IndexOf("ts_code");
            int nameIndex = data.fields.IndexOf("name");
            int marketIndex = data.fields.IndexOf("market");
            int industryIndex = data.fields.IndexOf("industry");

            foreach (var item in data.items)
            {
                var stock = new StockData
                {
                    TsCode = item[tsCodeIndex]?.ToString(),
                    Name = item[nameIndex]?.ToString(),
                    Market = item[marketIndex]?.ToString(),
                    Industry = item[industryIndex]?.ToString(),
                    StockDailyDataList = new List<StockDailyData>(),
                    StockWeeklyDataList = new List<StockWeeklyData>()
                };
                result.Add(stock);
            }
        }
        return result;
    }

    // ==================== 股东人数 ====================
    public async Task<Dictionary<string, StockData>> GetStocksHolderNumber()
    {
        string latestQuarter = GetLatestQuarter();

        var request = new TushareRequest
        {
            api_name = "stk_holdernumber",
            token = TOKEN,
            parameters = new Dictionary<string, string>
            {
                { "period", latestQuarter }
            },
            fields = "ts_code,ann_date,end_date,holder_num"
        };

        string jsonData = JsonConvert.SerializeObject(request);
        UnityWebRequest webRequest = new UnityWebRequest(API_URL, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);

        webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
        webRequest.downloadHandler = new DownloadHandlerBuffer();
        webRequest.SetRequestHeader("Content-Type", "application/json");

        var localTcs = new TaskCompletionSource<string>();
        var asyncOp = webRequest.SendWebRequest();

        asyncOp.completed += _ =>
        {
            if (webRequest.result == UnityWebRequest.Result.Success)
                localTcs.SetResult(webRequest.downloadHandler.text);
            else
                localTcs.SetException(new Exception($"网络请求失败: {webRequest.error}"));

            webRequest.Dispose();
        };

        string jsonResponse = await localTcs.Task;
        return ParseStockHolderNumber(jsonResponse);
    }

    public Dictionary<string, StockData> ParseStockHolderNumber(string jsonResponse)
    {
        var result = new Dictionary<string, StockData>();
        TushareResponse response = JsonConvert.DeserializeObject<TushareResponse>(jsonResponse);

        if (response.code == 0 && response.data != null)
        {
            var data = response.data;
            int tsCodeIndex = data.fields.IndexOf("ts_code");
            int annDateIndex = data.fields.IndexOf("ann_date");
            int endDateIndex = data.fields.IndexOf("end_date");
            int holderNumIndex = data.fields.IndexOf("holder_num");

            foreach (var item in data.items)
            {
                var stock = new StockData
                {
                    TsCode = item[tsCodeIndex]?.ToString(),
                    AnnDate = item[annDateIndex]?.ToString(),
                    EndDate = item[endDateIndex]?.ToString(),
                    HolderNum = Convert.ToDouble(item[holderNumIndex])
                };
                result[stock.TsCode] = stock;
            }
        }
        return result;
    }

    public string GetLatestQuarter()
    {
        DateTime now = DateTime.Now;
        int quarter = (now.Month + 2) / 3;
        return $"{now.Year}Q{quarter}";
    }

    // ==================== 每日行情 daily ====================
    public async Task<List<StockDailyData>> GetDailyStocksByDate(string tradeDate)
    {
        var request = new TushareRequest
        {
            api_name = "daily",
            token = TOKEN,
            parameters = new Dictionary<string, string>
            {
                { "trade_date", tradeDate }
            },
            fields = "ts_code,trade_date,open,high,low,close,pct_chg,vol,amount"
        };

        string jsonData = JsonConvert.SerializeObject(request);
        UnityWebRequest webRequest = new UnityWebRequest(API_URL, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);

        webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
        webRequest.downloadHandler = new DownloadHandlerBuffer();
        webRequest.SetRequestHeader("Content-Type", "application/json");

        var localTcs = new TaskCompletionSource<string>();
        var asyncOp = webRequest.SendWebRequest();

        asyncOp.completed += _ =>
        {
            if (webRequest.result == UnityWebRequest.Result.Success)
                localTcs.SetResult(webRequest.downloadHandler.text);
            else
                localTcs.SetException(new Exception($"网络请求失败: {webRequest.error}"));

            webRequest.Dispose();
        };
        //Debug.Log("111111111111");
        string jsonResponse = await localTcs.Task;
        //Debug.Log("22222222222222");
        return ParseDailyStocks(jsonResponse, tradeDate);
    }

    public List<StockDailyData> ParseDailyStocks(string jsonResponse, string tradeDate)
    {
        var result = new List<StockDailyData>();
        TushareResponse response = JsonConvert.DeserializeObject<TushareResponse>(jsonResponse);

        if (response.code == 0 && response.data != null)
        {
            var data = response.data;
            int tsCodeIndex = data.fields.IndexOf("ts_code");
            int tradeDateIndex = data.fields.IndexOf("trade_date");
            int openIndex = data.fields.IndexOf("open");
            int highIndex = data.fields.IndexOf("high");
            int lowIndex = data.fields.IndexOf("low");
            int closeIndex = data.fields.IndexOf("close");
            int pctChgIndex = data.fields.IndexOf("pct_chg");
            int volIndex = data.fields.IndexOf("vol");
            int amountIndex = data.fields.IndexOf("amount");

            foreach (var item in data.items)
            {
                string itemTradeDate = item[tradeDateIndex]?.ToString();
                if (itemTradeDate == tradeDate)
                {
                    var stock = new StockDailyData
                    {
                        TsCode = item[tsCodeIndex]?.ToString(),
                        TradeDate = itemTradeDate,
                        Open = Convert.ToDouble(item[openIndex]),
                        High = Convert.ToDouble(item[highIndex]),
                        Low = Convert.ToDouble(item[lowIndex]),
                        Close = Convert.ToDouble(item[closeIndex]),
                        PctChg = Convert.ToDouble(item[pctChgIndex]),
                        Vol = Convert.ToDouble(item[volIndex]),
                        Amount = Convert.ToDouble(item[amountIndex])
                    };
                    result.Add(stock);
                }
            }
        }
        return result;
    }

    // ==================== 每日指标 daily_basic ====================
    public async Task<Dictionary<string, StockDailyData>> GetDailyBasicByDate(string tradeDate)
    {
        var request = new TushareRequest
        {
            api_name = "daily_basic",
            token = TOKEN,
            parameters = new Dictionary<string, string>
            {
                { "trade_date", tradeDate }
            },
            fields = "ts_code,trade_date,pe,pe_ttm,total_mv"
        };

        string jsonData = JsonConvert.SerializeObject(request);
        UnityWebRequest webRequest = new UnityWebRequest(API_URL, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);

        webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
        webRequest.downloadHandler = new DownloadHandlerBuffer();
        webRequest.SetRequestHeader("Content-Type", "application/json");

        var localTcs = new TaskCompletionSource<string>();
        var asyncOp = webRequest.SendWebRequest();

        asyncOp.completed += _ =>
        {
            if (webRequest.result == UnityWebRequest.Result.Success)
                localTcs.SetResult(webRequest.downloadHandler.text);
            else
                localTcs.SetException(new Exception($"网络请求失败: {webRequest.error}"));

            webRequest.Dispose();
        };

        string jsonResponse = await localTcs.Task;
        return ParseDailyBasic(jsonResponse, tradeDate);
    }

    public Dictionary<string, StockDailyData> ParseDailyBasic(string jsonResponse, string tradeDate)
    {
        var result = new Dictionary<string, StockDailyData>();
        TushareResponse response = JsonConvert.DeserializeObject<TushareResponse>(jsonResponse);

        if (response.code == 0 && response.data != null)
        {
            TushareData data = response.data;
            int tsCodeIndex = data.fields.IndexOf("ts_code");
            int tradeDateIndex = data.fields.IndexOf("trade_date");
            int peIndex = data.fields.IndexOf("pe");
            int peTtmIndex = data.fields.IndexOf("pe_ttm");
            int totalMvIndex = data.fields.IndexOf("total_mv");

            foreach (List<object> item in data.items)
            {
                string itemTradeDate = item[tradeDateIndex]?.ToString();
                if (itemTradeDate == tradeDate)
                {
                    StockDailyData stock = new StockDailyData
                    {
                        TsCode = item[tsCodeIndex]?.ToString(),
                        TradeDate = itemTradeDate,
                        Pe = Convert.ToDouble(item[peIndex]),
                        Pe_ttm = Convert.ToDouble(item[peTtmIndex]),
                        Total_mv = Convert.ToDouble(item[totalMvIndex])
                    };
                    result[stock.TsCode] = stock;
                }
            }
        }
        return result;
    }

    // ==================== 筹码分布 cyq_chips ====================
    public async Task<List<(double Price, double Percent)>> GetCyqChips(string tsCode, string tradeDate)
    {
        var request = new TushareRequest
        {
            api_name = "cyq_chips",
            token = TOKEN,
            parameters = new Dictionary<string, string>
            {
                { "ts_code", tsCode },
                { "trade_date", tradeDate }
            },
            fields = "price,percent"
        };

        string jsonData = JsonConvert.SerializeObject(request);
        UnityWebRequest webRequest = new UnityWebRequest(API_URL, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);

        webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
        webRequest.downloadHandler = new DownloadHandlerBuffer();
        webRequest.SetRequestHeader("Content-Type", "application/json");

        var localTcs = new TaskCompletionSource<string>();
        var asyncOp = webRequest.SendWebRequest();

        asyncOp.completed += _ =>
        {
            if (webRequest.result == UnityWebRequest.Result.Success)
                localTcs.SetResult(webRequest.downloadHandler.text);
            else
                localTcs.SetException(new Exception($"网络请求失败: {webRequest.error}"));

            webRequest.Dispose();
        };

        string jsonResponse = await localTcs.Task;
        var response = JsonConvert.DeserializeObject<TushareResponse>(jsonResponse);
        var list = new List<(double, double)>();

        if (response.code == 0 && response.data?.items != null)
        {
            foreach (var item in response.data.items)
            {
                if (item.Count < 2) continue;
                list.Add((Convert.ToDouble(item[0]), Convert.ToDouble(item[1])));
            }
        }
        return list;
    }

    // ==================== 周线 weekly ====================
    public async Task<List<StockWeeklyData>> GetWeeklyStocksByDate(string tradeDate)
    {
        var request = new TushareRequest
        {
            api_name = "weekly",
            token = TOKEN,
            parameters = new Dictionary<string, string>
            {
                { "trade_date", tradeDate }
            },
            fields = "ts_code,trade_date,open,high,low,close,pct_chg,vol,amount"
        };

        string jsonData = JsonConvert.SerializeObject(request);
        UnityWebRequest webRequest = new UnityWebRequest(API_URL, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);

        webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
        webRequest.downloadHandler = new DownloadHandlerBuffer();
        webRequest.SetRequestHeader("Content-Type", "application/json");

        var localTcs = new TaskCompletionSource<string>();
        var asyncOp = webRequest.SendWebRequest();

        asyncOp.completed += _ =>
        {
            if (webRequest.result == UnityWebRequest.Result.Success)
                localTcs.SetResult(webRequest.downloadHandler.text);
            else
                localTcs.SetException(new Exception($"网络请求失败: {webRequest.error}"));

            webRequest.Dispose();
        };

        string jsonResponse = await localTcs.Task;
        return ParseWeeklyStocks(jsonResponse, tradeDate);
    }

    public List<StockWeeklyData> ParseWeeklyStocks(string jsonResponse, string tradeDate)
    {
        var result = new List<StockWeeklyData>();
        TushareResponse response = JsonConvert.DeserializeObject<TushareResponse>(jsonResponse);

        if (response.code == 0 && response.data != null)
        {
            var data = response.data;
            int tsCodeIndex = data.fields.IndexOf("ts_code");
            int tradeDateIndex = data.fields.IndexOf("trade_date");
            int openIndex = data.fields.IndexOf("open");
            int highIndex = data.fields.IndexOf("high");
            int lowIndex = data.fields.IndexOf("low");
            int closeIndex = data.fields.IndexOf("close");
            int pctChgIndex = data.fields.IndexOf("pct_chg");
            int volIndex = data.fields.IndexOf("vol");
            int amountIndex = data.fields.IndexOf("amount");

            var stockDict = new Dictionary<string, List<StockWeeklyData>>();

            foreach (var item in data.items)
            {
                string itemDate = item[tradeDateIndex]?.ToString();
                if (itemDate != tradeDate) continue;

                var stock = new StockWeeklyData
                {
                    TsCode = item[tsCodeIndex]?.ToString(),
                    TradeDate = itemDate,
                    Open = Convert.ToDouble(item[openIndex]),
                    High = Convert.ToDouble(item[highIndex]),
                    Low = Convert.ToDouble(item[lowIndex]),
                    Close = Convert.ToDouble(item[closeIndex]),
                    PctChg = Convert.ToDouble(item[pctChgIndex]),
                    Vol = Convert.ToDouble(item[volIndex]),
                    Amount = Convert.ToDouble(item[amountIndex])
                };

                if (!stockDict.ContainsKey(stock.TsCode))
                    stockDict[stock.TsCode] = new List<StockWeeklyData>();

                stockDict[stock.TsCode].Add(stock);
            }

            foreach (var kv in stockDict)
            {
                CalcSingleStockMacdKdj(kv.Value);
                result.AddRange(kv.Value);
            }
        }
        return result;
    }

    // 单只股票正确计算 MACD + KDJ
    private void CalcSingleStockMacdKdj(List<StockWeeklyData> candles)
    {
        if (candles.Count == 0) return;

        const int SHORT = 12, LONG = 26, SIGNAL = 9;
        const int KDJ_PERIOD = 9;

        double emaS = 0, emaL = 0, dea = 0;
        double k = 50, d = 50;

        foreach (var c in candles)
        {
            // MACD
            if (emaS == 0) { emaS = c.Close; emaL = c.Close; }
            else
            {
                emaS = c.Close * (2.0 / (SHORT + 1)) + emaS * (1 - 2.0 / (SHORT + 1));
                emaL = c.Close * (2.0 / (LONG + 1)) + emaL * (1 - 2.0 / (LONG + 1));
            }

            double dif = emaS - emaL;
            dea = dea == 0 ? dif : dif * (2.0 / (SIGNAL + 1)) + dea * (1 - 2.0 / (SIGNAL + 1));
            double macd = (dif - dea) * 2;

            c.macdData = new MacdData { DIF = dif, DEA = dea, MACD = macd };

            // KDJ
            int idx = candles.IndexOf(c);
            int start = Math.Max(0, idx - KDJ_PERIOD + 1);
            double h = double.MinValue, l = double.MaxValue;

            for (int i = start; i <= idx; i++)
            {
                h = Math.Max(h, candles[i].High);
                l = Math.Min(l, candles[i].Low);
            }

            double rsv = h == l ? 50 : (c.Close - l) / (h - l) * 100;
            k = k * 2 / 3 + rsv * 1 / 3;
            d = d * 2 / 3 + k * 1 / 3;
            double j = 3 * k - 2 * d;

            c.kdjData = new KdjData { K = k, D = d, J = j };
        }
    }

    public override void Release()
    {
    }
}

[System.Serializable]
public class TushareRequest
{
    public string api_name;
    public string token;
    [JsonProperty("params")]
    public System.Collections.Generic.Dictionary<string, string> parameters;
    public string fields;
}

[System.Serializable]
public class TushareResponse
{
    public string request_id;
    public int code;
    public string msg;
    public TushareData data;
}

[System.Serializable]
public class TushareData
{
    public List<string> fields;
    public List<List<object>> items;
}