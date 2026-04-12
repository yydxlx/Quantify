using ClientBase;
using System.IO;
using UnityEngine;
//using YZL.Compress.UPK;
//

namespace Cocoon.Resource
{
    public class BundleUtility
    {
        public static string GetDownloadedPath(string resType, string name)
        {
            return FileUtility.Util.GetPersistencePath();
        }
        
        public static void DownloadSuccess(byte[] bytes, string name)
        {
            if (bytes == null)
                return;
            var filePath = FileHelper.CombinePath(FileUtility.Util.GetPersistencePath(), name);//(Downloaded)
            FileHelper.FileWriteAllBytes(filePath, bytes);
            //int packLength = 1024 * 20;
            //byte[] nbytes = new byte[packLength];

            //using (FileStream fs = new FileStream(FileUtility.Util.GetPersistencePath() + "/" + pIndex + ".png", FileMode.Create))
            //{
            //    int nReadSize = 0;
            //    using (Stream netStream = new MemoryStream(request.downloadHandler.data))
            //    {
            //        nReadSize = netStream.Read(nbytes, 0, packLength);
            //        while (nReadSize > 0)
            //        {
            //            fs.Write(nbytes, 0, nReadSize);
            //            nReadSize = netStream.Read(nbytes, 0, packLength);
            //        }
            //    }
            //    //File.Move(tempFile, suffixName);
            //}
        }

        //public static void DownloadAndCompress(byte[] bytes, string name)
        //{
        //    if (bytes == null)
        //        return;
        //    var filePath = FileHelper.CombinePath(FileUtility.Util.GetPersistencePath(), name);
        //    Debug.Log(filePath);
        //    FileHelper.FileWriteAllBytes(filePath, bytes);
        //    UPKFolder.UnPackFolderAsync(filePath, Application.dataPath.Substring(0, Application.dataPath.LastIndexOf("/Assets")) + "/ABDownload/", ShowProgress);
        //}
        //private static void ShowProgress(long all, long now)
        //{
            
        //    double progress = (double)now / all;
        //    Loom.Ins.QueueOnMainThread(() =>
        //    {
        //        EventManager.Ins.Emit("OnCompress_CompressPrgress", progress * 100);
        //    });
        //    if (progress == 1)
        //    {
        //        Loom.Ins.QueueOnMainThread(() =>
        //        {
        //            Debug.Log("发放解压完事件");
        //            EventManager.Ins.Emit("OnCompressFinish", null);
                    
        //        });
        //    }
            
        //}
    }
}