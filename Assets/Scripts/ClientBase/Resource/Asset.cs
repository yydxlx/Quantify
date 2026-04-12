using System.Collections;
using UnityEngine;

namespace CocoonAsset
{
    public class Asset : IEnumerator
    {
        
        #region IEnumerator implementation

        public bool MoveNext()
        {
            return !isDone();
        }

        public void Reset(){ }

        public object Current{get{return null; }}

        #endregion

        public int references;

        public string assetPath { get; protected set; }
        public System.Type assetType;
        public virtual bool isDone()
        {
            return true;
        }

        public Object asset { get; protected set; }
        internal Asset(string path, System.Type _assetType)
        {
            assetPath = path;
            assetType = _assetType;
        }

        public virtual void OnLoad()
        {
#if UNITY_EDITOR
            //if (assetPath == "Assets/ABResources/Sprites/ItemIcon/10001.png")
            //    asset = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
            //else
            //{
                //Debug.Log(assetPath);
                //Debug.Log("Assets/ABResources/Sprites/ItemIcon/10001.png");
                //Debug.Log(assetPath == "Assets/ABResources/Sprites/ItemIcon/10001.png");
                //Debug.Log(assetPath.Length);
                //Debug.Log("Assets/ABResources/Sprites/ItemIcon/10001.png".Length);
                asset = UnityEditor.AssetDatabase.LoadAssetAtPath(assetPath, assetType);
            //}
                

#endif
        }

        internal virtual bool Update()
        {
            return true;
        }
        public virtual void OnUnload()
        {
            asset = null;
            assetPath = null;
        }
        public void Release()
        {
            if (--references < 0)
            {
                Debug.LogError(asset.name + " refCount = " + references );
            }
        }
    }

    public class BundleAsset : Asset
    {
        protected Bundle request;

        internal BundleAsset(string path, System.Type _assetType) : base(path, _assetType)
        {
        }

        public override void OnLoad()
        {
            //request = BundleMgr.Ins.Load(ResourceMgr.Ins.GetBundleName(assetPath));
            string name = ResourceMgr.Ins.GetBundleName(assetPath);
            request = BundleMgr.Ins.Load(name);
            asset = request.LoadAsset(ResourceMgr.Ins.GetAssetName(assetPath));
        }

        public override void OnUnload()
        {
            base.OnUnload();
            if (request != null)
            {
                request.Release();
            }
            request = null;
        }
    }

    public class BundleAssetAsync : BundleAsset
    {
        public System.Action<Asset> completed;

        //public void AddCompletedListener(System.Action<Asset> listener)
        //{
        //    completed += listener;
        //}

        //public void RemoveCompletedListener(System.Action<Asset> listener)
        //{
        //    completed -= listener;
        //}
        AssetBundleRequest assetBundleRequest;
        int loadState;//0,1加载中,2完成
        internal BundleAssetAsync(string path, System.Type _assetType) : base(path, _assetType)
        {

        }

        public override void OnLoad()
        {
            //Debug.Log("ab path: " + assetPath);
            string bundleName = ResourceMgr.Ins.GetBundleName(assetPath);
            
            request = BundleMgr.Ins.LoadAsync(bundleName);
        }

        public override void OnUnload()
        {
            base.OnUnload();
            assetBundleRequest = null;
            loadState = 0;
        }
        internal override bool Update()
        {
            if (isDone())
            {
                if (completed != null)
                {
                    completed.Invoke(this);
                    completed = null;
                }
                return true;
            }
            return false;
        }

        public override bool isDone()
        {
            if (loadState == 2)
            {
                return true;
            }

            if (request.error != null)
            {
                return true;
            }

            for (int i = 0; i < request.dependencies.Count; i++) // 依赖没有错误
            {
                var dep = request.dependencies[i];
                if (dep.error != null)
                {
                    return true;
                }
            }

            if (loadState == 1)
            {
                if (assetBundleRequest.isDone)
                {
                    asset = assetBundleRequest.asset;
                    loadState = 2;
                    return true;
                }
            }
            else
            {
                bool allReady = true;
                if (!request.isDone)
                {
                    allReady = false;
                }

                if (request.dependencies.Count > 0)
                {
                    if (!request.dependencies.TrueForAll(bundle => bundle.isDone))
                    {
                        allReady = false;
                    }
                }

                if (allReady)
                {
                    assetBundleRequest = request.LoadAssetAsync(System.IO.Path.GetFileName(assetPath));
                    if (assetBundleRequest == null)
                    {
                        loadState = 2;
                        return true;
                    }
                    loadState = 1;
                }
            }
            return false;

        }
    }
}
