using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using ClientBase;
//using ClientBase.Coroutine;
//using BehaviorDesigner.Runtime;
using UnityEngine.U2D;
using Object = UnityEngine.Object;

namespace CocoonAsset
{
    public sealed class ResourceMgr : Singleton<ResourceMgr>
    {
        public Dictionary<string, GameObject> prefabDic = new Dictionary<string, GameObject>();//预加载prefab
        //public Dictionary<string, ExternalBehavior> treeDic = new Dictionary<string, ExternalBehavior>();//预加载prefab
        Manifest manifest = new Manifest();
        public string[] allAssetNames { get { return manifest.allAssets; } }
        public string[] allBundleNames { get { return manifest.allBundles; } }
        public string GetBundleName(string assetPath) {
            return manifest.GetBundleName(assetPath);
        }
        public string GetAssetName(string assetPath) { return manifest.GetAssetName(assetPath); }
        readonly List<Asset> assets = new List<Asset>();
        //private Dictionary<string, Object[]> atlasDic; //图集的集合
        private Dictionary<string, SpriteAtlas> atlasDic;

        public bool Init()
        {
            atlasDic = new Dictionary<string, SpriteAtlas>();

#if UNITY_EDITOR
            if (AssetBundleUtility.ActiveBundleMode) 
            {
                return InitializeBundle();
            }
            return true;
#else
			return InitializeBundle();
#endif

        }
        public void DelFun1()
        {
            Bundle bundle = BundleMgr.Ins.Load(GetBundleName("Assets/ABResources/Material/HomeGrass.mat"));
            Object[] allAsset = bundle.assetBundle.LoadAllAssets();
            for (int i = 0; i < allAsset.Length; i++)
            {
                Material aa = allAsset[i] as Material;
                if (aa != null)
                {
                    aa.shader = Shader.Find(aa.shader.name);
                    Debug.Log(aa.shader.name);
                    Debug.Log(aa.shader);
                    Debug.Log(Shader.Find("Custom/TerrainLitGrass"));
                    if (aa.name == "HomeGrass")
                    {
                        aa.shader = Shader.Find("Custom/TerrainLitGrass");
                    }
                }
            }
        }
        public void LoadPrePrefab(System.Action<GameObject> callBack = null)//以后改成异步预加载,按场景分类模型，加载到场景的时候才加载对应的模型prefab
        {
            // Debug.Log("添加prefab完毕");
            //prefabDic.Add("Player", Load<GameObject>("BehaviorAssets/Prefab/Player.prefab"));
            //prefabDic.Add("Role1", Load("BehaviorAssets/1/1.prefab", typeof(GameObject)) as GameObject);
            //prefabDic.Add("Role2", Load("BehaviorAssets/Prefab/Role2.prefab", typeof(GameObject)) as GameObject);
            //prefabDic.Add("Role3", Load("BehaviorAssets/Prefab/Role3.prefab", typeof(GameObject)) as GameObject);
            //prefabDic.Add("Role4", Load("BehaviorAssets/Prefab/Role4.prefab", typeof(GameObject)) as GameObject);
            //prefabDic.Add("Role5", Load("BehaviorAssets/Prefab/Role5.prefab", typeof(GameObject)) as GameObject);
            //prefabDic.Add("Role6", Load("BehaviorAssets/Prefab/Role6.prefab", typeof(GameObject)) as GameObject);
            //prefabDic.Add("Npc1", Load("BehaviorAssets/Prefab/Npc1.prefab", typeof(GameObject)) as GameObject);
            //prefabDic.Add("Npc2", Load("BehaviorAssets/Prefab/Npc1.prefab", typeof(GameObject)) as GameObject);
            //prefabDic.Add("Npc3", Load("BehaviorAssets/Prefab/Npc1.prefab", typeof(GameObject)) as GameObject);
            //prefabDic.Add("Npc4", Load("BehaviorAssets/Prefab/Npc1.prefab", typeof(GameObject)) as GameObject);
            //prefabDic.Add("Npc5", Load("BehaviorAssets/Prefab/Npc1.prefab", typeof(GameObject)) as GameObject);
            //treeDic.Add("Hero", Load("BehaviorAssets/TreeAssets/PlayerBehavior.asset", typeof(ExternalBehavior)) as ExternalBehavior);
            prefabDic.Add("Hud", Load("Prefabs/Hud/Hud.prefab", typeof(GameObject)) as GameObject);
            prefabDic.Add("NpcHud", Load("Prefabs/Hud/NpcHud.prefab", typeof(GameObject)) as GameObject);
            prefabDic.Add("TransporterHud", Load("Prefabs/Hud/TransporterHud.prefab", typeof(GameObject)) as GameObject);
            prefabDic.Add("Transporter", Load("SkillEffect/Ornament/TransporterBlue.prefab", typeof(GameObject)) as GameObject);
            prefabDic.Add("Drop", Load("SkillEffect/Ornament/Drop.prefab", typeof(GameObject)) as GameObject);
            prefabDic.Add("TaskPos", Load("SkillEffect/Ornament/TaskPos.prefab", typeof(GameObject)) as GameObject);
            prefabDic.Add("RoleShadow", Load("Prefabs/Hud/RoleShadow.prefab", typeof(GameObject)) as GameObject);
            prefabDic.Add("SelectRole", Load("Prefabs/Hud/SelectRole.prefab", typeof(GameObject)) as GameObject);

            prefabDic.Add("AddAtk", Load<GameObject>("SkillEffect/Buff/AddAtk.prefab"));
            prefabDic.Add("MinusAtk", Load<GameObject>("SkillEffect/Buff/MinusAtk.prefab"));
            prefabDic.Add("AddDef", Load<GameObject>("SkillEffect/Buff/AddDef.prefab"));
            prefabDic.Add("MinusDef", Load<GameObject>("SkillEffect/Buff/MinusDef.prefab"));
            prefabDic.Add("AddmAtk", Load<GameObject>("SkillEffect/Buff/AddmAtk.prefab"));
            prefabDic.Add("MinusmAtk", Load<GameObject>("SkillEffect/Buff/MinusmAtk.prefab"));
            prefabDic.Add("AddmDef", Load<GameObject>("SkillEffect/Buff/AddmDef.prefab"));
            prefabDic.Add("MinusmDef", Load<GameObject>("SkillEffect/Buff/MinusmDef.prefab"));
            prefabDic.Add("AddAtkSpeed", Load<GameObject>("SkillEffect/Buff/AddAtkSpeed.prefab"));
            prefabDic.Add("MinusAtkSpeed", Load<GameObject>("SkillEffect/Buff/MinusAtkSpeed.prefab"));
            //常驻资源
            Load<GameObject>("NewResources/BlazingHighlands/Prefabs/Foliage/SM_Flowers_A_01.prefab");
            Load<GameObject>("NewResources/BlazingHighlands/Prefabs/Foliage/SM_Flowers_B_01.prefab");
            Load<GameObject>("NewResources/BlazingHighlands/Prefabs/Foliage/SM_Flowers_C_01.prefab"); 
            Load<GameObject>("NewResources/BlazingHighlands/Prefabs/Foliage/SM_Flowers_D_01.prefab");
            Load<GameObject>("NewResources/BlazingHighlands/Prefabs/Foliage/SM_Grass_01.prefab");
            Load<GameObject>("NewResources/BlazingHighlands/Prefabs/Foliage/SM_Grass_02.prefab");
            Load<GameObject>("NewResources/BlazingHighlands/Prefabs/Foliage/SM_Grass_Gaomu.prefab");
            Load<GameObject>("NewResources/BlazingHighlands/Prefabs/Foliage/SM_Grass_Gutian.prefab");
            Load<GameObject>("NewResources/BlazingHighlands/Prefabs/Foliage/SM_Grass_Gutian2.prefab");
            Load<GameObject>("NewResources/BlazingHighlands/Prefabs/Foliage/SM_Grass_Heyin.prefab");
            Load<GameObject>("NewResources/BlazingHighlands/Prefabs/Foliage/SM_Grass_Hongye.prefab");
            Load<GameObject>("NewResources/BlazingHighlands/Prefabs/Foliage/SM_Grass_Mingshui.prefab");
            Load<GameObject>("NewResources/BlazingHighlands/Prefabs/Foliage/SM_Tree_01.prefab");
            Load<GameObject>("NewResources/BlazingHighlands/Prefabs/Foliage/SM_Tree_02.prefab");
        }

        public T Load<T>(string path) where T : Object
        {
            //if (typeof(T) == typeof(Sprite))
            //{
            //    return LoadSprite(path) as T;
            //}
            //else
            //{
                path = "Assets/ABResources/" + path;
                return LoadInternal(path, typeof(T), false).asset as T;
            //}
        }
        public Object Load(string path, System.Type type) 
        {
            path = "Assets/ABResources/" + path;
            return LoadInternal(path, type, false).asset;
        }

        //public static T LoadAsync<T>(string path) where T : Object
        //{
        //    path = "Assets/ABResources/" + path;
        //    return LoadAsync(path, typeof(T)).asset as T;
        //}

        //public Asset LoadAsync(string path)
        //{
        //    path = "Assets/ABResources/" + path;
        //    return LoadInternal(path, true);
        //}
        public Asset LoadAsync(string path, System.Type type, System.Action<Asset> onFin)
        {
            path = "Assets/ABResources/" + path; 
            Asset asset = assets.Find(obj => { return obj.assetPath == path; });
            if (asset == null)
            {

#if UNITY_EDITOR
                if (AssetBundleUtility.ActiveBundleMode)
                {
                    asset = new BundleAssetAsync(path, type);
                    (asset as BundleAssetAsync).completed += onFin;
                    assets.Add(asset);
                    asset.references++;
                    asset.OnLoad();
                }
                else
                {
                    
                    asset = new Asset(path, type);
                    assets.Add(asset);
                    asset.references++;
                    asset.OnLoad();
                    onFin?.Invoke(asset);
                }
#else
                asset = new BundleAssetAsync(path, type);
                (asset as BundleAssetAsync).completed += onFin;
                assets.Add(asset);
                asset.references++;
                asset.OnLoad();
#endif
            }
            else if (asset.asset == null)//如果是第二次异步加载 但是第一次异步加载还没加载完
            {
                asset.references++;
                (asset as BundleAssetAsync).completed += onFin;
            }
            else
            {
                asset.references++;
                onFin?.Invoke(asset);
            }
            //asset.references++;
            return asset as Asset;
        }

        public void Unload(Asset asset)
        {
            asset.Release();
        }
        public void UnloadAsync(Asset asset)//删除异步加载的资源，目的是如果异步没加载完就卸载了，必须先清除回调
        {
            if (asset.asset == null)
                (asset as BundleAssetAsync).completed = null;
            asset.Release();
        }
        bool InitializeBundle()
        {
            var url =
#if UNITY_EDITOR
                AssetBundleUtility.AssetBundlesOutputPath + "/";
#else
				Path.Combine(Application.streamingAssetsPath, AssetBundleUtility.AssetBundlesOutputPath) + "/"; 
#endif
            if (BundleMgr.Ins.Initialize(url))
            {
                //Debug.Log(url);
                var bundle = BundleMgr.Ins.Load("manifest");
                if (bundle != null)
                {
                    var asset = bundle.LoadAsset<TextAsset>("Manifest.txt");
                    if (asset != null)
                    {
                        using (var reader = new StringReader(asset.text))
                        {
                            manifest.Load(reader);
                            reader.Close();
                        }
                        bundle.Release();
                        Resources.UnloadAsset(asset);
                        asset = null;
                    }
                    return true;
                }
                throw new FileNotFoundException("assets manifest not exist.");
            }
            throw new FileNotFoundException("bundle manifest not exist.");
        }

        //        void InitializeBundleAsync(System.Action onComplete)
        //        {
        //            string relativePath = Path.Combine(EditorUtility.AssetBundlesOutputPath, EditorUtility.GetPlatformName());
        //            var url =
        //#if UNITY_EDITOR
        //                relativePath + "/";
        //#else
        //				Path.Combine(Application.streamingAssetsPath, relativePath) + "/"; 
        //#endif

        //            Defer.RunCoroutine(BundleMgr.Ins.InitializeAsync(url, bundle =>
        //            {
        //                if (bundle != null)
        //                {
        //                    var asset = bundle.LoadAsset<TextAsset>("Manifest.txt");
        //                    if (asset != null)
        //                    {
        //                        using (var reader = new StringReader(asset.text))
        //                        {
        //                            manifest.Load(reader);
        //                            reader.Close();
        //                        }
        //                        bundle.Release();
        //                        Resources.UnloadAsset(asset);
        //                        asset = null;
        //                    }
        //                }

        //                if (onComplete != null)
        //                {
        //                    onComplete.Invoke();
        //                }
        //            }));
        //        }

        Asset CreateAssetRuntime(string path, System.Type type, bool asyncMode)
        {
            if (asyncMode)
                return new BundleAssetAsync(path, type);
            return new BundleAsset(path, type);
        }

        Asset LoadInternal(string path, System.Type type, bool asyncMode)
        {
            Asset asset = assets.Find(obj => { return obj.assetPath == path; });
            if (asset == null)
            {
#if UNITY_EDITOR
                if (AssetBundleUtility.ActiveBundleMode)
                {
                    asset = CreateAssetRuntime(path, type, asyncMode);
                }
                else
                {
                    //Debug.Log(path);
                    asset = new Asset(path, type);
                }
#else
				asset = CreateAssetRuntime (path,type, asyncMode);
#endif
                assets.Add(asset);
                asset.OnLoad();
            }
            asset.references++;
            //asset.Retain(); 
            return asset;
        }

        


        System.Collections.IEnumerator gc = null;
        System.Collections.IEnumerator GC()
        {
			System.GC.Collect ();
            yield return 0;
            yield return Resources.UnloadUnusedAssets();
        }

        public void Update()
        {
            bool removed = false;
            for (int i = assets.Count - 1; i >= 0; i--)
            {
                var asset = assets[i];
                if (asset.Update() && asset.references <= 0)
                {
                    //Debug.Log("卸载资源 " + asset.asset.name);
                    asset.OnUnload();
                    asset = null;
                    assets.RemoveAt(i);
                    removed = true;
                }
            }

            if (removed)
            {
                if (gc != null)
                {
                    gc = GC();
                }
            }

            BundleMgr.Ins.Update();
        }

        //public Sprite LoadSprite(string path)
        //{
        //    path = "Assets/ABResources/" + path;
        //    string pathTemp = path.Substring(0, path.LastIndexOf("/"));//Assets/ABResources/Atlas/BookBut/BookBut.png
        //    string resourceName = path.Substring(path.LastIndexOf("/")+1, path.Length - path.LastIndexOf("/")-1);//  "树真好"
        //    return LoadAtlasSprite(pathTemp, resourceName);

        //}
        public Sprite LoadSingleSprite(string path)
        {
            Texture2D texture = Load(path, typeof(Texture2D)) as Texture2D;
            return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
        }
        public Sprite LoadAtlasSprite(string spriteAtlasPath, string spriteName)
        {
            spriteAtlasPath = "Assets/ABResources/Atlas/" + spriteAtlasPath + ".spriteatlasv2";
            SpriteAtlas atlas;
            if (!atlasDic.TryGetValue(spriteAtlasPath, out atlas))
            {
#if UNITY_EDITOR
                if (AssetBundleUtility.ActiveBundleMode)
                {
                    //string url = Path.Combine(Application.streamingAssetsPath, AssetBundleUtility.AssetBundlesOutputPath) + "/";
                    //string path = url + spriteAtlasPath;
                    //Debug.Log("LoadSpriteAB " + GetBundleName(spriteAtlasPath));
                    Bundle bundle = BundleMgr.Ins.Load(GetBundleName(spriteAtlasPath));
                    atlas = bundle.assetBundle.LoadAsset<SpriteAtlas>(spriteAtlasPath.ToLower());
                    //string[] allname = bundle.assetBundle.GetAllAssetNames();
                    //for (int i = 0; i < allname.Length; i++)
                    //{
                    //    Debug.Log(allname[i]);
                    //}
                }
                else
                {
                    
                    atlas = UnityEditor.AssetDatabase.LoadAssetAtPath<SpriteAtlas>(spriteAtlasPath);
                }

#else
                //string url = Path.Combine(Application.streamingAssetsPath, AssetBundleUtility.AssetBundlesOutputPath) + "/"; 
                //string path = url + spriteAtlasPath;
                //Debug.Log("LoadSprite " + url + GetBundleName(spriteAtlasPath));
                Bundle bundle = BundleMgr.Ins.Load(GetBundleName(spriteAtlasPath));
                atlas = bundle.assetBundle.LoadAsset<SpriteAtlas>(spriteAtlasPath.ToLower());
                // atlas = AssetBundle.LoadFromFile(url + GetBundleName(spriteAtlasPath)).LoadAllAssets();
#endif
                atlasDic.Add(spriteAtlasPath, atlas);
            }
            //for (int i = 0; i < atlas.Length; i++)
            //{
            //    if (atlas[i].name == spriteName)
            //    {
            //        Sprite sp = atlas[i] as Sprite;
            //        return sp;
            //    }
            //}
            //Debug.Log("加载图片, 图集: " + spriteAtlasPath + "图片: " + spriteName);
            //Debug.Log(atlas);
            return atlas.GetSprite(spriteName);
        }
        //删除图集缓存  
        public void DeleteAtlas(string spriteAtlasPath)
        {
            if (atlasDic.ContainsKey(spriteAtlasPath))
            { 
                atlasDic.Remove(spriteAtlasPath); 
            }
        }

        public GameObject LoadL2dModel(int roleId)//目前需要判断是否加载成功，未来删除此函数
        {
            // todo 从统一的位置获取container

            try
            {
                GameObject l2dPrefab = Load<GameObject>("Live2d/" + roleId + "/" + roleId + ".prefab");
                return l2dPrefab;
                
            }
            catch (Exception e)
            {
                //Debug.Log(e.ToString());
                return null;
            }
        }

        // public bool LoadL2dModel(int roleId)
        // {
        //     var containerTrans = GameObject.Find("/Live2dContainer").transform;
        //     for (int i = containerTrans.childCount - 1; i >= 0; i--)
        //     {
        //         Object.Destroy(containerTrans.GetChild(i).gameObject);
        //     }
        //
        //     bool isSuccess = false;
        //     // todo 从asset bundle中加载
        //     var path = "Assets/ABResources/Live2d/" + roleId + "/1.model3.json";
        //     var model3Json = CubismModel3Json.LoadAtPath(path, BuiltinLoadAssetAtPath);
        //     if (model3Json != null)
        //     {
        //         var model = model3Json.ToModel();
        //         if (model != null)
        //         {
        //             isSuccess = true;
        //             model.transform.SetParent(containerTrans);
        //             model.transform.SetLocalPosition(Vector3.zero);
        //         }
        //     }
        //
        //     return isSuccess;
        // }
        //
        // private static object BuiltinLoadAssetAtPath(Type assetType, string absolutePath)
        // {
        //     if (assetType == typeof(byte[]))
        //     {
        //         return File.ReadAllBytes(absolutePath);
        //     }
        //
        //     if (assetType == typeof(string))
        //     {
        //         return File.ReadAllText(absolutePath);
        //     }
        //
        //     if (assetType == typeof(Texture2D))
        //     {
        //         var texture = new Texture2D(1, 1);
        //         texture.LoadImage(File.ReadAllBytes(absolutePath));
        //         return texture;
        //     }
        //
        //     throw new NotSupportedException();
        // }
    }
}
