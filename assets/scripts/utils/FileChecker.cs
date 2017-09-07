
using System.IO;

namespace tpf
{
    public class FileChecker
    {
        public static string GetFileSignature(string fileName)
        {
            if (!File.Exists(fileName))
                return "";

            FileStream file = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            string signature = MD5Hash.Get(file);
            file.Close();
            return signature;
        }

        public static bool VerifyFileSignature(string fileName,string signature)
        {
            if (!File.Exists(fileName))
                return false;
            FileStream file = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            bool b = MD5Hash.Verify(file, signature);
            file.Close();
            return b;
        }
    }
}