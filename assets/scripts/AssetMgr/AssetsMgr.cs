using System;
using System.IO;
using UnityEngine;
using System.Collections.Generic;

//1.只有AssetCache 和 AssetMgr 有可能会和应用层打交道.
//2.简单的应用方式通过AssetsUtils和应用层交互

namespace tpf
{
    //public
    public class AssetCache
    {
        private string m_name;
        private UnityEngine.Object m_asset = null;
        private AssetInfo m_info;
        private AssetMemoryType m_memoryType = AssetMemoryType.Auto_Clean;
        private float m_lastRefTime;
        private int m_refCount;
        private AssetType m_type = AssetType.Resource;

        public AssetCache(string name,AssetInfo info,UnityEngine.Object asset)
        {
            m_name = name;
            m_info = info;
            m_asset = asset;
            m_refCount = 0;

            if(info == null || info.extName.Equals(".prefab"))
            {
                m_type = AssetType.Prefab;
            }
            Access();
        }
        public void OnRelease()
        {
            if (m_asset != null)
            {
                if(m_type == AssetType.Resource)
                    Resources.UnloadAsset(m_asset);
            }
            m_asset = null;
            m_info = null;
        }
        public float GetLastRefTime()
        {
            return m_lastRefTime;
        }
        public AssetMemoryType GetMemoryType()
        {
            return m_memoryType;
        }
        public void SetMemoryType(AssetMemoryType type)
        {
            m_memoryType = type;
        }
        public UnityEngine.Object GetAsset()
        {
            Access();
            return m_asset;
        }
        public int GetRef()
        {
            return m_refCount;
        }
        public void Access()
        {
            m_lastRefTime = Time.time;
        }
        public void AddRef()
        {
            Access();
            ++m_refCount;
        }
        public void ReduceRef()
        {
            Access();
            --m_refCount;
        }

        public static string AssetNameToBundleAssetName(string assetName, string extName)
        {
            int idx = assetName.IndexOf("assets/resources/");
            if (idx < 0)
            {
                assetName = "assets/resources/" + assetName + extName;
            }
            return assetName;
        }
    }

    public class AssetLoadRequest
    {
        public string name;
        public Action<string, AssetCache> finishedCallback;
        public UnityEngine.Object asset = null;
        public AssetInfo info;
        
        private AssetLoadStatus m_status = AssetLoadStatus.Wait;
        private BundleCache m_refBundle = null;

        public AssetLoadStatus GetStatus()
        {
            return m_status;
        }

        public void OnRelease()
        {
            asset = null;
            info = null;
            finishedCallback = null;
            if (m_refBundle != null)
            {
                m_refBundle.ReduceRef();
                m_refBundle = null;
            }
        }

        public void SetLoading()
        {
            m_status = AssetLoadStatus.Loading;
        }

        public void OnAssetLoaded(AssetCache asset)
        {
            m_status = AssetLoadStatus.Finished;
            if(asset != null)
                asset.Access();
            if (finishedCallback != null)
            {
                finishedCallback(name,asset);
            }
        }

        public bool UpdateState()
        {
            bool handle = false;
            if(m_status == AssetLoadStatus.Loaded)
            {
                m_status = AssetLoadStatus.Finished;
                if (m_refBundle == null)
                {
                    OnAssetLoaded(null);
                    return handle;
                }

                UnityEngine.Object obj = m_refBundle.LoadAsset(AssetCache.AssetNameToBundleAssetName(info.name, info.extName));
                if (obj == null)
                {
                    OnAssetLoaded(null);
                    return handle;
                }

                handle = true;
                AssetCache cache = new AssetCache(name, info, obj);
                AssetsMgr.GetInst().AddAssetCache(name, cache);
                OnAssetLoaded(cache);
            }
            return handle;
        }

        public void OnAssetBundleLoaded(string bundleName, BundleCache bundle)
        {
            LogUtils.Log("bundle loaded!:",bundleName);
            m_status = AssetLoadStatus.Loaded;
            if (bundle == null)
            {
                OnAssetLoaded(null);
                return;
            }

            if(m_refBundle != null)
            {
                m_refBundle.ReduceRef();
            }
            m_refBundle = bundle;
            m_refBundle.AddRef();
        }
    }

    //public
    public class AssetsMgr : Looper
    {
        static AssetsMgr mgr = null;
        public static AssetsMgr GetInst()
        {
            if(mgr == null)
            {
                mgr = new AssetsMgr();
            }
            return mgr;
        }

        AssetsConfig m_assetsConfig = new AssetsConfig();
        List<AssetLoadRequest> m_loadRequest = new List<AssetLoadRequest>();
        AssetBundleMgr m_bundleMgr = new AssetBundleMgr();
        Dictionary<string, AssetCache> m_assetsCache = new Dictionary<string, AssetCache>();

        bool isInit = false;
        public void Init()
        {
            if (isInit) return;
            isInit = true;
            LogUtils.Log("AssetsMgr.Init()...");
            LoopUpdateMgr.GetInst().Add(this);
            m_assetsConfig.ReloadConfig();
            m_bundleMgr.Init(m_assetsConfig);
        }
        public void Uninit()
        {
            if (!isInit) return;
            isInit = false;
            ForceRelease();
            m_bundleMgr.Uninit();
            LogUtils.Log("AssetsMgr.Uninit()...");
            LoopUpdateMgr.GetInst().Remove(this);
        }

        //just for debug
        /*
        public AssetBundleMgr GetAssetBundleMgr()
        {
            return m_bundleMgr;
        } 
        */

        public void UnLoadUnUsedAsset()
        {
            Resources.UnloadUnusedAssets();
            GC.Collect();
        }

        public void AddAssetCache(string name, AssetCache cache)
        {
            m_assetsCache.Add(name, cache);
        }

        public void LoadAssetAsync(string name,Action<string, AssetCache> callback)
        {
            name = name.ToLower(); //unity res lower

            for (int i = 0; i < m_loadRequest.Count; ++i)
            {
                if (m_loadRequest[i].name.Equals(name))
                {
                    m_loadRequest[i].finishedCallback += callback;
                    return;
                }
            }

            AssetInfo info = m_assetsConfig.GetAssetInfo(name);
            AssetLoadRequest request = new AssetLoadRequest();
            request.name = name;
            request.info = info;
            request.finishedCallback += callback;
            m_loadRequest.Add(request);
        }

        public AssetCache LoadAssetSync(string name)
        {
            name = name.ToLower();  //unity res lower
            AssetCache asset = null;
            if (m_assetsCache.TryGetValue(name,out asset))
            {
                return asset;
            }
            UnityEngine.Object obj = null;
            AssetCache cache = null;
            AssetInfo info = m_assetsConfig.GetAssetInfo(name);
            if(info == null)
            {
                obj = LoadAsset(name);
                if (obj != null)
                {
                    cache = new AssetCache(name,info,obj);
                    AddAssetCache(name, cache);
                    return cache;
                }
                return null;
            }

            BundleCache bundle = m_bundleMgr.LoadAssetBundleSync(info.dependBundle);
            if (bundle == null)
                return null;

            obj = bundle.LoadAsset(AssetCache.AssetNameToBundleAssetName(info.name,info.extName));
            if (obj == null)
                return null;

            cache = new AssetCache(name, info, obj);
            AddAssetCache(name, cache);
            return cache;
        }

        public void ForceRelease()
        {
            _CleanRequest();
            _CleanAsset(true);
        }

#region private

        private void _CleanRequest()
        {
            for (int i = 0; i < m_loadRequest.Count;++i)
            {
                AssetLoadRequest request = m_loadRequest[i];
                request.OnRelease();
            }
            m_loadRequest.Clear();
        }

        private void UnLoadAsset(UnityEngine.Object obj)
        {
            Resources.UnloadAsset(obj);
        }
        private UnityEngine.Object LoadAsset(string name)
        {
            name = name.ToLower();
            int idx = name.IndexOf("assets/resources/");
            if(idx >= 0)
            {
                name.Replace("assets/resources/", "");
            }
            return UnityEngine.Resources.Load(name);
        }

        float lastHandlTime = 0.0f;
        List<AssetCache> m_temp = new List<AssetCache>();
        private void _CleanAsset(bool immediately)
        {
            //UnLoadAsset
            float timeNow = Time.time;
            if (immediately || timeNow > lastHandlTime + 30.0f)
            {
                lastHandlTime = timeNow;
                if (m_assetsCache.Count > 0)
                {
                    m_temp.Clear();
                    foreach (KeyValuePair<string, AssetCache> kv in m_assetsCache)
                    {
                        AssetMemoryType memoryType = kv.Value.GetMemoryType();
                        if (memoryType == AssetMemoryType.Auto_Clean)
                        {
                            if (immediately || timeNow > kv.Value.GetLastRefTime() + 60.0f)
                            {
                                m_temp.Add(kv.Value);
                            }
                        }
                        else if (memoryType == AssetMemoryType.Manual_Clean)
                        {
                            if (immediately)
                            {
                                m_temp.Add(kv.Value);
                            }
                        }
                    }
                    for (int i = 0; i < m_temp.Count; ++i)
                    {
                        m_temp[i].OnRelease();
                    }
                    m_temp.Clear();
                }
            }
        }

        float lastTime = Time.time;
        public void OnUpdate()
        {
            {
                //time ctrl
                float nowTime = Time.time;
                if(nowTime - lastTime > 0.080f)
                {
                    lastTime = nowTime;
                    return;
                }

                bool idle = true;
                //load asset
                if (m_loadRequest.Count > 0)
                {
                    for (int i = 0; i < m_loadRequest.Count;++i)
                    {
                        AssetLoadRequest request = m_loadRequest[i];
                        if (request.GetStatus() == AssetLoadStatus.Wait)
                        {
                            AssetCache asset = null;
                            if (m_assetsCache.TryGetValue(request.name, out asset))
                            {
                                idle = false;
                                request.OnAssetLoaded(asset);
                                break;//continue;
                            }

                            if (request.info == null || request.info.dependBundle == null)
                            {
                                UnityEngine.Object obj = LoadAsset(request.name);
                                if (obj != null)
                                {
                                    idle = false;
                                    AssetCache cache = new AssetCache(request.name, request.info,obj);
                                    AddAssetCache(request.name, cache);
                                    request.OnAssetLoaded(cache);
                                    break; //continue;
                                }
                                else
                                {
                                    request.OnAssetLoaded(null);
                                    continue;
                                }
                            }

                            BundleCache bundle = m_bundleMgr.GetBundleCached(request.info.dependBundle);
                            request.SetLoading();
                            if (bundle != null)
                            {
                                idle = false;
                                request.OnAssetBundleLoaded(request.info.dependBundle, bundle);
                                break; //continue;
                            }
                            m_bundleMgr.LoadAssetBundleAsync(request.info.dependBundle, request.OnAssetBundleLoaded);
                        }
                    }
                }

                //update
                for (int i = 0; i < m_loadRequest.Count; ++i)
                {
                    AssetLoadRequest request = m_loadRequest[i];
                    bool handled = request.UpdateState();
                    if(handled)
                    {
                        idle = false;
                        break;
                    }
                }
//__end:
                //clean request
                for (int i = 0; i < m_loadRequest.Count;)
                {
                    AssetLoadRequest request = m_loadRequest[i];
                    if (request.GetStatus() == AssetLoadStatus.Finished)
                    {
                        request.OnRelease();
                        m_loadRequest.RemoveAt(i);
                    }
                    else
                    {
                        ++i;
                    }
                }

                //load bundle
                if (idle)
                {
                    //clean asset
                    _CleanAsset(false);
                    //load bundle
                    m_bundleMgr.OnTick();
                }

                lastTime = nowTime;
            }
        }
    }
#endregion
}