using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public class PythonFileMerger
{
    [MenuItem("Tools/合并py文件")]
    public static void MergerPY()
    {
        try
        {
            string targetDirectory = Path.Combine(Application.dataPath, "PY");

            // 验证目录是否存在
            if (!Directory.Exists(targetDirectory))
            {
                Console.WriteLine("错误：指定的目录不存在！");
                return;
            }

            // 获取输出文件路径（默认在当前目录下）
            string outputFilePath = Path.Combine(Application.dataPath, "PY/AllPythonFiles.txt");

            // 创建StringBuilder用于高效拼接文本
            StringBuilder allCode = new StringBuilder();

            // 递归查找所有.py文件并读取内容
            ProcessDirectory(targetDirectory, allCode);

            // 将所有内容写入输出文件
            File.WriteAllText(outputFilePath, allCode.ToString(), Encoding.UTF8);

            Console.WriteLine($"操作完成！所有Python文件内容已保存至：{outputFilePath}");
            Console.WriteLine("按任意键退出...");
            Console.ReadKey();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"发生错误：{ex.Message}");
            Console.WriteLine("按任意键退出...");
            Console.ReadKey();
        }
    }

    /// <summary>
    /// 递归处理目录中的所有文件和子目录
    /// </summary>
    static void ProcessDirectory(string directoryPath, StringBuilder allCode)
    {
        try
        {
            // 处理当前目录中的所有.py文件
            string[] pyFiles = Directory.GetFiles(directoryPath, "*.py");
            foreach (string file in pyFiles)
            {
                ProcessPythonFile(file, allCode);
            }

            // 递归处理所有子目录
            string[] subDirectories = Directory.GetDirectories(directoryPath);
            foreach (string subDir in subDirectories)
            {
                ProcessDirectory(subDir, allCode);
            }
        }
        catch (UnauthorizedAccessException ex)
        {
            Console.WriteLine($"无权访问目录 {directoryPath}：{ex.Message}");
        }
        catch (PathTooLongException ex)
        {
            Console.WriteLine($"路径过长 {directoryPath}：{ex.Message}");
        }
    }

    /// <summary>
    /// 处理单个Python文件，将其内容添加到字符串构建器
    /// </summary>
    static void ProcessPythonFile(string filePath, StringBuilder allCode)
    {
        try
        {
            // 添加文件路径作为分隔标识
            allCode.AppendLine("==================================================");
            allCode.AppendLine($"文件路径: {filePath}");
            allCode.AppendLine("--------------------------------------------------");

            // 读取文件内容并添加
            string fileContent = File.ReadAllText(filePath, Encoding.UTF8);
            allCode.AppendLine(fileContent);
            allCode.AppendLine(); // 添加空行分隔不同文件
        }
        catch (UnauthorizedAccessException ex)
        {
            Console.WriteLine($"无权访问文件 {filePath}：{ex.Message}");
        }
        catch (IOException ex)
        {
            Console.WriteLine($"读取文件 {filePath} 时出错：{ex.Message}");
        }
    }
}