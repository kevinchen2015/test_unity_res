using UnityEngine;
using System;
using System.Collections;

namespace tpf
{
    public class CoroutineTaskMgr : MonoBehaviour
    {
        static CoroutineTaskMgr Inst = null;
    
        public static CoroutineTaskMgr GetInst()
        {
            return Inst;
        }

        public void Init()
        {
            LogUtils.Log("CoroutineTaskMgr.Init()");
        }
        public void Uninit()
        {
            if (Inst == null) return;

            LogUtils.Log("CoroutineTaskMgr.Uninit()");
            Clean();
            Inst = null;
        }
        public void AddTask(string name, IEnumerator routine)
        {
            LogUtils.Log("AddTask(),name:", name);
            StartCoroutine(routine);
        }
        public void RemoveTask(string name, IEnumerator routine)
        {
            LogUtils.Log("RemoveTask(),name:", name);
            StopCoroutine(routine);
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

        }
        void Clean()
        {
          
        }
        void OnDestroy()
        {
            Uninit();
            Inst = null;
        }
    }
}
