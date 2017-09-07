
using System.Collections.Generic;

namespace tpf
{
 
    public class AssetsConfig
    {
        Dictionary<string, BundleInfo> m_bundleDic = new Dictionary<string, BundleInfo>();
        Dictionary<string, AssetInfo> m_assetDic = new Dictionary<string, AssetInfo>();
        public void ReloadConfig()
        {
            m_bundleDic.Clear();
            m_assetDic.Clear();
            string fullPath = PathUtils.GetVaildFullPath("bundle_info.json");
            BundleInfoConfig bundleCfg = SerializeHelper.LoadFromFile<BundleInfoConfig>(fullPath);
            if(bundleCfg != null)
            {
                for (int i = 0; i < bundleCfg.bundleInfo.Count; ++i)
                {
                    BundleInfo info = bundleCfg.bundleInfo[i];
                    m_bundleDic.Add(info.name, info);
                }
            }

            fullPath = PathUtils.GetVaildFullPath("asset_info.json");
            AssetInfoConfig assetCfg = SerializeHelper.LoadFromFile<AssetInfoConfig>(fullPath);
            if(assetCfg != null)
            {
                for (int i = 0; i < assetCfg.assetInfo.Count; ++i)
                {
                    AssetInfo info = assetCfg.assetInfo[i];
                    m_assetDic.Add(info.name, info);
                }
            }
        }

        public AssetInfo GetAssetInfo(string name)
        {
            AssetInfo info = null;
            if (m_assetDic.TryGetValue(name, out info))
                return info;

            return null;
        }

        public string[] GetFileListByExtName(string extName)
        {
            List<string> fileList = new List<string>();
            foreach(KeyValuePair<string,AssetInfo> kv in m_assetDic)
            {
                if(kv.Value.extName.Equals(extName))
                {
                    fileList.Add(kv.Key);
                }
            }
            return fileList.ToArray();
        }

        public BundleInfo GetBundleInfo(string name)
        {
            BundleInfo info = null;
            if(m_bundleDic.TryGetValue(name, out info))
                return info;

            return null;
        }
    }

}