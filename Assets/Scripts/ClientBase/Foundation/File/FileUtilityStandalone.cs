using UnityEngine;

namespace ClientBase
{
    public class FileUtilityStandalone : FileUtilityBase
    {
        private bool isEditorAndroid;
        public FileUtilityStandalone(bool isEditorAndroid)
        {
            this.isEditorAndroid = isEditorAndroid;
        }

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
            //string path = FileHelper.CombinePath(
            //    Application.dataPath.Substring(0, Application.dataPath.Length - 7) + "/build/",
            //    isEditorAndroid ? ClientBaseConst.EditorPreloadPathAndroid : ClientBaseConst.EditorPreloadPathIOS);
            //var fileName = FileHelper.CombinePath(path, relatedPath);
            var fileName = GetPreloadPathByWWWPath(relatedPath);
            if (FileHelper.FileExist(fileName))
                return FileHelper.FileReadAllBytes(fileName);
            else
                return null;
        }

        public override string GetFixedResDownloadRootUrl()
        {
            var path = FileHelper.CombinePath(Application.dataPath.Substring(0, Application.dataPath.Length - 7),
                ClientBaseConst.EditorBuildFolder);
            return isEditorAndroid
                ? FileHelper.CombinePath(path, ClientBaseConst.EditorNetResPathAndroid)
                : FileHelper.CombinePath(path, ClientBaseConst.EditorNetResPathIOS);
        }

        public override string GetPreloadPathByWWWPath(string relatedPath)
        {
            string path = FileHelper.CombinePath(
                Application.dataPath.Substring(0, Application.dataPath.Length - 7) + "/build/",
                isEditorAndroid ? ClientBaseConst.EditorPreloadPathAndroid : ClientBaseConst.EditorPreloadPathIOS);
            return FileHelper.CombinePath(path, relatedPath);
        }
    }
}