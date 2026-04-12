using ClientBase;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputMono : MonoBehaviour
{
    Camera mainCamera;
    Ray ray;
    RaycastHit hit;

    private Vector3 _LeftInputDown = Vector3.zero;//鼠标左击的起始位置

    private void Start()
    {
        mainCamera = GameObject.Find("/MainCamera").GetComponent<Camera>();
    }

    public void Update()
    {
        //Vector2 trans_MousePosition = new Vector3(Input.mousePosition.x / Screen.width * 1920, Input.mousePosition.y / Screen.height * 1080); 
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current.IsPointerOverGameObject())
            {
                //Debug.Log("是UI");
                return;
            }
            _LeftInputDown = Input.mousePosition;
            //Mgrs.Ins.minimapMgr.OnInputDown(ref _LeftInputDown);
        }
        else if (Input.GetMouseButton(0))
        {
            //if (EventSystem.current.IsPointerOverGameObject())
            //{
            //    //Debug.Log("是UI");
            //    return;
            //}
            //Mgrs.Ins.minimapMgr.OnInput(Input.mousePosition.x, Input.mousePosition.y);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            //if (Mgrs.Ins.minimapMgr.OnInputUp() == true)
            //{
            //    if (_LeftInputDown.x - Input.mousePosition.x < 0.3f && _LeftInputDown.x - Input.mousePosition.x  > -0.3f
            //        && _LeftInputDown.y - Input.mousePosition.y < 0.3f && _LeftInputDown.y - Input.mousePosition.y > -0.3f)
            //    {
            //        //Debug.Log("是左键单击");
            //        ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            //        if (Physics.Raycast(ray, out hit))
            //        {
            //            if (hit.transform.tag == "Player")//点击了野怪
            //            {
            //                UIDProxy uidProxy = hit.collider.GetComponent<UIDProxy>();
            //                if (uidProxy.roleType == 1)//点击了野怪
            //                {
            //                }
            //                else// 0//点击了Servent
            //                {
            //                    _selectHero.Clear();
            //                    _selectHeroId.Clear();
            //                    _selectHero.Add(hit.collider.gameObject);
            //                    _selectHeroId.Add(uidProxy.uid);
            //                    Mgrs.Ins.heroMgr.SelectHero(_selectHero, _selectHeroId);
            //                }
            //            }
            //        }
            //    }

            //    else
            //    {
            //        //Debug.Log("是左键框选");

            //        _selectHero.Clear();
            //        _selectHeroId.Clear();

            //        float x0; float x1; float y0; float y1;
            //        if (_LeftInputDown.x < Input.mousePosition.x)
            //        {
            //            x0 = _LeftInputDown.x;
            //            x1 = Input.mousePosition.x;
            //        }
            //        else
            //        {
            //            x1 = _LeftInputDown.x;
            //            x0 = Input.mousePosition.x;
            //        }
            //        if (_LeftInputDown.y < Input.mousePosition.y)
            //        {
            //            y0 = _LeftInputDown.y;
            //            y1 = Input.mousePosition.y;
            //        }
            //        else
            //        {
            //            y1 = _LeftInputDown.y;
            //            y0 = Input.mousePosition.y;
            //        }

            //        //Debug.Log(x0 + " " + y0+ " "+ x1 + " " + y1);
            //        List<int> allRoleUids = Mgrs.Ins.familyMgr.mainFamilyInfo.allRoleUids;
            //        //if (GameStatic.GetGameMode() == 0)
            //        //    allRoleUids = Mgrs.Ins.familyMgr.mainFamilyInfo.allRoleUids;
            //        //else if (GameStatic.GetGameMode() == 1)
            //        //    allRoleUids = Mgrs.Ins.towerMgr.towerFamilyInfo.allRoleUids;
            //        //else
            //        //    allRoleUids = new List<int>();
            //        for (int i = 0; i < allRoleUids.Count; i++)
            //        {
            //            if (allRoleUids[i] != -1)//塔防战里，会有-1
            //            {
            //                GameObject heroGo = Mgrs.Ins.heroMgr.allHeroGoInfoDic[allRoleUids[i]].go;
            //                Vector3 screenPos = CocoonTools.WorldToUIPoint(heroGo.transform.position) + UiOffset;
            //                //Debug.Log(screenPos);
            //                if (screenPos.x > x0 && screenPos.x < x1 && screenPos.y > y0 && screenPos.y < y1)
            //                {
            //                    //Debug.Log("在选框内");
            //                    _selectHero.Add(heroGo);
            //                    _selectHeroId.Add(allRoleUids[i]);
            //                }
            //            }

            //        }
            //        if (_selectHero.Count > 0)
            //        {
            //            Mgrs.Ins.heroMgr.SelectHero(_selectHero, _selectHeroId);
            //        }
            //    }
            //}
        }
        else if (Input.GetMouseButtonUp(1))
        {
            //if (EventSystem.current.IsPointerOverGameObject())
            //{
            //    //Debug.Log("是UI");
            //    CSharpHelper.Ins.RightClick();
            //    return;
            //}

            //ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            //if (Physics.Raycast(ray, out hit))
            //{
            //    Mgrs.Ins.gatherMgr.TryStopGather();
            //    if (hit.transform.tag == "Ground")//点击了地面
            //    {
            //        //Debug.Log("点击了地面");
            //        //if (GameStatic.gameMode == 0)
            //        Mgrs.Ins.heroMgr.NavSetDestination(hit.point);
            //        //else if (GameStatic.gameMode == 1)
            //        //    Mgrs.Ins.towerMgr.NavSetDestination(hit.point);
            //    }
            //    else if (hit.transform.tag == "Player")//点击了野怪
            //    {

            //        UIDProxy uidProxy = hit.collider.GetComponent<UIDProxy>();
            //        if (uidProxy.roleType == 1)//点击了野怪
            //        {
            //            //Debug.Log("点击了野怪 " + uidProxy.uid);
            //            //if (GameStatic.gameMode == 0)
            //            Mgrs.Ins.heroMgr.DesignativeAttack(uidProxy.uid);
            //            //else if (GameStatic.gameMode == 1)
            //            //    Mgrs.Ins.towerMgr.DesignativeAttack(uidProxy.uid);
            //        }
            //        else// 0//点击了Hero
            //        {
            //            //Debug.Log("点击了Hero " + uidProxy.uid);
            //            //if (GameStatic.gameMode == 0)
            //            Mgrs.Ins.heroMgr.MoveToHero(uidProxy.uid);
            //            //else if (GameStatic.gameMode == 1)
            //            //    Mgrs.Ins.towerMgr.MoveToHero(uidProxy.uid);
            //        }
            //    }
            //    else if (hit.transform.tag == "Npc")//点击了NPC
            //    {
            //        if (GameStatic.GetGameMode() == 0)
            //            Mgrs.Ins.familyMgr.mainFamilyInfo.MoveToNpc(int.Parse(hit.collider.gameObject.name));
            //        else if (GameStatic.GetGameMode() == 1)
            //        {

            //        }
            //        //Mgrs.Ins.towerMgr.MoveToNpc(int.Parse(hit.collider.gameObject.name));
            //    }
            //    else if (hit.transform.tag == "Gather")//点击了Gather
            //    {
            //        Mgrs.Ins.familyMgr.mainFamilyInfo.MoveToGather(int.Parse(hit.collider.gameObject.name));
            //    }
            //}
        }
        
        if (Input.GetKeyDown(KeyCode.Space))
        {
            //if (Mgrs.Ins.cropMgr.TryReapCrop() == true)
            //    return;
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            //CSharpHelper.Ins.OnEsc();
        }
        if (Input.GetKeyDown(KeyCode.M))
        {
            //CSharpHelper.Ins.FetchDlg("BigMapDlg", "");
        }
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.P))
        {
            //Mgrs.Ins.equipBagMgr.TestShow();
            //Mgrs.Ins.bagMgr.TestShow();
            //Mgrs.Ins.roleMgr.TestShow();
            //Mgrs.Ins.taskMgr.TestShow();
            //Mgrs.Ins.settingMgr.ChangeMainCamera();
            //------------------------
            //Color[] testColor = new Color[10];
            //for (int i = 0; i < testColor.Length; i++)
            //{
            //    testColor[i] = new Color(i, 0, i, i);

            //}
            //CheckDisBuffer.SetAllDistance(testColor);
            //---------------------------
            //TargetMgr.Ins.CreateDieTarget(Mgrs.Ins.familyMgr.mainFamilyInfo.allRoleUids[0]);
        }

#endif
        if (Time.timeScale != 0)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
            }
            else if (Input.GetKeyDown(KeyCode.F))
            {

            }
        }
        
    }
}
