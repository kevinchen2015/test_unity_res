using UnityEngine;
using System.IO;
using System.Collections.Generic;

namespace tpf
{
    public class PathUtils
    {
        static string persistentPath;
        static string streamAssetsPath;
        static string streamAssetsPathURL;
        static string innerAssetBundlePathURL;
        static string extAssetBundlePath;
        static string extAssetBundlePathURL;

        public static string GetPersistentPath()
        {
            return persistentPath;
        }

        public static void Init()
        {
            streamAssetsPath = Application.streamingAssetsPath + "/";
            streamAssetsPathURL = "file://" + Application.streamingAssetsPath + "/";

            if (Application.isEditor)
            {
                persistentPath = Application.dataPath + "/../PersistentData/";
                extAssetBundlePath = persistentPath + "AssetBundles/";
            }
            else
            {
                extAssetBundlePath = Application.persistentDataPath + "/AssetBundles/";
                persistentPath = Application.persistentDataPath + "/";
                if (Application.platform == RuntimePlatform.IPhonePlayer)
                {

                }
                else if (Application.platform == RuntimePlatform.Android)
                {
                    streamAssetsPathURL = Application.streamingAssetsPath + "/";
                }
                else
                {
                    persistentPath = Application.dataPath + "/../PersistentData/";
                    extAssetBundlePath = persistentPath + "AssetBundles/";
                }
            }
            innerAssetBundlePathURL = streamAssetsPathURL + "AssetBundles/";
            extAssetBundlePathURL = "file://" + extAssetBundlePath + "/";

            if (!Directory.Exists(persistentPath))
            {
                Directory.CreateDirectory(persistentPath);
            }
        }

        public static string GetStreamingPath()
        {
            string Path = "";
            if (Application.platform == RuntimePlatform.WindowsEditor ||
                Application.platform == RuntimePlatform.OSXEditor ||
                Application.platform == RuntimePlatform.WindowsPlayer ||
                Application.platform == RuntimePlatform.LinuxPlayer
#if PLATFORM_PS4
		    || Application.platform == RuntimePlatform.PS4
#endif
#if UNITY_XBOXONE
			|| Application.platform == RuntimePlatform.XboxOne
#endif
)
            {
                Path = Application.dataPath + "/StreamingAssets/";
            }
            else if (Application.platform == RuntimePlatform.OSXPlayer ||
                Application.platform == RuntimePlatform.OSXDashboardPlayer)
            {
                Path = Application.dataPath + "/Data/StreamingAssets/";
            }
            else if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                Path = Application.dataPath + "/Raw/";
            }
            else if (Application.platform == RuntimePlatform.Android)
            {
                Path = "jar:file://" + Application.dataPath + "!/assets/";
            }
            return Path;
        }


        static string strNull = "";
        public static string GetVaildFullPath(string name)
        {
            string path = persistentPath + name;
            if(File.Exists(path))
            {
                return path;
            }
            path = GetStreamingPath() + name;
            if (File.Exists(path))
            {
                return path;
            }
            return strNull;
        }

        public static void GetDirList(ref List<string> dirs, string path, bool includeSubPath)
        {
            string[] folders = Directory.GetDirectories(path);
            foreach (string dirName in folders)
            {
                if (dirName.EndsWith(".svn"))
                    continue;

                string str = dirName.Replace('\\', '/');
                dirs.Add(str);
            }
            if (includeSubPath)
            {
                foreach (string folderName in folders)
                {
                    if (folderName.EndsWith(".svn"))
                        continue;
                    GetDirList(ref dirs, folderName, includeSubPath);
                }
            }
        }
        public static void GetFileList(ref List<string> files, string path, bool includeSubPath)
        {
            string[] _files = Directory.GetFiles(path);
            foreach (string fileName in _files)
            {
                if (fileName.EndsWith(".svn") || fileName.EndsWith(".meta"))
                    continue;

                string name = fileName.Replace("\\", "/");
                files.Add(name);
            }

            if (includeSubPath)
            {
                string[] folders = Directory.GetDirectories(path);
                foreach (string folderName in folders)
                {
                    if (folderName.EndsWith(".svn"))
                        continue;
                    GetFileList(ref files, folderName, includeSubPath);
                }
            }
        }
        public static void GetFileListByEnds(ref List<string> srcFiles, string ends, ref List<string> desFiles)
        {
            foreach (string fileName in srcFiles)
            {
                if (fileName.EndsWith(ends))
                {
                    desFiles.Add(fileName);
                }
            }
        }

        public static string ConvPathToBundleName(string path)
        {
            path = path.Replace('\\', '/');
            string keyPath = "Resources/";
            int idx = path.IndexOf(keyPath);
            if (idx < 0)
                return "";

            int startPos = idx + keyPath.Length;
            string bundleName = path.Substring(startPos, path.Length - startPos);
            bundleName += ".bundle";
            return bundleName;

        }

        public static void MakeSureDirExist(string fullFileName)
        {
            fullFileName = fullFileName.Replace('\\', '/');
            string[] subPath = fullFileName.Split('/');
            if (subPath == null) return;

            string dir = "";
            for(int i = 0;i < subPath.Length;++i)
            {
                if(i == subPath.Length-1)
                {
                    int idx = subPath[i].IndexOf('.');
                    if(idx > 0)
                    {
                        continue;
                    }
                }
                dir += (subPath[i]+"/");
                if(!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
            }
        }

    }
}