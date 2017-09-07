
using System;
using System.Collections.Generic;

namespace tpf
{
    public class VersionResDef
    {
        //1 = big version
        //2 = features version
        //3 = resource version
        //4 = build version ,or svn tag
        public static string version = "1.2.3.1234";  //该版本号写死在程序中
    }

    [Serializable]
    public class VersionRes
    {
        public string version = "1.2.4.0001";
        public int channelId = 1;
        public string url = "";
        public string apk = "";
        public string apkSignature = "";
        public string ios = "";
        public string miniVersion = "0.1.0.2222";
        public  List<VersionResFile> versionResFile = new List<VersionResFile>();
    }

    public enum VersionResType
    {
        PreDownload = 0, //必须预先下载
        LazyDownload,    //使用时下载
        IdleDownload,    //闲时下载（todo）
    }

    [Serializable]
    public class VersionResFile
    {
        public string name;
        public string signature;
        public string compress;
        public VersionResType resType = VersionResType.PreDownload;
    }

    [Serializable]
    public class ChannelVerAddrCfg
    {
        public List<ChannelVerAddrItem> item = new List<ChannelVerAddrItem>();
    }

    [Serializable]
    public class ChannelVerAddrItem
    {
        public int id;
        public string name;
        public string addr;
    }

}