using System;

namespace tpf
{
    public enum VesionUpdateStatus
    {
        AppChannelCheck = 0,
        GameVersionCheck,
        ResDownload,
        Game,
    }
    public class VersionUpdate
    {
        static VersionUpdate inst = null;

        private StatusMgr m_statusMgr = new StatusMgr();
        private VersionRes m_verRes = new VersionRes();
        public int GetChannelID()
        {
            return m_verRes.channelId;
        }
        public string GetVersion()
        {
            return m_verRes.version;
        }
        public void SetVersion(VersionRes ver)
        {
            m_verRes = ver;
        }
        public VersionRes GetVersionRes()
        {
            return m_verRes;
        }
        public static VersionUpdate GetIns()
        {
            if (inst == null)
            {
                inst = new VersionUpdate();
            }
            return inst;
        }
        public void Init()
        {
            m_statusMgr.RegisterStatus(new AppChannelStatus());
            m_statusMgr.RegisterStatus(new GameVersionStatus());
            m_statusMgr.RegisterStatus(new ResDownloadStatus());
            m_statusMgr.RegisterStatus(new GameStatus());

            DownloadMgr.GetInst().Init();
            m_statusMgr.Init();

            string verFile = PathUtils.GetVaildFullPath("version.json");
            if(verFile.Length > 0)
            {
                m_verRes = SerializeHelper.LoadFromFile<VersionRes>(verFile);
            }
            
            if(m_verRes == null)
            {
                LogUtils.LogError("verFile can not find!");
                return;
            }
        }
        public void UnInit()
        {
            DownloadMgr.GetInst().Uninit();
            m_statusMgr.Uninit();
        }
        public void Update()
        {
            DownloadMgr.GetInst().Update();
            m_statusMgr.Update();
        }
        public void StartStatus(VesionUpdateStatus status)
        {
            m_statusMgr.ChangeStatus((int)status);
        }
        public bool UpdateOneRes(string name,Action<string,bool> cb)
        {
            for (int i = 0; i < m_verRes.versionResFile.Count; ++i)
            {
                if(m_verRes.versionResFile[i].name.Equals(name))
                {
                    //不用校验了，启动游戏的时候已经全量校验过了
                    DownloadMgr.GetInst().CreateTask(m_verRes.versionResFile[i].name, m_verRes.versionResFile[i].signature, cb);
                    return true;
                }
            }
            return false;
        }

    }
}