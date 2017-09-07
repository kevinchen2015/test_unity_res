using System;
using System.Net;
using UnityEngine;

namespace tpf
{
    public class AppChannelStatus : Status
    {
        ChannelVerAddrCfg verAddrCfg = new ChannelVerAddrCfg();
        public override int GetStatusID()
        {
            return (int)VesionUpdateStatus.AppChannelCheck;
        }
        public override void OnInit(StatusMgr mgr)
        {
			base.OnInit(mgr);
        }
        public override void OnEnter(Status oldStatus)
        {
            LogUtils.Log("enter app channel status");
            verAddrCfg.item.Clear();

            try
            {
                Uri requestUrl = new Uri("http://10.0.0.252:8080/channel_cfg/channel_ver_addr.json");
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestUrl);
                request.Timeout = 2000;
                HttpWebResponse respone = (HttpWebResponse)request.GetResponse();

                long blockSize = respone.ContentLength;
                byte[] blockBuff = new byte[blockSize];
                var respStream = respone.GetResponseStream();
                int rec = respStream.Read(blockBuff, 0, (int)blockSize);
                respone.Close();
                request.Abort();

                string str = System.Text.Encoding.Default.GetString(blockBuff);
                verAddrCfg = JsonUtility.FromJson<ChannelVerAddrCfg>(str);

                int channelId = VersionUpdate.GetIns().GetChannelID();
                for (int i = 0; i < verAddrCfg.item.Count; ++i)
                {
                    if (verAddrCfg.item[i].id == channelId)
                    {
                        DownloadMgr.GetInst().SetBaseURL(verAddrCfg.item[i].addr);
                        m_mgr.ChangeNextStatus();
                        return;
                    }
                }
                LogUtils.LogError("can not find channel ver addr!");
            }
            catch (System.Exception ex)
            {
                LogUtils.LogError(ex.ToString());
            }
        }
        public override void OnExit(Status newStatus)
        {
           
        }
        public override void OnUnit()
        {
            base.OnUnit();
        }
    }

}