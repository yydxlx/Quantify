using ClientBase;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class TushareMgr : MgrBase
{
    public const string API_URL = "http://api.tushare.pro";
    public const string TOKEN = "5e0369e3370196c1d5fa191a3f444bcf26bcde8309d93ee1db8db8b5";

    private static readonly object _lock = new object();
    private static int _activeRequests = 0;
    private const int MAX_CONCURRENT = 10;

    private async Task Throttle()
    {
        while (true)
        {
            lock (_lock)
            {
                if (_activeRequests < MAX_CONCURRENT)
                {
                    _activeRequests++;
                    break;
                }
            }
            await Task.Delay(100);
        }
    }

    private void ReleaseRequest()
    {
        lock (_lock) _activeRequests--;
    }

    public override void Init() { }

    public async Task<List<StockData>> GetStocksBasicInfo()
    {
        //await Throttle();
        //try
        //{
            var req = new TushareRequest
            {
                api_name = "stock_basic",
                token = TOKEN,
                parameters = new Dictionary<string, string> { { "exchange", "" }, { "list_status", "L" } },
                fields = "ts_code,name,market,industry,list_date"
            };

            using var uwr = CreateRequest(req);
            var tcs = new TaskCompletionSource<string>();
            uwr.SendWebRequest().completed += _ => OnRequestCompleted(uwr, tcs);
            string json = await tcs.Task;
            return ParseStockBasicInfo(json);
        //}
        //finally
        //{
        //    ReleaseRequest();
        //}
    }

    public List<StockData> ParseStockBasicInfo(string json)
    {
        var res = JsonConvert.DeserializeObject<TushareResponse>(json);
        var list = new List<StockData>();
        if (res.code != 0 || res.data == null) return list;
        var d = res.data;
        int ts = d.fields.IndexOf("ts_code");
        int name = d.fields.IndexOf("name");
        int market = d.fields.IndexOf("market");
        int industry = d.fields.IndexOf("industry");

        foreach (var item in d.items)
        {
            list.Add(new StockData
            {
                TsCode = item[ts]?.ToString(),
                Name = item[name]?.ToString(),
                Market = item[market]?.ToString(),
                Industry = item[industry]?.ToString(),
                StockDailyDataList = new List<StockDailyData>(),
                StockWeeklyDataList = new List<StockWeeklyData>()
            });
        }
        return list;
    }

    public async Task<Dictionary<string, StockData>> GetStocksHolderNumber()
    {
        await Throttle();
        try
        {
            var req = new TushareRequest
            {
                api_name = "stk_holdernumber",
                token = TOKEN,
                parameters = new Dictionary<string, string> { { "period", GetLatestQuarter() } },
                fields = "ts_code,ann_date,end_date,holder_num"
            };

            using var uwr = CreateRequest(req);
            var tcs = new TaskCompletionSource<string>();
            uwr.SendWebRequest().completed += _ => OnRequestCompleted(uwr, tcs);
            string json = await tcs.Task;
            return ParseStockHolderNumber(json);
        }
        finally
        {
            ReleaseRequest();
        }
    }

    public Dictionary<string, StockData> ParseStockHolderNumber(string json)
    {
        var res = JsonConvert.DeserializeObject<TushareResponse>(json);
        var dict = new Dictionary<string, StockData>();
        if (res.code != 0 || res.data == null) return dict;
        var d = res.data;
        int ts = d.fields.IndexOf("ts_code");
        int ann = d.fields.IndexOf("ann_date");
        int end = d.fields.IndexOf("end_date");
        int num = d.fields.IndexOf("holder_num");

        foreach (var item in d.items)
        {
            var s = new StockData
            {
                TsCode = item[ts]?.ToString(),
                AnnDate = item[ann]?.ToString(),
                EndDate = item[end]?.ToString(),
                HolderNum = Convert.ToDouble(item[num])
            };
            dict[s.TsCode] = s;
        }
        return dict;
    }

    public string GetLatestQuarter()
    {
        DateTime now = DateTime.Now;
        int q = (now.Month + 2) / 3;
        return $"{now.Year}Q{q}";
    }

    public async Task<List<StockDailyData>> GetDailyStocksByDate(string tradeDate)
    {
        //await Throttle();
        //try
        //{
            var req = new TushareRequest
            {
                api_name = "daily",
                token = TOKEN,
                parameters = new Dictionary<string, string> { { "trade_date", tradeDate } },
                fields = "ts_code,trade_date,open,high,low,close,pct_chg,vol,amount"
            };

            using var uwr = CreateRequest(req);
            var tcs = new TaskCompletionSource<string>();
            uwr.SendWebRequest().completed += _ => OnRequestCompleted(uwr, tcs);
            string json = await tcs.Task;
            return ParseDailyStocks(json, tradeDate);
        //}
        //finally
        //{
        //    ReleaseRequest();
        //}
    }

    public List<StockDailyData> ParseDailyStocks(string json, string tradeDate)
    {
        var res = JsonConvert.DeserializeObject<TushareResponse>(json);
        var list = new List<StockDailyData>();
        if (res.code != 0 || res.data == null) return list;
        var d = res.data;
        int ts = d.fields.IndexOf("ts_code");
        int dt = d.fields.IndexOf("trade_date");
        int o = d.fields.IndexOf("open");
        int h = d.fields.IndexOf("high");
        int l = d.fields.IndexOf("low");
        int c = d.fields.IndexOf("close");
        int pct = d.fields.IndexOf("pct_chg");
        int vol = d.fields.IndexOf("vol");
        int amt = d.fields.IndexOf("amount");

        foreach (var item in d.items)
        {
            string date = item[dt]?.ToString();
            if (date != tradeDate) continue;
            list.Add(new StockDailyData
            {
                TsCode = item[ts]?.ToString(),
                TradeDate = date,
                Open = Convert.ToDouble(item[o]),
                High = Convert.ToDouble(item[h]),
                Low = Convert.ToDouble(item[l]),
                Close = Convert.ToDouble(item[c]),
                PctChg = Convert.ToDouble(item[pct]),
                Vol = Convert.ToDouble(item[vol]),
                Amount = Convert.ToDouble(item[amt])
            });
        }
        return list;
    }

    public async Task<Dictionary<string, StockDailyData>> GetDailyBasicByDate(string tradeDate)
    {
        //await Throttle();
        //try
        //{
            var req = new TushareRequest
            {
                api_name = "daily_basic",
                token = TOKEN,
                parameters = new Dictionary<string, string> { { "trade_date", tradeDate } },
                fields = "ts_code,trade_date,pe,pe_ttm,total_mv"
            };

            using var uwr = CreateRequest(req);
            var tcs = new TaskCompletionSource<string>();
            uwr.SendWebRequest().completed += _ => OnRequestCompleted(uwr, tcs);
            string json = await tcs.Task;
            return ParseDailyBasic(json, tradeDate);
        //}
        //finally
        //{
        //    ReleaseRequest();
        //}
    }

    public Dictionary<string, StockDailyData> ParseDailyBasic(string json, string tradeDate)
    {
        var res = JsonConvert.DeserializeObject<TushareResponse>(json);
        var dict = new Dictionary<string, StockDailyData>();
        if (res.code != 0 || res.data == null) return dict;
        var d = res.data;
        int ts = d.fields.IndexOf("ts_code");
        int dt = d.fields.IndexOf("trade_date");
        int pe = d.fields.IndexOf("pe");
        int pet = d.fields.IndexOf("pe_ttm");
        int mv = d.fields.IndexOf("total_mv");

        foreach (var item in d.items)
        {
            string date = item[dt]?.ToString();
            if (date != tradeDate) continue;
            var s = new StockDailyData
            {
                TsCode = item[ts]?.ToString(),
                TradeDate = date,
                Pe = Convert.ToDouble(item[pe]),
                Pe_ttm = Convert.ToDouble(item[pet]),
                Total_mv = Convert.ToDouble(item[mv])
            };
            dict[s.TsCode] = s;
        }
        return dict;
    }

    public async Task<List<(double Price, double Percent)>> GetCyqChips(string tsCode, string tradeDate)
    {
        await Throttle();
        try
        {
            var req = new TushareRequest
            {
                api_name = "cyq_chips",
                token = TOKEN,
                parameters = new Dictionary<string, string> { { "ts_code", tsCode }, { "trade_date", tradeDate } },
                fields = "price,percent"
            };

            using var uwr = CreateRequest(req);
            var tcs = new TaskCompletionSource<string>();
            uwr.SendWebRequest().completed += _ => OnRequestCompleted(uwr, tcs);
            string json = await tcs.Task;
            var res = JsonConvert.DeserializeObject<TushareResponse>(json);
            var list = new List<(double, double)>();
            if (res.code == 0 && res.data?.items != null)
            {
                foreach (var item in res.data.items)
                {
                    if (item.Count < 2) continue;
                    list.Add((Convert.ToDouble(item[0]), Convert.ToDouble(item[1])));
                }
            }
            return list;
        }
        finally
        {
            ReleaseRequest();
        }
    }

    public async Task<List<StockWeeklyData>> GetWeeklyStocksByDate(string tradeDate)
    {
        await Throttle();
        try
        {
            var req = new TushareRequest
            {
                api_name = "weekly",
                token = TOKEN,
                parameters = new Dictionary<string, string> { { "trade_date", tradeDate } },
                fields = "ts_code,trade_date,open,high,low,close,pct_chg,vol,amount"
            };

            using var uwr = CreateRequest(req);
            var tcs = new TaskCompletionSource<string>();
            uwr.SendWebRequest().completed += _ => OnRequestCompleted(uwr, tcs);
            string json = await tcs.Task;
            return ParseWeeklyStocks(json, tradeDate);
        }
        finally
        {
            ReleaseRequest();
        }
    }

    public List<StockWeeklyData> ParseWeeklyStocks(string json, string tradeDate)
    {
        var res = JsonConvert.DeserializeObject<TushareResponse>(json);
        var list = new List<StockWeeklyData>();
        if (res.code != 0 || res.data == null) return list;
        var d = res.data;
        int ts = d.fields.IndexOf("ts_code");
        int dt = d.fields.IndexOf("trade_date");
        int o = d.fields.IndexOf("open");
        int h = d.fields.IndexOf("high");
        int l = d.fields.IndexOf("low");
        int c = d.fields.IndexOf("close");
        int pct = d.fields.IndexOf("pct_chg");
        int vol = d.fields.IndexOf("vol");
        int amt = d.fields.IndexOf("amount");

        var dict = new Dictionary<string, List<StockWeeklyData>>();
        foreach (var item in d.items)
        {
            string date = item[dt]?.ToString();
            if (date != tradeDate) continue;
            var s = new StockWeeklyData
            {
                TsCode = item[ts]?.ToString(),
                TradeDate = date,
                Open = Convert.ToDouble(item[o]),
                High = Convert.ToDouble(item[h]),
                Low = Convert.ToDouble(item[l]),
                Close = Convert.ToDouble(item[c]),
                PctChg = Convert.ToDouble(item[pct]),
                Vol = Convert.ToDouble(item[vol]),
                Amount = Convert.ToDouble(item[amt])
            };
            if (!dict.ContainsKey(s.TsCode)) dict[s.TsCode] = new List<StockWeeklyData>();
            dict[s.TsCode].Add(s);
        }

        foreach (var kv in dict)
        {
            CalcSingleStockMacdKdj(kv.Value);
            list.AddRange(kv.Value);
        }
        return list;
    }

    private void CalcSingleStockMacdKdj(List<StockWeeklyData> candles)
    {
        if (candles.Count == 0) return;
        const int SHORT = 12, LONG = 26, SIGNAL = 9, KDJ = 9;
        double emaS = 0, emaL = 0, dea = 0;
        double k = 50, d = 50;

        foreach (var c in candles)
        {
            if (emaS == 0) { emaS = c.Close; emaL = c.Close; }
            else
            {
                emaS = c.Close * 2.0 / (SHORT + 1) + emaS * (1 - 2.0 / (SHORT + 1));
                emaL = c.Close * 2.0 / (LONG + 1) + emaL * (1 - 2.0 / (LONG + 1));
            }
            double dif = emaS - emaL;
            dea = dea == 0 ? dif : dif * 2.0 / (SIGNAL + 1) + dea * (1 - 2.0 / (SIGNAL + 1));
            c.macdData = new MacdData { DIF = dif, DEA = dea, MACD = (dif - dea) * 2 };

            int i = candles.IndexOf(c);
            int start = Math.Max(0, i - KDJ + 1);
            double h = double.MinValue, l = double.MaxValue;
            for (int j = start; j <= i; j++)
            {
                h = Math.Max(h, candles[j].High);
                l = Math.Min(l, candles[j].Low);
            }
            double rsv = h == l ? 50 : (c.Close - l) / (h - l) * 100;
            k = k * 2 / 3 + rsv / 3;
            d = d * 2 / 3 + k / 3;
            c.kdjData = new KdjData { K = k, D = d, J = 3 * k - 2 * d };
        }
    }

    private UnityWebRequest CreateRequest(TushareRequest req)
    {
        string json = JsonConvert.SerializeObject(req);
        byte[] bytes = Encoding.UTF8.GetBytes(json);
        var uwr = new UnityWebRequest(API_URL, "POST");
        uwr.uploadHandler = new UploadHandlerRaw(bytes);
        uwr.downloadHandler = new DownloadHandlerBuffer();
        uwr.SetRequestHeader("Content-Type", "application/json");
        return uwr;
    }

    private void OnRequestCompleted(UnityWebRequest uwr, TaskCompletionSource<string> tcs)
    {
        try
        {
            if (uwr.result == UnityWebRequest.Result.Success)
                tcs.SetResult(uwr.downloadHandler.text);
            else
                tcs.SetException(new Exception(uwr.error));
        }
        catch (Exception e)
        {
            tcs.SetException(e);
        }
    }

    public override void Release() { }
}

[Serializable]
public class TushareRequest
{
    public string api_name;
    public string token;
    [JsonProperty("params")]
    public Dictionary<string, string> parameters;
    public string fields;
}

[Serializable]
public class TushareResponse
{
    public string request_id;
    public int code;
    public string msg;
    public TushareData data;
}

[Serializable]
public class TushareData
{
    public List<string> fields;
    public List<List<object>> items;
}