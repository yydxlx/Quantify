
using ClientBase;
using UnityEngine;

namespace Cocoon.Settings
{
    public static class ProjectSettings
    {
        public static bool isUpdate = false;
        public static bool isLua = true;
        public static PlatformDefines platform;
        /// <summary>
        /// 资源加载的方式
        /// </summary>
        public enum LoadResType
        {
            Local = 1,
            LoadFromServer = 2,
            LoadFromFile = 3,
        }
        [SerializeField]
        public static LoadResType resLoadType = LoadResType.Local; // 是否资源从本地读取
        public static bool enableUpdateMode = true;                 // 是否是更新模式
        [SerializeField] public static string Version = "1.0";
        [SerializeField] public static string ResServerUrl = "http://39.98.46.20/DataReport_iMonkey/api.php?c=Update.checkVersion&version=";
        [SerializeField] public static bool UseResServer = true;


        /// <summary>
        /// 初始化
        /// </summary>
        public static void Init(bool isEditorAndroid = true)
        {
            switch (Application.platform)
            {
                case RuntimePlatform.Android:
                    platform = PlatformDefines.Android;
                    break;
                case RuntimePlatform.IPhonePlayer:
                    platform = PlatformDefines.IOS;
                    break;
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.OSXEditor:
                case RuntimePlatform.LinuxEditor:
                    platform = PlatformDefines.Editor;
                    break;
                case RuntimePlatform.WindowsPlayer:
                    platform = PlatformDefines.Windows;
                    break;
                default:
                    Assert.Fail("Unexpected runtime platform: " + Application.platform);
                    return;
            }
            // 设置文件工具的类型
            FileUtility.SetMode(platform, isEditorAndroid);
        }
        /// <summary>
        /// 释放
        /// </summary>
        //internal static void Dispose()
        //{
        //    TimerClockManager.Ins.Dispose();
        //}
    }

}