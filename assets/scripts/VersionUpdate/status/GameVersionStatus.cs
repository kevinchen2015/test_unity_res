using UnityEngine;

namespace tpf
{
    public class GameVersionStatus : Status
    {
        VersionRes m_verRes;
        public override int GetStatusID()
        {
            return (int)VesionUpdateStatus.GameVersionCheck;
        }
        public override void OnInit(StatusMgr mgr)
        {
			base.OnInit(mgr);
        }
        public override void OnEnter(Status oldStatus)
        {
            LogUtils.Log("enter get version info status");
            //string timeNow = System.DateTime.Now.ToLocalTime().ToString();
            DownloadMgr.GetInst().CreateTask("version.json","", DownloadResult);
        }
        void DownloadResult(string name,bool isSucc)
        {
            if(isSucc)
            {
                DoCheck();
            }
            else
            {
                LogUtils.LogError("download file failed!");
            }
        }
        void DoCheck()
        {
            string verFile = PathUtils.GetVaildFullPath("version.json");
            m_verRes = SerializeHelper.LoadFromFile<VersionRes>(verFile);

            //这个大版本要写在程序里，才能保证获取的本地当前版本是对的！！！
            string[] programVersion = VersionResDef.version.Split('.');

            string[] oldVersion = VersionUpdate.GetIns().GetVersion().Split('.');
            string[] newVersion = m_verRes.version.Split('.');
            string[] miniVersion = m_verRes.miniVersion.Split('.');

            //强制更新apk,ios
            if(int.Parse(programVersion[0]) < int.Parse(miniVersion[0])
                || int.Parse(programVersion[1]) < int.Parse(miniVersion[1])
                )
            {
                if (Application.platform == RuntimePlatform.Android)
                {
                    DownloadMgr.GetInst().CreateTask(m_verRes.apk,m_verRes.apkSignature,DownloadApkResult);
                }
                else if (Application.platform == RuntimePlatform.IPhonePlayer)
                {
                    //todo...
                }
                else
                {
                    //todo...
                }
                return;  
            }

            int oldBigVersion = int.Parse(programVersion[0]);
            int newBigVersion = int.Parse(newVersion[0]);
            if (newBigVersion > oldBigVersion)
            {
                //todo...可选更新apk,ios
            }

            VersionUpdate.GetIns().SetVersion(m_verRes);
            m_mgr.ChangeNextStatus();
        }
        void DownloadApkResult(string name, bool isSucc)
        {
            if(isSucc)
            {
                string apkPath = PathUtils.GetPersistentPath() + m_verRes.apk;
                Application.OpenURL(apkPath);
                Application.Quit();
            }
        }
        public override void OnExit(Status newStatus)
        {
            DownloadMgr.GetInst().StopAllTask();
        }
        public override void OnUnit()
        {
            base.OnUnit();
        }
    }

}