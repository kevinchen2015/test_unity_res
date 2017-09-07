using System;
using System.IO;
using System.Net;
using System.Threading;


namespace tpf
{
    public class DownloadTempFile
    {
        static string downloadTempPath = "temp/";
        static string downloadCfgExt = ".dlcfg";
        static string downloadTempFileExt = ".dl";

        private string m_url;
        private string m_name;
        private string m_signature;
        private string m_cfg;
        private string m_temp;
        private HttpDownloadTempFileHelper tempFileHelper = new HttpDownloadTempFileHelper();
        private Action<bool> m_callback;
        private Action m_failedCb;

        public DownloadTempFile(string url,string name,string signature,Action<bool> cb)
        {
            m_url = url;
            m_name = name;
  
            m_signature = signature;
            m_callback = cb;

            string downloadPath = PathUtils.GetPersistentPath() + downloadTempPath;
            if(!Directory.Exists(downloadPath))
            {
                Directory.CreateDirectory(downloadPath);
            }

            m_cfg = downloadPath + name + downloadCfgExt;
            m_temp = downloadPath + name + downloadTempFileExt;

            PathUtils.MakeSureDirExist(m_temp);
        }
        public string GetTempFileName()
        {
            return m_temp;
        }
        public void Stop()
        {
            tempFileHelper.Stop();
        }
        public void Delete()
        {
            tempFileHelper.DeleteFile(m_cfg, m_temp);
        }
        public float GetProgress()
        {
            if (tempFileHelper == null || tempFileHelper.GetTotalSize() == 0)
                return 0.0f;

            return (float)tempFileHelper.GetCurrentPos() / (float)tempFileHelper.GetTotalSize();
        }
        public void Start()
        {
            string url = m_url + m_name;
            tempFileHelper.DoDownload(m_cfg, m_temp, m_signature, url, TempFileDownloadCb);
        }
        void TempFileDownloadCb(bool succ)
        {
            if (m_callback != null)
            {
                m_callback(true);
            }
        }
    }

    public class HttpDownloadTempFileHelper
    {
        string m_signature;
        long m_totalSize;
        long m_currentPos;
        FileStream m_tempFile;
        static int downLoadblockSize = 32 * 1024;
        static int writeBlockSize = 8 * 1024;
        bool m_isStop = false;
    
        ~HttpDownloadTempFileHelper()
        {
            Close();
        }
        public long GetCurrentPos()
        {
            return m_currentPos;
        }
        public long GetTotalSize()
        {
            return m_totalSize;
        }
        public void DoDownload(string cfg, string temp, string signature,string url, Action<bool>callback)
        {
            LogUtils.Log("download:", url);
            try
            {
                {
                    Uri requestUrl = new Uri(url);
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestUrl);
                    request.Method = "HEAD";
                    request.Timeout = 2000;
                    HttpWebResponse respone = (HttpWebResponse)request.GetResponse();
                    m_totalSize = respone.ContentLength;
                    respone.Close();
                    request.Abort();
                }
                Check(cfg, temp, m_totalSize, signature);
                while(m_currentPos < m_totalSize && !m_isStop)
                {
                    Thread.Sleep(3);

                    if (m_currentPos < m_totalSize)
                    {
                        Uri requestUrl = new Uri(url);
                        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestUrl);
                        request.Method = "GET";
                        request.Timeout = 2000;
                        int endPos = (int)m_currentPos + downLoadblockSize;
                        if(endPos >= m_totalSize)
                        {
                            endPos = (int)m_totalSize - 1;
                        }
                        request.AddRange((int)m_currentPos, endPos);
                        HttpWebResponse respone = (HttpWebResponse)request.GetResponse();
                        WriteData(respone);
                        respone.Close();
                        request.Abort();
                    }
                }
                this.Close();
                if(callback != null)
                    callback(true);
            }
            catch (System.Exception ex)
            {
                this.Close();
                LogUtils.LogError(ex.ToString());

                if (callback != null)
                    callback(false);
            }
        }
        bool Check(string cfgName, string dlName, long totalSize, string newSignature)
        {
            if (m_tempFile != null)
            {
                m_tempFile.Close();
                m_tempFile = null;
            }
            m_isStop = false;
            bool deleteFile = false;
            if (File.Exists(cfgName))
            {
                FileStream file = new FileStream(cfgName, FileMode.Open, FileAccess.Read);
                StreamReader sr = new StreamReader(file);
                string str = sr.ReadLine();
                if (str == null)
                {
                    deleteFile = true;
                }
                else
                {
                    m_signature = str;
                    if (!newSignature.Equals(m_signature) || newSignature.Length == 0)
                    {
                        deleteFile = true;
                    }
                }
                sr = null;
                file.Close();
            }
            if (deleteFile)
            {
                if (File.Exists(dlName))
                    File.Delete(dlName);

                if (File.Exists(cfgName))
                    File.Delete(cfgName);
            }
            m_totalSize = totalSize;
            m_signature = newSignature;
            if (!File.Exists(cfgName))
            {
                FileStream file = new FileStream(cfgName, FileMode.OpenOrCreate, FileAccess.Write);
                StreamWriter w = new StreamWriter(file);
                w.WriteLine(m_signature);
                w.WriteLine(m_totalSize.ToString());
                w.Flush();
                file.Flush();
                file.Close();
            }
            if (File.Exists(dlName))
            {
                FileInfo fileInfo = new FileInfo(dlName);
                m_currentPos = fileInfo.Length;
                fileInfo = null;

                if (m_currentPos > m_totalSize)
                {
                    File.Delete(dlName);
                    m_currentPos = 0;
                    m_tempFile = new FileStream(dlName, FileMode.OpenOrCreate, FileAccess.Write);
                }
                else
                {
                    m_tempFile = new FileStream(dlName, FileMode.Append, FileAccess.Write);
                }
            }
            else
            {
                m_currentPos = 0;
                m_tempFile = new FileStream(dlName, FileMode.OpenOrCreate, FileAccess.Write);
            }
            return true;
        }
        void WriteData(WebResponse respon)
        {
            byte[] blockBuff = new byte[writeBlockSize];
            int offset = 0;
            var respStream = respon.GetResponseStream();
            int receivedBytesCount = 0;
            do
            {
                receivedBytesCount = respStream.Read(blockBuff, 0, writeBlockSize);

                if (receivedBytesCount > 0)
                    m_tempFile.Write(blockBuff, 0, receivedBytesCount);

                offset += receivedBytesCount;
                m_currentPos += receivedBytesCount;
            } while (receivedBytesCount != 0 || m_isStop);
            //m_tempFile.Flush();
        }
        public void Stop()
        {
            m_isStop = true;
        }
        public void Close()
        {
            if (m_tempFile != null)
            {
                m_tempFile.Flush();
                m_tempFile.Close();
                m_tempFile = null;
            }
        }
        public void DeleteFile(string cfgName, string dlName)
        {
            if (File.Exists(dlName))
                File.Delete(dlName);

            if (File.Exists(cfgName))
                File.Delete(cfgName);
        }
    }
}