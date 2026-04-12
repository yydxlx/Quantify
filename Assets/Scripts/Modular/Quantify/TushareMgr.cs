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

    // 获取股票基础信息
    public async Task<List<StockData>> GetStocksBasicInfo()
    {
        var tcs = new TaskCompletionSource<List<StockData>>();
        
        // 在主线程中执行UnityWebRequest
        Loom.Ins.QueueOnMainThread(() =>
        {
            try
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

                using (UnityWebRequest webRequest = new UnityWebRequest(API_URL, "POST"))
                {
                    byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
                    webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
                    webRequest.downloadHandler = new DownloadHandlerBuffer();
                    webRequest.SetRequestHeader("Content-Type", "application/json");

                    var localTcs = new TaskCompletionSource<string>();
                    var asyncOp = webRequest.SendWebRequest();

                    asyncOp.completed += _ =>
                    {
                        if (webRequest.result == UnityWebRequest.Result.Success)
                        {
                            localTcs.SetResult(webRequest.downloadHandler.text);
                        }
                        else
                        {
                            localTcs.SetException(new Exception($"网络请求失败: {webRequest.error}"));
                        }
                    };

                    string jsonResponse = localTcs.Task.Result;
                    var result = ParseStockBasicInfo(jsonResponse);
                    tcs.SetResult(result);
                }
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
        });
        
        return await tcs.Task;
    }

    // 解析股票基础信息
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
            int listDateIndex = data.fields.IndexOf("list_date");

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

    // 获取股东人数信息
    public async Task<Dictionary<string, StockData>> GetStocksHolderNumber()
    {
        var tcs = new TaskCompletionSource<Dictionary<string, StockData>>();
        
        // 在主线程中执行UnityWebRequest
        Loom.Ins.QueueOnMainThread(() =>
        {
            try
            {
                // 获取最近一个季度的数据
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

                using (UnityWebRequest webRequest = new UnityWebRequest(API_URL, "POST"))
                {
                    byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
                    webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
                    webRequest.downloadHandler = new DownloadHandlerBuffer();
                    webRequest.SetRequestHeader("Content-Type", "application/json");

                    var localTcs = new TaskCompletionSource<string>();
                    var asyncOp = webRequest.SendWebRequest();

                    asyncOp.completed += _ =>
                    {
                        if (webRequest.result == UnityWebRequest.Result.Success)
                        {
                            localTcs.SetResult(webRequest.downloadHandler.text);
                        }
                        else
                        {
                            localTcs.SetException(new Exception($"网络请求失败: {webRequest.error}"));
                        }
                    };

                    string jsonResponse = localTcs.Task.Result;
                    var result = ParseStockHolderNumber(jsonResponse);
                    tcs.SetResult(result);
                }
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
        });
        
        return await tcs.Task;
    }

    // 解析股东人数信息
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

    // 获取最近一个季度
    public string GetLatestQuarter()
    {
        DateTime now = DateTime.Now;
        int year = now.Year;
        int month = now.Month;

        // 确定季度
        int quarter = (month - 1) / 3 + 1;
        return $"{year}0{quarter}"; // 格式：202403
    }

    // 获取某一天所有股票 daily 数据
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

        Debug.Log(tradeDate);
        string jsonData = JsonConvert.SerializeObject(request);

        // 2. 创建网络请求
        using (UnityWebRequest webRequest = new UnityWebRequest(API_URL, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");

            // 3. 【关键修复】兼容所有 Unity 版本的异步等待
            UnityWebRequestAsyncOperation asyncOp = webRequest.SendWebRequest();
            // 正确异步等待（不会卡主线程，无编译错误）
            while (!asyncOp.isDone)
            {
                await Task.Yield();
            }
            // 5. 解析并返回
            string jsonResponse = webRequest.downloadHandler.text;
            var result = ParseDailyStocks(jsonResponse, tradeDate);
            return result;
        }
    }

    // 解析 daily 数据
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
                // 只添加与传入日期相同的数据
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

    // 获取某一天所有股票 daily_basic 数据
    public async Task<Dictionary<string, StockDailyData>> GetDailyBasicByDate(string tradeDate)
    {
        var tcs = new TaskCompletionSource<Dictionary<string, StockDailyData>>();
        
        // 在主线程中执行UnityWebRequest
        Loom.Ins.QueueOnMainThread(() =>
        {
            try
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

                using (UnityWebRequest webRequest = new UnityWebRequest(API_URL, "POST"))
                {
                    byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
                    webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
                    webRequest.downloadHandler = new DownloadHandlerBuffer();
                    webRequest.SetRequestHeader("Content-Type", "application/json");

                    var localTcs = new TaskCompletionSource<string>();
                    var asyncOp = webRequest.SendWebRequest();

                    asyncOp.completed += _ =>
                    {
                        if (webRequest.result == UnityWebRequest.Result.Success)
                        {
                            localTcs.SetResult(webRequest.downloadHandler.text);
                        }
                        else
                        {
                            localTcs.SetException(new Exception($"网络请求失败: {webRequest.error}"));
                        }
                    };

                    string jsonResponse = localTcs.Task.Result;
                    var result = ParseDailyBasic(jsonResponse, tradeDate);
                    tcs.SetResult(result);
                }
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
        });
        
        return await tcs.Task;
    }

    // 解析 daily_basic 数据
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
                // 只添加与传入日期相同的数据
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

    /// <summary>
    /// 从 Tushare 获取筹码分布
    /// </summary>
    public async Task<List<(double Price, double Percent)>> GetCyqChips(string tsCode, string tradeDate)
    {
        var tcs = new TaskCompletionSource<List<(double Price, double Percent)>>();
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
        using (UnityWebRequest webRequest = new UnityWebRequest(API_URL, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");

            var localTcs = new TaskCompletionSource<string>();
            var asyncOp = webRequest.SendWebRequest();

            asyncOp.completed += _ =>
            {
                if (webRequest.result == UnityWebRequest.Result.Success)
                {
                    localTcs.SetResult(webRequest.downloadHandler.text);
                }
                else
                {
                    localTcs.SetException(new Exception($"网络请求失败: {webRequest.error}"));
                }
            };

            string jsonResponse = localTcs.Task.Result;
            var response = JsonConvert.DeserializeObject<TushareResponse>(jsonResponse);

            var list = new List<(double Price, double Percent)>();
            if (response.code == 0 && response.data.items != null)
            {
                foreach (var item in response.data.items)
                {
                    if (item.Count < 2) continue;
                    double p = Convert.ToDouble(item[0]);
                    double per = Convert.ToDouble(item[1]);
                    list.Add((p, per));
                }
            }
            tcs.SetResult(list);
        }

        return await tcs.Task;
    }

    // 获取某一天所有股票 weekly 数据
    public async Task<List<StockWeeklyData>> GetWeeklyStocksByDate(string tradeDate)
    {
        var tcs = new TaskCompletionSource<List<StockWeeklyData>>();
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
        using (UnityWebRequest webRequest = new UnityWebRequest(API_URL, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");

            var localTcs = new TaskCompletionSource<string>();
            var asyncOp = webRequest.SendWebRequest();

            asyncOp.completed += _ =>
            {
                if (webRequest.result == UnityWebRequest.Result.Success)
                {
                    localTcs.SetResult(webRequest.downloadHandler.text);
                }
                else
                {
                    localTcs.SetException(new Exception($"网络请求失败: {webRequest.error}"));
                }
            };

            string jsonResponse = localTcs.Task.Result;
            var result = ParseWeeklyStocks(jsonResponse);
            tcs.SetResult(result);
        }       
        return await tcs.Task;
    }

    // 解析 weekly 数据
    public List<StockWeeklyData> ParseWeeklyStocks(string jsonResponse)
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

            // MACD参数
            const int ShortPeriod = 12;
            const int LongPeriod = 26;
            const int SignalPeriod = 9;
            // KDJ参数
            const int KdjPeriod = 9;
            double lastK = 50, lastD = 50;

            List<double> closeList = new List<double>();
            List<double> highList = new List<double>();
            List<double> lowList = new List<double>();
            double lastEmaShort = 0;
            double lastEmaLong = 0;
            double lastDea = 0;
            double shortMultiplier = 2.0 / (ShortPeriod + 1);
            double longMultiplier = 2.0 / (LongPeriod + 1);
            double deaMultiplier = 2.0 / (SignalPeriod + 1);

            foreach (var item in data.items)
            {
                var stock = new StockWeeklyData
                {
                    TsCode = item[tsCodeIndex]?.ToString(),
                    TradeDate = item[tradeDateIndex]?.ToString(),
                    Open = Convert.ToDouble(item[openIndex]),
                    High = Convert.ToDouble(item[highIndex]),
                    Low = Convert.ToDouble(item[lowIndex]),
                    Close = Convert.ToDouble(item[closeIndex]),
                    PctChg = Convert.ToDouble(item[pctChgIndex]),
                    Vol = Convert.ToDouble(item[volIndex]),
                    Amount = Convert.ToDouble(item[amountIndex])
                };

                // MACD计算
                double close = stock.Close;
                closeList.Add(close);

                if (closeList.Count == 1)
                {
                    lastEmaShort = close;
                    lastEmaLong = close;
                }
                else
                {
                    lastEmaShort = (close - lastEmaShort) * shortMultiplier + lastEmaShort;
                    lastEmaLong = (close - lastEmaLong) * longMultiplier + lastEmaLong;
                }

                double dif = lastEmaShort - lastEmaLong;
                if (closeList.Count == 1)
                {
                    lastDea = dif;
                }
                else
                {
                    lastDea = (dif - lastDea) * deaMultiplier + lastDea;
                }
                double macd = dif - lastDea;

                stock.macdData = new MacdData
                {
                    DIF = dif,
                    DEA = lastDea,
                    MACD = macd
                };

                // KDJ计算
                highList.Add(stock.High);
                lowList.Add(stock.Low);

                int kdjStart = Math.Max(0, highList.Count - KdjPeriod);
                double periodHigh = double.MinValue;
                double periodLow = double.MaxValue;
                for (int i = kdjStart; i < highList.Count; i++)
                {
                    if (highList[i] > periodHigh) periodHigh = highList[i];
                    if (lowList[i] < periodLow) periodLow = lowList[i];
                }

                double rsv = (periodHigh == periodLow) ? 50 : ((stock.Close - periodLow) / (periodHigh - periodLow)) * 100;
                double k = (2.0 / 3.0) * lastK + (1.0 / 3.0) * rsv;
                double d = (2.0 / 3.0) * lastD + (1.0 / 3.0) * k;
                double j = 3 * k - 2 * d;

                stock.kdjData = new KdjData
                {
                    K = k,
                    D = d,
                    J = j
                };

                lastK = k;
                lastD = d;

                result.Add(stock);
            }
        }
        return result;
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