using JetBrains.Annotations;
using UnityEngine;

namespace ClientBase
{
    public enum PlatformDefines
    {
        None,
        Editor,
        UseABEditor,
        IOS,
        Android,
        Windows

    }
    /// <summary>
    /// 文件管理类
    /// </summary>
    public static class FileUtility
    {
        public const string SettingsFolder = "GameSettings";
        public static FileUtilityBase Util { get; private set; }

        public static void SetMode(PlatformDefines platform, bool isEditorAndroid)
        {
            //Debug.Log(platform);
            if (platform == PlatformDefines.Editor || platform == PlatformDefines.UseABEditor)
                Util = new FileUtilityStandalone(isEditorAndroid);
            else if (platform == PlatformDefines.Android)
                Util = new FileUtilityAndroid();
            else if (platform == PlatformDefines.IOS)
                Util = new FileUtilityIOS();
            else if (platform == PlatformDefines.Windows)
                Util = new FileUtilityWindows();
            else
                Debug.LogError("没有这个平台" + platform);
        }
    }

    /// <summary>
    /// 文件工具基类
    /// </summary>
    public abstract class FileUtilityBase
    {
        protected string ResourceUrl;
        public void RegistResourceUrl(string resourceUrl)
        {
            ResourceUrl = resourceUrl;
        }

        /// <summary>
        /// 获取可持久化路径
        /// </summary>
        /// <returns></returns>
        public abstract string GetPersistencePath();

        /// <summary>
        /// 获取StreamingAsset文件路径
        /// </summary>
        /// <returns></returns>
        [CanBeNull]
        public abstract byte[] LoadBytesAtStreamingAssetsFolder(string relatedPath);

        /// <summary>
        /// 读取Preload地址
        /// </summary>
        /// <param name="includePrefix"></param>
        /// <returns></returns>
        [CanBeNull]
        public abstract byte[] LoadBytesAtPreloadFolder(string relatedPath);

        /// <summary>
        /// 读取固定的资源下载地址
        /// </summary>
        /// <returns></returns>
        public abstract string GetFixedResDownloadRootUrl();

        /// <summary>
        /// 获取StreamingAsset文件内容
        /// </summary>
        /// <param name="relatedPath"></param>
        /// <returns></returns>
        [CanBeNull]
        public string LoadTextAtStreamingAssetsFolder(string relatedPath)
        {            
            byte[] bytes = LoadBytesAtStreamingAssetsFolder(relatedPath);
            if (bytes != null)
                return System.Text.Encoding.UTF8.GetString(bytes);
            else
                return null;
        }

        /// <summary>
        /// Gets the preload path by WWW path.
        /// </summary>
        /// <param name="relatedPath">The related path.</param>
        /// <returns></returns>
        public abstract string GetPreloadPathByWWWPath(string relatedPath);
    }
}