using UnityEngine;

namespace ClientBase
{
    public class FileUtilityWindows : FileUtilityBase
    {
        public override string GetPersistencePath()
        {
            string path = Application.streamingAssetsPath + "/ABcatalog";
            FileHelper.CreateDirectoryByFile(path);
            return path;
        }

        public override byte[] LoadBytesAtStreamingAssetsFolder(string relatedPath)
        {
            var path = FileHelper.CombinePath(Application.streamingAssetsPath, relatedPath);
            if (FileHelper.FileExist(path))
                return FileHelper.FileReadAllBytes(path);
            else
                return null;
        }

        public override byte[] LoadBytesAtPreloadFolder(string relatedPath)
        {
            var path = FileHelper.CombinePath(Application.dataPath, relatedPath);
            if (FileHelper.FileExist(path))
                return FileHelper.FileReadAllBytes(path);
            else
                return null;
        }

        public override string GetFixedResDownloadRootUrl()
        {
            Assert.AssertStringNotNull(ResourceUrl);
            return ResourceUrl;
        }

        public override string GetPreloadPathByWWWPath(string relatedPath)
        {
            return FileHelper.CombinePath(Application.dataPath, relatedPath).AddWWWFilePrefix();
        }
    }
}