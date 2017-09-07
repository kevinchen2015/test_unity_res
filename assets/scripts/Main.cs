using UnityEngine;
using System.Collections;
using tpf;

public class Main : MonoBehaviour
{
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

	void Start ()
    {
        PathUtils.Init();
        LogUtils.Init(PathUtils.GetPersistentPath());

        LogUtils.Log("main start!");

        CoroutineTaskMgr mgr = gameObject.GetComponent<CoroutineTaskMgr>();
        if(mgr == null)
        {
            mgr = gameObject.AddComponent<CoroutineTaskMgr>();
        }
        LoopUpdateMgr looper = gameObject.GetComponent<LoopUpdateMgr>();
        if (looper == null)
        {
            looper = gameObject.AddComponent<LoopUpdateMgr>();
        }
        LoopUpdateMgr.GetInst().Init();
        CoroutineTaskMgr.GetInst().Init();
        VersionUpdate.GetIns().Init();
        VersionUpdate.GetIns().StartStatus(VesionUpdateStatus.Game);
    }
    void OnDestroy()
    {
        
        AssetsMgr.GetInst().Uninit();

        CoroutineTaskMgr.GetInst().Uninit();
        LoopUpdateMgr.GetInst().Uninit();
        LogUtils.Log("main ondestory!");
        LogUtils.Uninit();
    }
    void Update ()
    {
        VersionUpdate.GetIns().Update();
        LogUtils.Update();
    }
}
