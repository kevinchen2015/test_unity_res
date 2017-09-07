using System;
using UnityEngine;
using System.Collections;


namespace tpf
{
    //对外的简单实用方式
    public class AssetsUtils
    {
        public static UnityEngine.Object LoadAssetSync(string name)
        {
            AssetCache asset = AssetsMgr.GetInst().LoadAssetSync(name);
            if (asset == null)
                return null;

            return asset.GetAsset();
        } 
        
        public static void LoadAssetAync(string name,Action<string,UnityEngine.Object> callback)
        {
            AssetAyncLoaderProxy proxy = new AssetAyncLoaderProxy();
            proxy.callback = callback;
            proxyList.Add(proxy);
            AssetsMgr.GetInst().LoadAssetAsync(name, proxy.OnAssetLoaded);
        }

#region private
        static System.Collections.Generic.List<AssetAyncLoaderProxy> proxyList = new System.Collections.Generic.List<AssetAyncLoaderProxy>();
        class AssetAyncLoaderProxy
        {
            public Action<string, UnityEngine.Object> callback;
            public void OnAssetLoaded(string name, AssetCache cache)
            {
                if(cache == null)
                {
                    if(callback != null)
                    {
                        callback(name, null);
                    }
                }
                else
                {
                    if (callback != null)
                    {
                        callback(name, cache.GetAsset());
                    }
                }
                proxyList.Remove(this);
            }
        }
#endregion

    }

    //预加载器
    public class Preloader
    {
        public static System.Collections.Generic.List<PreloadItem> preLoadItems = new System.Collections.Generic.List<PreloadItem>();
        public static IEnumerator PreLoadAsyc(string[] resList )
        {
            counter = 0;

            preLoadItems.Clear();
            for (int i = 0; i < resList.Length; i++)
            {
                PreloadItem item = new PreloadItem();
                item.name = resList[i];
                item.onAssetLoaded = OnAssetLoaded;
                preLoadItems.Add(item);

                AssetsMgr.GetInst().LoadAssetAsync(item.name, item.OnAssetLoaded);
            }

            bool bAllFinished = false;
            while (bAllFinished == false)
            {
                if (counter == preLoadItems.Count)
                {
                    bAllFinished = true;
                    break;
                }
                else
                {
                    yield return counter;
                }
            }
        }

        static int counter = 0;
        static void OnAssetLoaded()
        {
            ++counter;
        }
    }

    public class PreloadItem
    {
        public string name;
        public UnityEngine.Object obj;
        public bool finished = false;
        public Action onAssetLoaded;
        public void OnAssetLoaded(string name, AssetCache cache)
        {
            finished = true;
            if (cache == null)
                obj = null;
            else
                obj = cache.GetAsset();

            if (onAssetLoaded != null)
            {
                onAssetLoaded();
            }
        }
    }
}


