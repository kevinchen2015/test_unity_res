using System;
using System.Collections.Generic;

namespace tpf
{
    public class DownloadMgr 
    {
        static DownloadMgr inst;
        List<DownloadTask> m_taskList = new List<DownloadTask>();
        int m_taskIdx = 0;
        string m_baseUrl = "http://10.0.0.252:8080/";
        int m_maxTaskNum = 1;

        public static DownloadMgr GetInst()
        {
            if(inst == null)
            {
                inst = new DownloadMgr();
            }
            return inst;
        }
        public void Init()
        {

        }
        public void Uninit()
        {
            StopAllTask();
        }
        public void SetBaseURL(string baseUrl)
        {
            m_baseUrl = baseUrl;
        }
        public void SetMaxTaskNum(int num)
        {
            m_maxTaskNum = num;
        }
        public void Update()
        {
            int runningNum = 0;
            for (int i = 0; i < m_taskList.Count; ++i)
            {
                m_taskList[i].Update();
                if(m_taskList[i].IsRunning())
                {
                    ++runningNum;
                }
            }
            for (int i = 0; i < m_taskList.Count; )
            {
                if(m_taskList[i].IsFinished())
                {
                    m_taskList[i].OnRelease();
                    m_taskList.RemoveAt(i);
                }
                else
                {
                    ++i;
                }
            }
            int startNum = m_maxTaskNum - runningNum;
            if(startNum > 0)
            {
                for (int i = 0; i < m_taskList.Count; ++i)
                {
                    if (m_taskList[i].IsWaiting())
                    {
                        m_taskList[i].Start();
                        if(--startNum <= 0)
                        {
                            break;
                        }
                    }
                }
            }
        }
        DownloadTask GetTaskByUrl(string url,string name)
        {
            for(int i = 0;i < m_taskList.Count;++i)
            {
                if(m_taskList[i].GetName().Equals(name))
                {
                    return m_taskList[i];
                }
            }
            return null;
        }
        public DownloadTask GetTask(int id)
        {
            for (int i = 0; i < m_taskList.Count; ++i)
            {
                if (m_taskList[i].GetId() == id)
                {
                    return m_taskList[i];
                }
            }
            return null;
        }
        public int CreateTask(string name,string signature, Action<string, bool> cb)
        {
            DownloadTask task = GetTaskByUrl(m_baseUrl,name);
            if(task != null)
            {
                if(cb != null)
                    task.resultCb += cb;
                return task.GetId();
            }
            int id = ++m_taskIdx;
            task = new DownloadTask(id, m_baseUrl,name, signature);
            if (cb != null)
                task.resultCb += cb;
            m_taskList.Add(task);
            return id;
        }
        public void StopTask(int id)
        {
            DownloadTask task = GetTask(id);
            if (task == null)
                return;

            task.Stop();
            m_taskList.Remove(task);
        }
        public void StopAllTask()
        {
            for(int i = 0;i < m_taskList.Count;++i)
            {
                m_taskList[i].Stop();
            }
            m_taskList.Clear();
        }
    }
}
