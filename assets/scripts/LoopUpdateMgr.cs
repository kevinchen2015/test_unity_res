using UnityEngine;
using System.Collections.Generic;

namespace tpf
{
    public interface Looper
    {
        void OnUpdate();
    }

    public class LoopUpdateMgr : MonoBehaviour
    {
        static LoopUpdateMgr Inst = null;

        public static LoopUpdateMgr GetInst()
        {
            return Inst;
        }

        private List<Looper> m_looper = new List<Looper>();
        private List<Looper> m_temp = new List<Looper>();
        public void Init()
        {
            LogUtils.Log("LoopUpdateMgr.Init()");
        }
        public void Uninit()
        {
            if (Inst == null) return;

            Clean();
            Inst = null;
        }
        public void Add(Looper looper, string name = null)
        {
            m_looper.Add(looper);
        }
        public void Remove(Looper looper, string name = null)
        {
            m_looper.Remove(looper);
        }
        void Awake()
        {
            Inst = this;
        }
        void Start()
        {
            
        }
        void Update()
        {
            m_temp.Clear();
            for (int i = 0;i < m_looper.Count;++i)
            {
                m_temp.Add(m_looper[i]);
            }
            for(int i = 0;i < m_temp.Count;++i)
            {
                m_temp[i].OnUpdate();
            }
        }
        void Clean()
        {
            m_looper.Clear();
        }
        void OnDestroy()
        {
            
            Uninit();
            Inst = null;
        }
    }
}
