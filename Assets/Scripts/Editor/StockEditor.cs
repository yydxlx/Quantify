using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public class StockEditor : MonoBehaviour
{
    [MenuItem("Tools/数据清理/删除 2026-2.json 文件", false, 10)]
    public static void Delete2026_2JsonFiles()
    {
        // 确定 dailyFullPath 路径
        string allDataPath = Path.Combine(Path.GetDirectoryName(Application.dataPath), "AllData");
        string dailyFullPath = Path.Combine(allDataPath, "AllDailyData");
        
        if (!Directory.Exists(dailyFullPath))
        {
            Debug.LogError($"Directory not found: {dailyFullPath}");
            return;
        }
        
        Debug.Log($"Searching for 2026-2.json files in {dailyFullPath}");
        
        // 搜索所有 2026-2.json 文件
        string[] files = Directory.GetFiles(dailyFullPath, "2026-2.json", SearchOption.AllDirectories);
        
        int deletedCount = 0;
        foreach (string file in files)
        {
            try
            {
                File.Delete(file);
                Debug.Log($"Deleted: {file}");
                deletedCount++;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error deleting file {file}: {e.Message}");
            }
        }
        
        Debug.Log($"Deleted {deletedCount} files.");
    }
}
