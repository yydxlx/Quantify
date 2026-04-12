using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace CocoonAsset.Editor
{
    public abstract class BuildRule
    {
        protected static List<string> packedAssets = new List<string>();
        protected static List<AssetBundleBuild> builds = new List<AssetBundleBuild>();
        private static List<BuildRule> rules = new List<BuildRule>();
        private static Dictionary<string, List<string>> allDependencies = new Dictionary<string, List<string>>();

        public static List<AssetBundleBuild> GetBuilds(string manifestPath)
        {
            packedAssets.Clear();
            builds.Clear();
            rules.Clear();
            allDependencies.Clear();

            AssetBundleBuild build = new AssetBundleBuild();
            build.assetBundleName = "manifest";
            build.assetNames = new string[] { manifestPath };
            builds.Add(build);

            const string rulesini = "Assets/Rules.txt";
            if (File.Exists(rulesini))
            {
                LoadRules(rulesini);
            }
            else
            { 
                Debug.LogError("没锟斤拷Rules.txt");
                rules.Add(new BuildAssetsWithFilename("Assets/SampleAssets", "*.prefab", SearchOption.AllDirectories));
                SaveRules(rulesini);
            }

            foreach (BuildRule item in rules)
            {
                List<string> files = _GetFilesWithoutDirectories(item.searchPath, item.searchPattern, item.searchOption);//rules锟秸刚讹拷锟斤拷Rules锟侥憋拷锟斤拷bulids只锟斤拷锟斤拷锟斤拷mainfest锟斤拷build
                CollectDependencies(files);//allDependencies为files锟侥憋拷锟斤拷锟斤拷锟斤拷锟叫憋拷
            }
            //BuildDependenciesAssets();//锟窖憋拷锟斤拷锟斤拷锟斤拷锟斤拷1锟斤拷锟斤拷锟斤拷源锟斤拷锟诫到List<AssetBundleBuild> builds锟斤拷

            foreach (BuildRule item in rules)
            {
                item.Build();
            }

#if ENABLE_ATLAS
			BuildAtlas(); 
#endif
            UnityEditor.EditorUtility.ClearProgressBar();

            return builds;
        }

        static void BuildAtlas()
        {
            Debug.Log("BuildAtlas");
            foreach (AssetBundleBuild item in builds)
            {
                string[] assets = item.assetNames;
                foreach (string asset in assets)
                {
                    AssetImporter importer = AssetImporter.GetAtPath(asset);
                    if (importer is TextureImporter)
                    {
                        TextureImporter ti = importer as TextureImporter;
                        if (ti.textureType == TextureImporterType.Sprite)
                        {
                            Texture tex = AssetDatabase.LoadAssetAtPath<Texture>(asset);
                            if (tex.texelSize.x >= 1024 || tex.texelSize.y >= 1024)
                            {
                                continue;
                            }

                            string tag = item.assetBundleName.Replace("/", "_");
                            if (! tag.Equals(ti.spritePackingTag))
                            {
                                TextureImporterPlatformSettings settings = ti.GetPlatformTextureSettings(AssetBundleUtility.GetPlatformName());
                                settings.format = ti.GetAutomaticFormat(AssetBundleUtility.GetPlatformName());
                                settings.overridden = true;
                                ti.SetPlatformTextureSettings(settings);
                                ti.spritePackingTag = tag;
                                ti.SaveAndReimport();
                            }
                        }
                    }
                }
 
            }
        }

        static void SaveRules(string rulesini)
        {
            using (var s = new StreamWriter(rulesini))
            {
                foreach (BuildRule item in rules)
                {
                    s.WriteLine("[{0}]", item.GetType().Name);
                    s.WriteLine("searchPath=" + item.searchPath);
                    s.WriteLine("searchPattern=" + item.searchPattern);
                    s.WriteLine("searchOption=" + item.searchOption);
                    s.WriteLine("bundleName=" + item.bundleName);
                    s.WriteLine();
                }
                s.Flush();
                s.Close();
            }
        }

        static void LoadRules(string rulesini)
        {
            using (var s = new StreamReader(rulesini))
            {
                rules.Clear();

                string line = null;
                while ((line = s.ReadLine()) != null)
                {
                    if (line == string.Empty || line.StartsWith("#", StringComparison.CurrentCulture) || line.StartsWith("//", StringComparison.CurrentCulture))
                    {
                        continue;
                    }
                    if (line.Length > 2 && line[0] == '[' && line[line.Length - 1] == ']')
                    {
                        var name = line.Substring(1, line.Length - 2);//BuildAssetsWithAssetBundleName
                        var searchPath = s.ReadLine().Split('=')[1];
                        var searchPattern = s.ReadLine().Split('=')[1];
                        var searchOption = s.ReadLine().Split('=')[1];
                        var bundleName = s.ReadLine().Split('=')[1];
                        Type type = typeof(BuildRule).Assembly.GetType("CocoonAsset.Editor." + name);
                        if (type != null)
                        {
                            var rule = Activator.CreateInstance(type) as BuildRule;
                            rule.searchPath = searchPath;
                            rule.searchPattern = searchPattern;
                            rule.searchOption = (SearchOption)Enum.Parse(typeof(SearchOption), searchOption);
                            rule.bundleName = bundleName;
                            rules.Add(rule);
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 锟斤拷取路锟斤拷锟斤拷指锟斤拷锟斤拷锟酵碉拷锟斤拷锟斤拷锟侥硷拷锟斤拷
        /// </summary>
        /// <param name="prefabPath"></param>
        /// <param name="searchPattern"></param>
        /// <param name="searchOption"></param>
        /// <returns></returns>
        public static List<string> _GetFilesWithoutDirectories(string prefabPath, string searchPattern, SearchOption searchOption)
        {
            string[] files = Directory.GetFiles(prefabPath, searchPattern, searchOption);
            List<string> items = new List<string>();
            foreach (string item in files)
            {
                if (item.EndsWith(".meta", StringComparison.CurrentCulture))
                    continue;
                string assetPath = item.Replace('\\', '/');
                if (!Directory.Exists(assetPath))//锟斤拷锟斤拷卸锟矫诧拷锟矫伙拷锟斤拷茫锟街憋拷锟絘dd也锟斤拷
                {
                    items.Add(assetPath);
                }
            }
            //Assets/SampleAssets/Logo.prefab
            //Assets/SampleAssets/UnityLogo.png
            return items;
        }

        protected static void BuildDependenciesAssets()
        {
            Dictionary<string, List<string>> bundles = new Dictionary<string, List<string>>();
            foreach (KeyValuePair<string, List<string>> item in allDependencies)
            {
                string assetPath = item.Key;
                if (!assetPath.EndsWith(".cs", StringComparison.CurrentCulture))
                {
                    if (packedAssets.Contains(assetPath))
                    {
                        continue;
                    }
                    if (assetPath.EndsWith(".shader", StringComparison.CurrentCulture))//shader锟斤拷锟斤拷弄一锟斤拷bundle
                    {
                        List<string> list = null;
                        if (!bundles.TryGetValue("shaders", out list))
                        {
                            list = new List<string>();
                            bundles.Add("shaders", list);
                        }
                        if (!list.Contains(assetPath))
                        {
                            list.Add(assetPath);
                            packedAssets.Add(assetPath);
                        }
                    }
                    else
                    {
                        if (item.Value.Count > 1)
                        {
                            //Debug.Log(assetPath);
                            //Assets/ABResources/Scene0/BG/logo.png
                            //Assets/ABResources/Scene0/BG/pangxieshouyeBG.png
                            //Assets/ABResources/Scene0/BG/锟斤拷_锟斤拷锟斤拷锟斤拷锟?...png

                            var name = "shared/" + BuildAssetBundleNameWithAssetPath(Path.GetDirectoryName(assetPath));
                            //Debug.Log("name " + name);
                            //shared/assets/abresources/scene0/bg
                            List<string> list = null;
                            if (!bundles.TryGetValue(name, out list))
                            {
                                list = new List<string>();
                                bundles.Add(name, list);
                            }
                            if (!list.Contains(assetPath))
                            {
                                list.Add(assetPath);
                                packedAssets.Add(assetPath);
                            }
                            // bundles = Dictionary<string, List<string>>
                            //{
                            //      shared/assets/abresources/scene0/bg = List<string>()
                            //      {
                            //          Assets/ABResources/Scene0/BG/logo.png
                            //          Assets/ABResources/Scene0/BG/pangxieshouyeBG.png
                            //          Assets/ABResources/Scene0/BG/锟斤拷_锟斤拷锟斤拷锟斤拷锟?...png
                            //      }
                            //}

                        }
                    }
                }
            }
            foreach (var item in bundles)
            {
                AssetBundleBuild build = new AssetBundleBuild();
                build.assetBundleName = item.Key + ".ab";// + item.Value.Count;
                build.assetNames = item.Value.ToArray();
                builds.Add(build);
            }
        }

        protected static List<string> GetDependenciesWithoutShared(string item)
        {
            var assets = AssetDatabase.GetDependencies(item);
            List<string> assetNames = new List<string>();
            foreach (var assetPath in assets)
            {
                if (assetPath.Contains(".prefab") || assetPath.Equals(item) || packedAssets.Contains(assetPath) || assetPath.EndsWith(".cs", StringComparison.CurrentCulture) || assetPath.EndsWith(".shader", StringComparison.CurrentCulture))
                {
                    continue;
                }
                if (allDependencies[assetPath].Count == 1)
                {
                    assetNames.Add(assetPath);
                }
            }
            return assetNames;
        }

        protected static void CollectDependencies(List<string> files)
        {
            //假如files来源于A文件夹，文件夹里a1，a2两个资源，a1依赖其他文件夹的b1,c1
            for (int i = 0; i < files.Count; i++)//files:a1,a2
            {
                string item = files[i];//item:a1
                string[] dependencies = AssetDatabase.GetDependencies(item);//锟斤拷锟斤拷锟斤拷源路锟斤拷锟斤拷锟斤拷锟斤拷锟斤拷锟斤拷锟皆达拷锟斤拷锟斤拷锟斤拷锟斤拷锟斤拷锟皆绰凤拷锟斤拷锟斤拷锟斤拷锟斤拷锟斤拷锟?                //dependencies:a1,b1,c1
                //没锟斤拷锟斤拷 锟斤拷锟斤拷锟斤拷
                //if (UnityEditor.EditorUtility.DisplayCancelableProgressBar(string.Format("Collecting... [{0}/{1}]", i, files.Count), item, i * 1f / files.Count))
                //{
                //    break;
                //}

                foreach (string assetPath in dependencies)//assetPath锟斤拷a1锟斤拷始
                {
                    if (!allDependencies.ContainsKey(assetPath))
                    {
                        allDependencies[assetPath] = new List<string>();
                    }

                    if (!allDependencies[assetPath].Contains(item))
                    {
                        allDependencies[assetPath].Add(item);
                    }
                }
            }
            // allDependencies = Dictionary<string, List<string>>
            //{
            //  a1 = List<string>()
            //      {
            //          a1
            //      }
            //  b1 = List<string>()
            //      {
            //          a1
            //      }
            //  c1 = List<string>()
            //      {
            //          a1
            //      }
            //  a2 = List<string>()
            //      {
            //          a2
            //      }
            //}
        }

        protected static List<string> GetFilesWithoutPacked(string searchPath, string searchPattern, SearchOption searchOption)
        {
            List<string> files = _GetFilesWithoutDirectories(searchPath, searchPattern, searchOption);
            int filesCount = files.Count;
            int removeAll = files.RemoveAll((string obj) =>//锟斤拷之前shared锟斤拷锟斤拷锟绞斤拷锟斤拷锟皆达拷锟絝iles锟叫憋拷锟斤拷删锟斤拷
            {
                return packedAssets.Contains(obj);
            });
            Debug.Log(string.Format("RemoveAll {0} size: {1}", removeAll, filesCount));

            return files;
        }

        protected static string BuildAssetBundleNameWithAssetPath(string assetPath)
        {
            return Path.Combine(Path.GetDirectoryName(assetPath), Path.GetFileNameWithoutExtension(assetPath)).Replace('\\', '/').ToLower();
        }

        public string searchPath;
        public string searchPattern;
        public SearchOption searchOption = SearchOption.AllDirectories;
        public string bundleName;


        protected BuildRule()
        {

        }

        protected BuildRule(string path, string pattern, SearchOption option)
        {
            searchPath = path;
            searchPattern = pattern;
            searchOption = option;
        }

        public abstract void Build();

        public abstract string GetAssetBundleName(string assetPath);
    }

    public class BuildAssetsWithAssetBundleName : BuildRule
    {
        public BuildAssetsWithAssetBundleName()
        {

        }

        public override string GetAssetBundleName(string assetPath)
        {
            return bundleName;
        }

        public BuildAssetsWithAssetBundleName(string path, string pattern, SearchOption option, string assetBundleName) : base(path, pattern, option)
        {
            bundleName = assetBundleName;
        }

        public override void Build()
        {
            var files = GetFilesWithoutPacked(searchPath, searchPattern, searchOption);
            List<string> list = new List<string>();
            foreach (var item in files)
            {
                list.AddRange(GetDependenciesWithoutShared(item));
            }
            files.AddRange(list);
            AssetBundleBuild build = new AssetBundleBuild();
            build.assetBundleName = bundleName;
            build.assetNames = files.ToArray();
            builds.Add(build);
            packedAssets.AddRange(files);
        }
    }

    public class BuildAssetsWithDirectroyName : BuildRule
    {
        public BuildAssetsWithDirectroyName()
        {

        }

        public BuildAssetsWithDirectroyName(string path, string pattern, SearchOption option) : base(path, pattern, option)
        {
        }

        public override string GetAssetBundleName(string assetPath)
        {
            return BuildAssetBundleNameWithAssetPath(Path.GetDirectoryName(assetPath));
        }

        public override void Build()
        {
            //List<string> files = GetFilesWithoutPacked(searchPath, searchPattern, searchOption);
            List<string> files = _GetFilesWithoutDirectories(searchPath, searchPattern, searchOption);
            Dictionary<string, List<string>> bundles = new Dictionary<string, List<string>>();
            for (int i = 0; i < files.Count; i++)
            {
                string item = files[i];
                if (UnityEditor.EditorUtility.DisplayCancelableProgressBar(string.Format("Collecting... [{0}/{1}]", i, files.Count), item, i * 1f / files.Count))
                {
                    break;
                }
                string path = Path.GetDirectoryName(item);
                if (!bundles.ContainsKey(path))
                {
                    bundles[path] = new List<string>();
                }
                bundles[path].Add(item);
                bundles[path].AddRange(GetDependenciesWithoutShared(item));
            }

            int count = 0;
            foreach (KeyValuePair<string, List<string>> item in bundles)
            {
                AssetBundleBuild build = new AssetBundleBuild();
                build.assetBundleName = BuildAssetBundleNameWithAssetPath(item.Key) + ".ab";// + item.Value.Count;
                build.assetNames = item.Value.ToArray();
                packedAssets.AddRange(build.assetNames);
                builds.Add(build);
                if (UnityEditor.EditorUtility.DisplayCancelableProgressBar(string.Format("Packing... [{0}/{1}]", count, bundles.Count), build.assetBundleName, count * 1f / bundles.Count))
                {
                    break;
                }
                count++;
            }
        }
    }

    public class BuildAssetsWithFilename : BuildRule
    {
        public BuildAssetsWithFilename()
        {

        }

        public override string GetAssetBundleName(string assetPath)
        {
            return BuildAssetBundleNameWithAssetPath(assetPath);
        }

        public BuildAssetsWithFilename(string path, string pattern, SearchOption option) : base(path, pattern, option)
        {
        }

        public override void Build()
        {
            var files = GetFilesWithoutPacked(searchPath, searchPattern, searchOption);

            for (int i = 0; i < files.Count; i++)
            {
                var item = files[i];
                if (UnityEditor.EditorUtility.DisplayCancelableProgressBar(string.Format("Packing... [{0}/{1}]", i, files.Count), item, i * 1f / files.Count))
                {
                    break;
                }
                AssetBundleBuild build = new AssetBundleBuild();
                build.assetBundleName = BuildAssetBundleNameWithAssetPath(item);
                var assetNames = GetDependenciesWithoutShared(item);
                assetNames.Add(item);
                build.assetNames = assetNames.ToArray();
                packedAssets.AddRange(assetNames);
                builds.Add(build);
            }
        }
    }

	public class BuildAssetsWithScenes : BuildRule
    {
#region implemented abstract members of BuildRule

		public override string GetAssetBundleName (string assetPath)
		{
            return BuildAssetBundleNameWithAssetPath(assetPath);
		}

#endregion

        public BuildAssetsWithScenes()
        {

        }

        public BuildAssetsWithScenes(string path, string pattern, SearchOption option) : base(path, pattern, option)
        {

        }

        public override void Build()
        {
            var files = GetFilesWithoutPacked(searchPath, searchPattern, searchOption);

            for (int i = 0; i < files.Count; i++)
            {
                var item = files[i];
                if (UnityEditor.EditorUtility.DisplayCancelableProgressBar(string.Format("Packing... [{0}/{1}]", i, files.Count), item, i * 1f / files.Count))
                {
                    break;
                }
                AssetBundleBuild build = new AssetBundleBuild();
                build.assetBundleName = BuildAssetBundleNameWithAssetPath(item); 
                build.assetNames = new [] { item };
                packedAssets.AddRange(build.assetNames);
                builds.Add(build);
            }
        }
    }

}