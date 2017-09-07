using System;
using System.Collections.Generic;


namespace tpf
{
    public enum AssetType
    {
        Resource = 0,
        Prefab,
    }
    public enum AssetLoadStatus
    {
        Wait = 0,
        Downloading,
        Downloaded,
        Loading,
        Loaded,
        Finished,
    }
    public enum AssetMemoryType
    {
        Auto_Clean = 0,
        Manual_Clean,
        DoNot_Clean,
    }
    //config info
    [Serializable]
    public class AssetInfo
    {
        public string name;
        public string dependBundle;
        public string extName;
    }

    [Serializable]
    public class AssetInfoConfig
    {
        public List<AssetInfo> assetInfo = new List<AssetInfo>();
    }

    [Serializable]
    public class BundleInfo
    {
        public string name;
        public List<string> dependBundle;

        public bool HasDependBundle()
        {
            return (dependBundle != null && dependBundle.Count > 0);
        }
        public bool IsDependBundle(string depName)
        {
            if (!HasDependBundle())
                return false;

            return dependBundle.Contains(depName);
        }
        public int GetDependBundleCount()
        {
            if (!HasDependBundle())
                return 0;
        
            return dependBundle.Count;
        }
    }

    [Serializable]
    public class BundleInfoConfig
    {
       public List<BundleInfo> bundleInfo = new List<BundleInfo>();
    }
}


