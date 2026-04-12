using System.Collections.Generic;
using Cocoon.Settings;
using ClientBase;
//指定场景的数据管理类，暂且把UI层的管理类合并到此
public class Mgrs : Singleton<Mgrs>
{
    public List<MgrBase> mgrs = new List<MgrBase>();

    private int _FixedUpdate5Time = 5;
    private int _CurFixedUpdate5Time = 0;
    private int _FixedUpdate50Time = 50;
    private int _CurFixedUpdate50Time = 0;
    public TushareMgr tushareMgr;

    public StockDataMgr stockDataMgr;
    public BasicMgr basicMgr;
    public DailyDataMgr dailyDataMgr;
    public WeekDataMgr weekDataMgr;
    public ChipMgr chipMgr;
    //public StockDataWeekMgr stockDataWeekMgr;
    public void Init ()
    {
        tushareMgr = new TushareMgr();
        stockDataMgr = new StockDataMgr();
        basicMgr = new BasicMgr();
        dailyDataMgr = new DailyDataMgr();
        weekDataMgr = new WeekDataMgr();
        chipMgr = new ChipMgr();
        //stockDataWeekMgr = new StockDataWeekMgr();
        mgrs.Add(stockDataMgr);
        mgrs.Add(basicMgr);
        mgrs.Add(dailyDataMgr);
        mgrs.Add(weekDataMgr);
        mgrs.Add(chipMgr);
        //mgrs.Add(stockDataWeekMgr);
        mgrs.Add(tushareMgr);
        for (int i = 0; i < mgrs.Count; i++)
        {
            mgrs[i].Init();
        }
    }
    public void Update()
    {
        for (int i = 0; i < mgrs.Count; i++)
        {
            mgrs[i].Update();
        }
    }
    public void FixedUpdate()
    {
        for (int i = 0; i < mgrs.Count; i++)
        {
            mgrs[i].FixedUpdate();
        }
        if (_CurFixedUpdate50Time == 0)
        {

        }
        else if (_CurFixedUpdate50Time == 49)
        {

        }
        _CurFixedUpdate50Time++;
        if (_CurFixedUpdate50Time >= _FixedUpdate50Time)
        {
            _CurFixedUpdate50Time = 0;
        }

        if (_CurFixedUpdate5Time == 0)
        {
        }
        else if (_CurFixedUpdate5Time == 4)
        {
        }
        _CurFixedUpdate5Time++;
        if (_CurFixedUpdate5Time >= _FixedUpdate5Time)
        {
            _CurFixedUpdate5Time = 0;
        }
    }
   
    public void ReleaseAll()
    {
        stockDataMgr.Release();
        basicMgr.Release();
        dailyDataMgr.Release();
        weekDataMgr.Release();
        chipMgr.Release();
        //stockDataWeekMgr.Release();
        tushareMgr.Release();

    }
}
