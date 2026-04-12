using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ClientBase
{
    public sealed class ClientBaseConst
    {
        public const string DownloadRecordFileName = "downloadRecord.bytes";
        public const string VersionFilename = "version.bytes";    // 版本管理文件
        public const string EditorBuildFolder = "build";

        public const string EditorNetResPathAndroid = "Cocoon_Android_NetRes";
        public const string EditorNetResPathIOS = "Cocoon_IOS_NetRes";
        public const string EditorPreloadPathAndroid = "Cocoon_Android_Preload";
        public const string EditorPreloadPathIOS = "Cocoon_IOS_Preload";

        public const int MaxTryCountDownload = 3;
        public const float DownloadTimeout = 15.0f;     // 下载超时时间

        public const string NameGlobals = "__CocoonGlobals__";

    }
}