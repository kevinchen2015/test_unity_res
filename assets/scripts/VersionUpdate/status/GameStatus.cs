using UnityEngine;

namespace tpf
{
    public class GameStatus : Status
    {
        public override int GetStatusID()
        {
            return (int)VesionUpdateStatus.Game;
        }
        public override void OnInit(StatusMgr mgr)
        {
			base.OnInit(mgr);
        }
        public override void OnEnter(Status oldStatus)
        {
            AssetsMgr.GetInst().Init();
            LogUtils.Log("enter game status!");

            //sync
            //UnityEngine.Object obj = AssetsUtils.LoadAssetSync("character/models/human/1210001/1210001_2");
            //GameObject o = GameObject.Instantiate(obj) as GameObject;

            //async
            AssetsUtils.LoadAssetAync("character/models/human/1230008/1230008_1", OnAssetLoaded);
        }

        void OnAssetLoaded(string name, UnityEngine.Object obj)
        {
            GameObject o = GameObject.Instantiate(obj) as GameObject;
        }


        public override void OnExit(Status newStatus)
        {
            AssetsMgr.GetInst().Uninit();
        }
        public override void OnUnit()
        {
            base.OnUnit();
        }
    }

}