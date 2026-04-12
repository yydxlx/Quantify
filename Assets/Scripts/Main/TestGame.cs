using UnityEngine;
using Cocoon.Settings;
using ClientBase.Coroutine;
using Cocoon.Auido;
using ClientBase;
using CocoonAsset;

using UnityEngine.UI;

public class TestGame : MonoBehaviour 
{
    void Start()
    {
        Application.targetFrameRate = 60;
        DontDestroyOnLoad(gameObject);
        //Loom.Init();
        AudioMgr.Ins.Init();
        PlayerConsole.Init();
        ResourceMgr.Ins.Init();
        Mgrs.Ins.Init();
    }
    void Update()
    {
        Loom.Ins.Update();
        ResourceMgr.Ins.Update();
    }
    void FixedUpdate()
    {
        TimeManager.Ins.FixedUpdate();
    }
}
