using System.Collections.Generic;
using System.IO;

namespace tpf
{
    public class ResDownloadStatus : Status
    {
        List<string> m_downloadFile = new List<string>();
        List<string> m_downloadSuccFile = new List<string>();

        public override int GetStatusID()
        {
            return (int)VesionUpdateStatus.ResDownload;
        }
        public override void OnInit(StatusMgr mgr)
        {
			base.OnInit(mgr);
        }
        public override void OnEnter(Status oldStatus)
        {
            LogUtils.Log("enter res file check and update status");

            //check 
            VersionRes verRes = VersionUpdate.GetIns().GetVersionRes();
            DownloadMgr.GetInst().SetBaseURL(verRes.url);
            DownloadMgr.GetInst().SetMaxTaskNum(3);

            m_downloadFile.Clear();
            m_downloadSuccFile.Clear();
            for (int i = 0; i < verRes.versionResFile.Count; ++i)
            {
                VersionResFile resFile = verRes.versionResFile[i];

                if(resFile.resType == VersionResType.PreDownload)
                {
                    string fullPath = PathUtils.GetVaildFullPath(resFile.name);
                    if (fullPath.Length == 0 || !FileChecker.VerifyFileSignature(fullPath, resFile.signature))
                    {
                        m_downloadFile.Add(resFile.name);
                        DownloadMgr.GetInst().CreateTask(resFile.name, verRes.versionResFile[i].signature, DownloadResult);
                    }
                }
                else
                {
                    string fileName = PathUtils.GetPersistentPath() + resFile.name;
                    if(fileName.Length > 0 && File.Exists(fileName))
                    {
                        if(!FileChecker.VerifyFileSignature(fileName, resFile.signature))
                        {
                            File.Delete(fileName);
                        }
                    }
                }
            }
            CheckFinish();
        }
        void DownloadResult(string name, bool isSucc)
        {
            if(isSucc)
            {
                //LogUtils.Log("download file succ! name:",name);
                m_downloadSuccFile.Add(name);
                CheckFinish();
            }
            else
            {
                //LogUtils.LogError("download file failed! name:",name);
            }
        }
        void CheckFinish()
        {
            if(m_downloadSuccFile.Count == m_downloadFile.Count)
            {
                LogUtils.Log("all res have done!");
                m_mgr.ChangeNextStatus();
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