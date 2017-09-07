using UnityEngine;
using System.IO;

namespace tpf
{
    public class SerializeHelper
    {
        public static bool SaveToFile(object obj,string fileName,bool format = false)
        {
            string strJson = JsonUtility.ToJson(obj, format);
            if(strJson == null)
            {
                return false;
            }
            if (File.Exists(fileName)) File.Delete(fileName);
            FileStream file = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Write);
            StreamWriter sw = new StreamWriter(file);
            sw.Write(strJson);
            sw.Flush();
            file.Flush();
            file.Close();
            return true;
        }
        public static T LoadFromFile<T>(string fileName)
        {
            if (!File.Exists(fileName)) return default(T);

            FileStream file = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            StreamReader sr = new StreamReader(file);
            string strJson = sr.ReadToEnd();
            file.Close();
            return JsonUtility.FromJson<T>(strJson);
        }
    }
}