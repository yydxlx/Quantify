using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using ClientBase;

namespace CocoonAsset.Editor
{
    public static class AssetsMenuItem
	{
		//[MenuItem ("CocoonAssets/Copy Asset Path")]
		//static void CopyAssetPath ()
		//{
		//	if (EditorApplication.isCompiling) {
		//		return;
		//	}
		//	string path = AssetDatabase.GetAssetPath (Selection.activeInstanceID);   
		//	GUIUtility.systemCopyBuffer = path;
		//	Debug.Log (string.Format ("systemCopyBuffer: {0}", path));
		//}  

		const string kRuntimeMode = "CocoonAssets/AB包模式"; 

		[MenuItem (kRuntimeMode)]
		public static void ToggleRuntimeMode ()
		{
            AssetBundleUtility.ActiveBundleMode = !AssetBundleUtility.ActiveBundleMode;  
		}

		[MenuItem (kRuntimeMode, true)]
		public static bool ToggleRuntimeModeValidate ()
		{
            Menu.SetChecked (kRuntimeMode, AssetBundleUtility.ActiveBundleMode);
			return true;
		} 

		const string assetsManifesttxt = "Assets/Manifest.txt";

		[MenuItem ("CocoonAssets/Build Manifest")]  
		public static void BuildAssetManifest ()
		{  
			if (EditorApplication.isCompiling) 
			{
				return;
			}     
			List<AssetBundleBuild> builds = BuildRule.GetBuilds (assetsManifesttxt);
            BuildScript.BuildManifest (assetsManifesttxt, builds);
		}  

		[MenuItem ("CocoonAssets/Build AB包")]  
		public static void BuildAssetBundles ()
		{  
			if (EditorApplication.isCompiling) 
			{
				return;
			}       
			List<AssetBundleBuild> builds = BuildRule.GetBuilds (assetsManifesttxt);
            BuildScript.BuildManifest (assetsManifesttxt, builds);
			BuildScript.BuildAssetBundles (builds);
		}  

		[MenuItem ("CocoonAssets/复制AB包到StreamingAssets")]  
		public static void CopyAssetBundlesToStreamingAssets ()
		{  
			if (EditorApplication.isCompiling) 
			{
				return;
			}        
			BuildScript.CopyAssetBundlesTo (Path.Combine (Application.streamingAssetsPath, AssetBundleUtility.AssetBundlesOutputPath));

            AssetDatabase.Refresh();
		}  
		[MenuItem ("CocoonAssets/删除所有AB包")] 
		public static void DeleteAllAB ()
		{  
			if (EditorApplication.isCompiling) 
			{
				return;
			}
            FileHelper.DeleteDirectory(Application.dataPath + "/../AssetBundles");
			FileHelper.DeleteDirectory(Application.streamingAssetsPath + "/AssetBundles");
            AssetDatabase.Refresh();
		}  

		[MenuItem ("CocoonAssets/一键打包")]  
		public static void BuildPlayer ()
		{
			if (EditorApplication.isCompiling) 
			{
				return;
			}  
			BuildScript.BuildStandalonePlayer ();
		}
	}
}