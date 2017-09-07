using System.Collections.Generic;

namespace tpf
{
    public class Status
    {
        protected StatusMgr m_mgr;
        public virtual int GetStatusID()
        {
            return 0;
        }
        public virtual void OnInit(StatusMgr mgr)
        {
            m_mgr = mgr;
        }
        public virtual void OnEnter(Status oldStatus)
        {

        }
        public virtual void OnExit(Status newStatus)
        {

        }
        public virtual void OnUnit()
        {
            m_mgr = null;
        }
    }

    public class StatusMgr
    {
        Dictionary<int, Status> m_statusDic = new Dictionary<int, Status>();
        Status m_currentStatus;
        public void RegisterStatus(Status status)
        {
            m_statusDic.Add(status.GetStatusID(), status);
        }
        public void Init()
        {
            foreach (KeyValuePair<int, Status> kv in m_statusDic)
            {
                kv.Value.OnInit(this);
            }
        }
        public void Uninit()
        {
            foreach (KeyValuePair<int, Status> kv in m_statusDic)
            {
                kv.Value.OnUnit();
            }
            m_statusDic.Clear();
        }
        public Status GetCurrentState()
        {
            return m_currentStatus;
        }
        int m_nextStatusId = -1;
        public void Update()
        {
            if(m_nextStatusId >= 0)
            {
                int next = m_nextStatusId;
                m_nextStatusId = -1;
                _ChangeStatus(next);
            }
        }
        //one way to use change status
        public void ChangeStatus(int newStatusId)
        {
            m_nextStatusId = newStatusId;
        }
        //another way to use change state
        public void ChangeNextStatus()
        {
            int nextId = 0;
            if(m_currentStatus == null)
            {
                nextId = 0;
            }
            else
            {
                nextId = m_currentStatus.GetStatusID()+1;
            }
            m_nextStatusId = nextId;
        }
        void _ChangeStatus(int newStatusId)
        {
            Status newStatus = null;
            if (!m_statusDic.TryGetValue(newStatusId, out newStatus))
            {
                return;
            }
            Status oldStatus = GetCurrentState();
            m_currentStatus = newStatus;
            if (oldStatus != null)
            {
                oldStatus.OnExit(newStatus);
            }
            newStatus.OnEnter(oldStatus);
        }
    }

}