using System;
using UnityEngine;
using System.Collections.Generic;


namespace tpf
{
    public class BundleCache
    {
        public string name;
        public AssetBundle bundle;
        public BundleInfo info;
        public float lastRefTime;

        public AssetMemoryType memoryType = AssetMemoryType.Auto_Clean;
        private int m_refCount;
        private List<BundleCache> m_refBundle = null;
        public int GetRef()
        {
            return m_refCount;
        }
        public void Access()
        {
            lastRefTime = Time.time;
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
            if(m_refCount==0)
            {
                //LogUtils.Log("GetRef() == 0");
            }
        }
        public bool HasDependBundle()
        {
            return (m_refBundle != null && m_refBundle.Count > 0);
        }
        public void AddDependBundle(BundleCache bundle)
        {
            if(m_refBundle == null)
            {
                m_refBundle = new List<BundleCache>();
            }
            if (m_refBundle.Contains(bundle))
            {
                return;
            }
            m_refBundle.Add(bundle);
            bundle.AddRef();
        }
        public void RemoveDependBundle(BundleCache bundle)
        {
            if (m_refBundle.Contains(bundle))
            {
                m_refBundle.Remove(bundle);
                bundle.ReduceRef();
                return;
            }
        }
        public void OnRelease()
        {
            if (m_refBundle != null)
            {
                for (int i = 0; i < m_refBundle.Count; ++i)
                {
                    m_refBundle[i].ReduceRef();
                }
                m_refBundle.Clear();
                m_refBundle = null;
            }
            if (bundle != null)
            {
                bundle.Unload(false);
                bundle = null;
            }
        }

        public UnityEngine.Object LoadAsset(string assetName)
        {
            return bundle.LoadAsset(assetName);
        }

        public UnityEngine.Object[] LoadAllAssets()
        {
            return bundle.LoadAllAssets();
        }

    }

    public class BundleLoadRequest
    {
        public string name;
        public Action<string, BundleCache> finishedCallback;
        public BundleCache self = null;
        public BundleInfo info;

        private AssetLoadStatus m_status = AssetLoadStatus.Wait;
        private List<BundleCache> m_refBundle = null;
        private List<string> m_unRefDependName = null;

        public BundleLoadRequest(string name, BundleInfo info, Action<string, BundleCache> callback)
        {
            this.name = name;
            this.info = info;
            if(callback != null)
                this.finishedCallback += callback;

            m_unRefDependName = null;

            //depend
            if(info.HasDependBundle())
            {
                if (m_refBundle == null)
                {
                    m_refBundle = new List<BundleCache>();
                }
                if (m_unRefDependName == null)
                {
                    m_unRefDependName = new List<string>();
                    for (int i = 0; i < info.dependBundle.Count; ++i)
                    {
                        m_unRefDependName.Add(info.dependBundle[i]);
                    }
                }
            }
        }

        public AssetLoadStatus GetStatus()
        {
            return m_status;
        }
        public void OnAssetBundleLoadFinished(string name, BundleCache bundle)
        {
            //failed
            if(bundle == null)
            {
                m_status = AssetLoadStatus.Finished;
                if(finishedCallback != null)
                    finishedCallback(name, null);
                return;
            }
            //self
            if(name.Equals(this.name))
            {
                if (self != null)
                    self.ReduceRef();

                self = bundle;
                self.AddRef();
            }     
            //no depend ab     
            if(!info.HasDependBundle())
            {
                m_status = AssetLoadStatus.Finished;
                if (finishedCallback != null)
                    finishedCallback(this.name, self);
                return;
            }
            m_status = AssetLoadStatus.Loading;
            if (name.Equals(this.name))
            {
                return;
            }
            //depend
            if(!info.IsDependBundle(name))
            {
                LogUtils.LogWarning("IsDependBundle() == false");
                return;
            }
            if(!m_refBundle.Contains(bundle))
            {
                m_refBundle.Add(bundle);
                bundle.AddRef();
            }
            m_unRefDependName.Remove(name);
            //load sucess
            if (info.GetDependBundleCount() == m_refBundle.Count && self != null)
            {
                for(int i = 0;i < m_refBundle.Count;++i)
                {
                    self.AddDependBundle(m_refBundle[i]);
                }

                m_status = AssetLoadStatus.Finished;
                if (finishedCallback != null)
                    finishedCallback(this.name, self);

                _CleanRef();
            }
        }

        List<string> temp = new List<string>();
        public List<string> GetUnRefDependName()
        {
            if (m_unRefDependName == null) return null;
            temp.Clear();
            for (int i = 0; i < m_unRefDependName.Count; ++i)
                temp.Add(m_unRefDependName[i]);
            return temp;
        }

        private void _CleanRef()
        {
            if (m_refBundle != null)
            {
                for (int i = 0; i < m_refBundle.Count; ++i)
                {
                    m_refBundle[i].ReduceRef();
                }
                m_refBundle.Clear();
                m_refBundle = null;
            }
        }

        public void OnRelease()
        {
            finishedCallback = null;
            if(self != null)
                self.ReduceRef();

            _CleanRef();
            self = null;
            info = null;
        }

        public bool DownLoad()
        {
            bool b = VersionUpdate.GetIns().UpdateOneRes(name, DownloadResult);
            if(b == true)
            {
                m_status = AssetLoadStatus.Downloading;
            }
            return b;
        }

        void DownloadResult(string name, bool isSucc)
        {
            m_status = AssetLoadStatus.Downloaded;
        }
    }

    public class AssetBundleMgr
    {
        List<BundleLoadRequest> m_loadRequest = new List<BundleLoadRequest>();
        Dictionary<string, BundleCache> m_bundleCache = new Dictionary<string, BundleCache>();
        AssetsConfig m_assetCfg;
        bool m_useDownload = true;
        public void Init(AssetsConfig assetCfg)
        {
            m_assetCfg = assetCfg;
        }
 
        public void Uninit()
        {
            ForceRelease();
            m_assetCfg = null;
        }

        public BundleCache LoadAssetBundleSync(string name)
        {
            BundleInfo info = m_assetCfg.GetBundleInfo(name);
            if (info == null)
            {
                return null;
            }

            //find cache
            BundleCache bundle = null;
            if (m_bundleCache.TryGetValue(name, out bundle))
            {
                bundle.Access();
                return bundle;
            }

            //load depend
            _AddDependAbSync(info);

            if (m_bundleCache.TryGetValue(name, out bundle))
            {
                bundle.Access();
                return bundle;
            }
            return null;
        }

        public BundleCache GetBundleCached(string name)
        {
            BundleCache bundle = null;
            if (m_bundleCache.TryGetValue(name, out bundle))
            {
                bundle.Access();
                return bundle;
            }
            return null;
        }

        public void LoadAssetBundleAsync(string name,Action<string, BundleCache> callback)
        {
            BundleInfo info = m_assetCfg.GetBundleInfo(name);
            if (info == null)
            {
                if(callback != null)
                    callback(name, null);
                return;
            }

            //find cache
            BundleCache bundle = null;
            if(m_bundleCache.TryGetValue(name, out bundle))
            {
                bundle.Access();
                callback(name, bundle);
                return;
            }

            //find request
            for(int i=0;i < m_loadRequest.Count;++i)
            {
                if(m_loadRequest[i].name.Equals(name))
                {
                    m_loadRequest[i].finishedCallback += callback;
                    return;
                }
            }

            //add depend ab
            BundleLoadRequest request = new BundleLoadRequest(name,info,callback);

            _AddDependAb(request);

            //add self
            m_loadRequest.Add(request);
        }

        public void ForceRelease()
        {
            _CleanRequest();
            _ReleaseAb(true);
        }

#region private

        private void _AddDependAbSync(BundleInfo info)
        {
            if (info == null)
            {
                return;
            }

            //find cache
            if (info.HasDependBundle())
            {
                for (int i = 0; i < info.dependBundle.Count; ++i)
                {
                    BundleInfo dependInfo = m_assetCfg.GetBundleInfo(info.dependBundle[i]);
                    //find cache
                    BundleCache bundle = null;
                    if (!m_bundleCache.TryGetValue(dependInfo.name, out bundle))
                    {
                        //load depend
                        _AddDependAbSync(dependInfo);
                    }
                    else
                    {
                        bundle.Access();
                    }
                }
            }

            //load
            AssetBundle ab = _LoadAssetBundleSync(info.name);
            if (ab != null)
            {
                BundleCache bundle = new BundleCache();
                bundle.name = info.name;
                bundle.info = info;
                bundle.bundle = ab;
                bundle.Access();
                m_bundleCache.Add(info.name, bundle);
            }
            else
            {
                if (m_useDownload)
                {
                    VersionUpdate.GetIns().UpdateOneRes(info.name, null);
                }
            }
        }

        private AssetBundle _LoadAssetBundleSync(string name)
        {
            string fullPath = PathUtils.GetVaildFullPath(name);
            if (fullPath.Length == 0)
            {
                return null;
            }
            return AssetBundle.LoadFromFile(fullPath);
        }

        private void _AddDependAb(BundleLoadRequest request)
        {
            if (!request.info.HasDependBundle())
                return;

            for(int i = 0;i < request.info.dependBundle.Count;++i)
            {
                bool needLoad = true;
                BundleInfo dependInfo = m_assetCfg.GetBundleInfo(request.info.dependBundle[i]);
                //find cache
                BundleCache bundle = null;
                if (m_bundleCache.TryGetValue(dependInfo.name, out bundle))
                {
                    needLoad = false;
                    request.OnAssetBundleLoadFinished(dependInfo.name,bundle);
                }
                else
                {
                    //find request
                    for (int k = 0; k < m_loadRequest.Count; ++k)
                    {
                        if (m_loadRequest[k].name.Equals(dependInfo.name))
                        {
                            needLoad = false;
                            break;
                        }
                    }
                }

                if(needLoad)
                {
                    //add depend ab
                    BundleLoadRequest depRequest = new BundleLoadRequest(dependInfo.name, dependInfo,null);

                    _AddDependAb(depRequest);

                    //add self
                    m_loadRequest.Add(depRequest);
                }
            }
        }

        float lastHandlTime = 0.0f;
        List<BundleCache> m_temp = new List<BundleCache>();
        private void _ReleaseAb(bool immediately)
        {
            //release ab
            float timeNow = Time.time;
            if (immediately || timeNow > lastHandlTime + 10.0f)
            {
                lastHandlTime = timeNow;
                if (m_bundleCache.Count > 0)
                {
                    m_temp.Clear();
                    foreach (KeyValuePair<string, BundleCache> kv in m_bundleCache)
                    {
                        AssetMemoryType memoryType = kv.Value.memoryType;
                        if (memoryType == AssetMemoryType.Auto_Clean)
                        {
                            if (immediately || timeNow > kv.Value.lastRefTime + 15.0f)
                            {
                                m_temp.Add(kv.Value);
                            }
                        }
                        else if(memoryType == AssetMemoryType.Manual_Clean)
                        {
                            if(immediately)
                            {
                                m_temp.Add(kv.Value);
                            }
                        }
                    }
                    for(int i =0;i < m_temp.Count;++i)
                    {
                        m_temp[i].OnRelease();
                    }
                    m_temp.Clear();
                }
            }
        }

        private void _CleanRequest()
        {
            for (int i = 0; i < m_loadRequest.Count; ++i)
            {
                BundleLoadRequest request = m_loadRequest[i];
                request.OnRelease();
            }
            m_loadRequest.Clear();
        }
        
        public void OnTick()
        {
            bool isIdle = true;
            //load ab
            for (int i = 0; i < m_loadRequest.Count;++i)
            {
                if(m_loadRequest[i].GetStatus() == AssetLoadStatus.Wait ||
                  m_loadRequest[i].GetStatus() == AssetLoadStatus.Downloaded)
                {
                    string name = m_loadRequest[i].name;
                    BundleCache cache = null;
                    if (m_bundleCache.TryGetValue(name, out cache))
                    {
                        m_loadRequest[i].OnAssetBundleLoadFinished(name, cache);
                        continue;
                    }
                    AssetBundle ab = _LoadAssetBundleSync(name);
                    if(ab == null)
                    {
                        bool isFaild = true;
                        if (m_useDownload && m_loadRequest[i].GetStatus() != AssetLoadStatus.Downloaded)
                        {
                            isFaild = !m_loadRequest[i].DownLoad();
                        }
                        
                        if(isFaild)
                        {
                            m_loadRequest[i].OnAssetBundleLoadFinished(name, null);
                        }
                    }
                    else
                    {
                        isIdle = false;

                        cache = new BundleCache();
                        cache.name = name;
                        cache.info = m_loadRequest[i].info;
                        cache.bundle = ab;
                        cache.Access();
                        m_bundleCache.Add(name, cache);
                        m_loadRequest[i].OnAssetBundleLoadFinished(name, cache);
                        break;  //do one!
                    }
                }
            }

            //check 
            for (int i = 0; i < m_loadRequest.Count; ++i)
            {
                if (m_loadRequest[i].GetStatus() == AssetLoadStatus.Loading)
                {
                    isIdle = false;
                    List<string> unRefDepend = m_loadRequest[i].GetUnRefDependName();
                    if(unRefDepend != null)
                    {
                        for(int k = 0;k < unRefDepend.Count;++k)
                        {
                            BundleCache cache = null;
                            if (m_bundleCache.TryGetValue(unRefDepend[k], out cache))
                            {
                                m_loadRequest[i].OnAssetBundleLoadFinished(unRefDepend[k], cache);
                            }
                        }
                    }
                }
            }

//__end:
            //clear finished request
            for (int i = 0; i < m_loadRequest.Count;)
            {
                BundleLoadRequest request = m_loadRequest[i];
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

            if (isIdle)
            {
                _ReleaseAb(false);
            }

           
        }
        #endregion

    }

}