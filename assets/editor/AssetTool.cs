using UnityEditor;
using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;

//本套工具要求，美术原始资源放在Resources外，游戏内引用资源或者加工后资源放在Resources下,相关资源或类似资源，需要归类到相同的文件夹或者子文件夹下，作为打包依据
//好处：1. 隔离美术资源对游戏资源的影响。2.资源引用和清理简单明确，打包也易做区分。3.对开发和发布下，游戏资源上层使用的透明一致性。


namespace tpf
{
    public class AssetTool
    {
        static string outputName = "PersistentData";
        static string outputPath = "./" + outputName;
        static string bundleInfoFileName = "bundle_info.json";
        static string assetInfoFileName = "asset_info.json";

        static BuildInfo buildInfo = new BuildInfo();

        //打包方式
        public enum PackType  
        {
            PackByDir = 0,  //以文件夹为单位打包 ,会做子文件夹深度遍历
            PackBySubDir,   //以文件夹下的子文件夹为单位打包 ,会做子子文件夹深度遍历
        }

        [Serializable]
        public class BuildBundleInfo
        {
            public PackType packType = PackType.PackByDir;
            public string path = "";                                  //目录名
            public List<string> extNameList = new List<string>();     //对要打入包中的扩展名的强要求，为空，表示全部游戏资源都要打入
            public string assignBundleName = "";                      //PackBySubDir 方式下无效,其他打包方式下空代表采用默认名（目录名）

            public void SetAssetBundleInfo()
            {
                string fullPath = Application.dataPath +"/Resources/" + path;
                if(!Directory.Exists(fullPath))
                {
                    Debug.LogError("path error!"+fullPath);
                    return;
                }

                fullPath = "Assets/Resources/" + path;
                if (packType == PackType.PackByDir)
                {
                    string bundleName = PathUtils.ConvPathToBundleName(fullPath);
                    PackOne(fullPath, assignBundleName.Length > 0 ? assignBundleName : bundleName, extNameList);
                }
                else if(PackType.PackBySubDir == packType)
                {
                    List<string> subDir = new List<string>();
                    PathUtils.GetDirList(ref subDir, fullPath, false);
                    for(int i=0;i< subDir.Count;++i)
                    {
                        string bundleName = PathUtils.ConvPathToBundleName(subDir[i]);
                        PackOne(subDir[i], bundleName, extNameList);
                    }
                }
            }
            public void Remove()
            {
                if (Directory.Exists(path))
                {
                    Debug.Log("test delete path:" + path);
                    //todo delete
                    //Directory.Delete(path, true);
                }
            }
            void PackOne(string fullPath,string bundleName,List<string> extNameList)
            {
                //Debug.Log("pack one,full path:"+fullPath+" ,bundle name:"+bundleName);
                List<string> files = new List<string>();
                PathUtils.GetFileList(ref files, fullPath, true);
                if(extNameList.Count == 0)
                {
                    foreach (string file in files)
                    {
                        AssetImporter assetImporter = AssetImporter.GetAtPath(file);
                        if (assetImporter != null)
                        {
                            assetImporter.assetBundleName = bundleName;
                        }
                    }
                    return;
                }
                List<string>[] resList = new List<string>[extNameList.Count];
                for(int i = 0;i < extNameList.Count;++i)
                {
                    resList[i] = new List<string>();
                }
                for (int i = 0; i < extNameList.Count; ++i)
                {
                    PathUtils.GetFileListByEnds(ref files, extNameList[i], ref resList[i]);
                }
                for (int i = 0; i < extNameList.Count; ++i)
                {
                    foreach (string file in resList[i])
                    {
                        AssetImporter assetImporter = AssetImporter.GetAtPath(file);
                        if (assetImporter != null)
                        {
                            assetImporter.assetBundleName = bundleName;
                        }
                    }
                }
            }
        }
        [Serializable]
        public class BuildAssetInfo
        {
            public List<BuildBundleInfo> buildBundle = new List<BuildBundleInfo>();

            public void BuildAll(BuildTarget target)
            {
                for(int i = 0;i < buildBundle.Count;++i)
                {
                    EditorUtility.DisplayProgressBar("mark assetbundle name", "handle...", (float)i / buildBundle.Count);
                    buildBundle[i].SetAssetBundleInfo();
                }
                EditorUtility.ClearProgressBar();
                
            }

            public void RemoveBundledDir()
            {
                for (int i = 0; i < buildBundle.Count; ++i)
                {
                    buildBundle[i].Remove();
                }
            }
        }

        [Serializable]
        public class BuildInfo
        {
            public BuildTarget target = BuildTarget.Android;
            public BuildAssetInfo buildAssetInfo = new BuildAssetInfo();

            public void MarkBundleName()
            {
                buildAssetInfo.BuildAll(target);
            }

            public void BuildBundle()
            { 
                PackBundle(target);
            }

            public void CleanAssetsBundleInfo()
            {
                MakeAssetBundleNameAsNull("Assets/");
            }

            //删除在打包规则里的Resources下目录，不在构建player的时候构建进包中
            public void RemoveBundledDir()
            {
                buildAssetInfo.RemoveBundledDir();
            }
        }

        static void MakeAssetBundleNameAsNull(string dir)
        {
            List<string> files = new List<string>();
            PathUtils.GetFileList(ref files, dir, true);
            int filesCount = files.Count;
            int counter = 0;
            foreach (string file in files)
            {
                ++counter;
                EditorUtility.DisplayProgressBar("MakeAssetBundleNameAsNull", "handle...", (float)counter / filesCount);

                string strExtension = Path.GetExtension(file);
                if (strExtension.Equals(".cs"))
                    continue;

                AssetImporter assetImporter = AssetImporter.GetAtPath(file);
                if (assetImporter != null)
                {
                    assetImporter.assetBundleName = "";
                }
            }
            EditorUtility.ClearProgressBar();
        }

        static void MakePrefabAssetbundleName(string path)
        {
            List<string> files = new List<string>();
            List<string> prefabs = new List<string>();
            PathUtils.GetFileList(ref files, path, true);
            PathUtils.GetFileListByEnds(ref files, ".prefab", ref prefabs);

            string assetBundleName = path.Replace("Assets/", "");
            assetBundleName += ".bundle";
            int filesCount = prefabs.Count;
            int counter = 0;
            foreach (string file in prefabs)
            {
                EditorUtility.DisplayProgressBar("MakeAssetBundleNameAsNull", "handle...", (float)counter / filesCount);

                string strExtension = Path.GetExtension(file);
                if (strExtension.Equals(".cs"))
                    continue;

                AssetImporter assetImporter = AssetImporter.GetAtPath(file);
                if (assetImporter != null)
                {
                    assetImporter.assetBundleName = assetBundleName;
                }
            }
            EditorUtility.ClearProgressBar();
        }

        [MenuItem("AssetTool/CleanAssetBundleInfo")]
        public static void CleanAssetBundleInfo()
        {
            MakeAssetBundleNameAsNull("Assets/");
        }

        
        [MenuItem("AssetTool/SetAssetBundleInfo")]
        public static void SetAssetBundleInfo()
        {
            //MakePrefabAssetbundleName("Assets/Resources");
            string build = Application.dataPath + "/build.json";
            buildInfo = SerializeHelper.LoadFromFile<BuildInfo>(build);
            buildInfo.MarkBundleName();
        }
        


        static void CopyDirWithoutFiles(string src,string des)
        {
            if(!Directory.Exists(des))
            {
                Directory.CreateDirectory(des);
            }
            List<string> dirList = new List<string>();
            PathUtils.GetDirList(ref dirList, src, true);

            for(int i = 0;i < dirList.Count;++i)
            {
                dirList[i] = dirList[i].Replace(src, des);
                //Debug.Log(dirList[i]);

                if (!Directory.Exists(dirList[i]))
                {
                    Directory.CreateDirectory(dirList[i]);
                }
            }
        }

        static void CopyFileWithExt(string src,string des,string ext)
        {
            List<string> file = new List<string>();
            PathUtils.GetFileList(ref file, src, true);
            List<string> extFile = new List<string>();
            PathUtils.GetFileListByEnds(ref file, ext, ref extFile);
            for (int i = 0; i < extFile.Count; ++i)
            {
                string fileSrc = extFile[i];
                extFile[i] = extFile[i].Replace(src, des);

                if (File.Exists(extFile[i]))
                {
                    File.Delete(extFile[i]);
                }
                if (File.Exists(extFile[i] + ".meta"))
                {
                    File.Delete(extFile[i] + ".meta");
                }
                //Debug.Log(extFile[i]);

                File.Copy(fileSrc, extFile[i]);
                File.Copy(fileSrc + ".meta", extFile[i]+ ".meta");

                File.Delete(fileSrc);
                File.Delete(fileSrc + ".meta");
            }
        }

        static void DeleteNullDir(string dir)
        {
            if (!Directory.Exists(dir))
                return;

            string[] folders = Directory.GetDirectories(dir);
            string[] _files = Directory.GetFiles(dir);

            foreach (string folderName in folders)
            {
                if (folderName.EndsWith(".svn"))
                      continue;

                DeleteNullDir(folderName);
            }
            
            if(folders.Length == 0)
            {
                if(_files.Length == 0)
                    Directory.Delete(dir);
            }
        }

        
        //[MenuItem("AssetTool/CopyPrefab")]
        public static void CopyPrefab()
        {
            string src = Application.dataPath + "/Art/Character/";
            string des = Application.dataPath + "/Resources/Character/";
            //CopyDirWithoutFiles(src, des);
            //CopyFileWithExt(src, des,".prefab");
            //DeleteNullDir(des);

            //buildInfo.buildAssetInfo.buildBundle.Clear();
            //BuildBundleInfo info = new BuildBundleInfo();
            //info.path = "1";
            //info.assignBundleName = "xxx";
            //info.extNameList.Add(".prefab");
            //buildInfo.buildAssetInfo.buildBundle.Add(info);
            //string build = Application.dataPath + "/build.json";
            //SerializeHelper.SaveToFile(buildInfo, build, true);
        }
        
        
        [MenuItem("AssetTool/PackBundle")]
        public static void PackBundle()
        {
            buildInfo.target = BuildTarget.StandaloneWindows;
            buildInfo.BuildBundle();
            MakeAssetConfig();
        }
        

        [MenuItem("AssetTool/MakeAssetConfig")]
        public static void MakeAssetConfig()
        {
            try
            {
                AssetInfoConfig assetInfoCfg = new AssetInfoConfig();
                BundleInfoConfig bundleInfoCfg = new BundleInfoConfig();

                AssetBundle manifestBundle = AssetBundle.LoadFromFile(outputPath + "/" + outputName);
                AssetBundleManifest manifest = (AssetBundleManifest)manifestBundle.LoadAsset("AssetBundleManifest");
                string[] allBundle = manifest.GetAllAssetBundles();
                manifestBundle.Unload(false);

                for (int i = 0; i < allBundle.Length; ++i)
                {
                    EditorUtility.DisplayProgressBar("make asset config", "handle...", (float)i / allBundle.Length);

                    string[] depBundle = manifest.GetAllDependencies(allBundle[i]);
                    BundleInfo bundleInfo = new BundleInfo();
                    bundleInfoCfg.bundleInfo.Add(bundleInfo);
                    bundleInfo.name = allBundle[i];
                    bundleInfo.dependBundle = new List<string>();
                    for (int k = 0; k < depBundle.Length; ++k)
                    {
                        bundleInfo.dependBundle.Add(depBundle[k]);
                    }

                    AssetBundle bundle = AssetBundle.LoadFromFile(outputPath + "/" + allBundle[i]);
                    if (bundle == null)
                    {
                        Debug.LogError("assetbundle can not find!");
                        continue;
                    }
                    string[] allAsset = bundle.GetAllAssetNames();
                    bundle.Unload(false);
                    for (int k = 0; k < allAsset.Length; ++k)
                    {
                        AssetInfo assetInfo = new AssetInfo();
                        assetInfoCfg.assetInfo.Add(assetInfo);
                        string assetName = allAsset[k].Replace("assets/resources/", "");  
                        string strExtension = Path.GetExtension(assetName);
                        assetInfo.name = assetName.Replace(strExtension, "");
                        assetInfo.extName = strExtension;
                        assetInfo.dependBundle = allBundle[i];
                    }
                }

                EditorUtility.ClearProgressBar();
                string persistentPath = Application.dataPath + "/../PersistentData/";
                SerializeHelper.SaveToFile(bundleInfoCfg, persistentPath + bundleInfoFileName, true);
                SerializeHelper.SaveToFile(assetInfoCfg, persistentPath + assetInfoFileName, true);

                MakeVersionConfig("1.2.3.1111", "http://10.0.0.252:8080/oppo_ver_cfg/");

                Debug.Log("打包结束!");
            }
            catch (System.Exception ex)
            {
                EditorUtility.ClearProgressBar();
                Debug.Log("打包失败!");
            }
        }
        static void MakeVersionConfig(string version,string url)
        {
            AssetBundle manifestBundle = AssetBundle.LoadFromFile(outputPath + "/" + outputName);
            AssetBundleManifest manifest = (AssetBundleManifest)manifestBundle.LoadAsset("AssetBundleManifest");
            string[] allBundle = manifest.GetAllAssetBundles();
            manifestBundle.Unload(false);

            VersionRes ver = new VersionRes();
            ver.version = version;
            ver.url = url;

            {
                VersionResFile file = new VersionResFile();
                file.name = bundleInfoFileName;
                file.signature = FileChecker.GetFileSignature(outputPath + "/" + bundleInfoFileName);
                file.compress = "none";
                ver.versionResFile.Add(file);
            }

            {
                VersionResFile file = new VersionResFile();
                file.name = assetInfoFileName;
                file.signature = FileChecker.GetFileSignature(outputPath + "/" + assetInfoFileName);
                file.compress = "none";
                ver.versionResFile.Add(file);
            }

            for (int i = 0; i < allBundle.Length; ++i)
            {
                EditorUtility.DisplayProgressBar("MakeVersionConfig config", "handle...", (float)i / allBundle.Length);

                VersionResFile file = new VersionResFile();
                file.name = allBundle[i];
                file.signature = FileChecker.GetFileSignature(outputPath + "/" + allBundle[i]);
                file.compress = "none";
                ver.versionResFile.Add(file);
            }
            EditorUtility.ClearProgressBar();
            string persistentPath = Application.dataPath + "/../PersistentData/";
            SerializeHelper.SaveToFile(ver, persistentPath + "version.json", true);
        }
        static void PackBundle(BuildTarget target)
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            BuildPipeline.BuildAssetBundles(outputPath, BuildAssetBundleOptions.UncompressedAssetBundle, target);
            //AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        

    }
}