using System;
using System.IO;
using System.Threading;

namespace tpf
{
    public enum DownLoadTaskStatus
    {
        Wait = 0,
        Start,
        DownLoading,
        Finished,
    }
    public class DownloadTask
    {
        private int m_id;
        private string m_url;
        private string m_name;
        private string m_signature;
        private DownLoadTaskStatus m_status = DownLoadTaskStatus.Wait;
        private Thread m_thread;
        private DownloadTempFile m_tempFile;
        private bool m_isStop = false;
        private bool m_succesed = false;
        public Action<string, bool> resultCb = null;
        private int m_retryTimes;

        public DownloadTask(int id,string url,string name,string signature)
        {
            m_id = id;
            m_url = url;
            m_name = name;
            m_signature = signature;
            m_retryTimes = 2;
        }
        public void SetTryTimes(int times)
        {
            m_retryTimes = times;
        }
        public bool IsSuccesed()
        {
            return m_succesed;
        }
        public int GetId()
        {
            return m_id;
        }
        public string GetUrl()
        {
            return m_url;
        }
        public string GetName()
        {
            return m_name;
        }
        public void OnRelease()
        {
            if (m_tempFile != null)
            {
                m_tempFile = null;
            }
            if (m_thread != null)
            {
                m_thread.Abort();
                m_thread = null;
            }
        }
        public void Update()
        {
            if(m_status == DownLoadTaskStatus.Start)
            {
                LogUtils.Log(m_name + " download begin!");
                m_status = DownLoadTaskStatus.DownLoading;
                m_thread = new Thread(OnTaskStart);
                m_thread.Start();
            }
            if(m_status == DownLoadTaskStatus.Finished)
            {
                if(!m_succesed && m_retryTimes>0 && !m_isStop)
                {
                    --m_retryTimes;
                    m_status = DownLoadTaskStatus.Wait;  //try again
                    return;
                }
                if (resultCb != null)
                {
                    resultCb(m_name, m_succesed);
                }
                resultCb = null;
            }
        }
        public bool IsRunning()
        {
            if (m_status == DownLoadTaskStatus.Start || m_status == DownLoadTaskStatus.DownLoading)
                return true;
            return false;
        }
        public bool IsWaiting()
        {
            return (m_status == DownLoadTaskStatus.Wait);
        }
        public void Start()
        {
            if (m_status < DownLoadTaskStatus.Start)
            {
                m_status = DownLoadTaskStatus.Start;
            }
        }
        public bool IsFinished()
        {
            return (m_status == DownLoadTaskStatus.Finished);
        }
        public void Stop()
        {
            if(m_tempFile != null)
            {
                m_tempFile.Stop();
            }

            if (m_thread != null)
            {
                m_thread.Abort();
                m_thread = null;
            }
            m_status = DownLoadTaskStatus.Finished;
        }
        public float GetProgress()
        {
            if (m_tempFile == null)
                return 0.0f;

            return m_tempFile.GetProgress();
        }
        void OnTaskStart()
        {
            //create temp file
            m_tempFile = new DownloadTempFile(m_url,m_name, m_signature,OnFinishedCallback);
            m_tempFile.Start();
        }
        void OnFinishedCallback(bool succ)
        {
            m_succesed = succ;
            if (m_succesed)
            {
                LogUtils.Log(m_name + " download succ!");

                try
                {
                    //todo...move or decrompress
                    string desName = PathUtils.GetPersistentPath() + m_name;
                    PathUtils.MakeSureDirExist(desName);

                    if (File.Exists(desName))
                        File.Delete(desName);

                    File.Copy(m_tempFile.GetTempFileName(), desName);
                    m_tempFile.Delete();

                    //check
                    if (m_signature.Length > 0)
                    {
                        bool vaild = FileChecker.VerifyFileSignature(desName, m_signature);
                        if (vaild == false)
                        {
                            File.Delete(desName);
                            LogUtils.LogError(m_name + " VerifyFileSignature faild!");
                        }
                        m_succesed = vaild;
                    }
                }
                catch (System.Exception ex)
                {
                    m_succesed = false;
                    LogUtils.LogError(ex.ToString());
                }
            }
            else
            {
                LogUtils.LogError(m_name + " download faild!");
            }
            m_status = DownLoadTaskStatus.Finished;
        }
    }
}
