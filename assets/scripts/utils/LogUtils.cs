using UnityEngine;
using System;
using System.IO;
using System.Text;

namespace tpf
{
    public class LogUtils
    {
        public enum LogLevel
        {
            Level_Error = 1,
            Level_Warning = 2,
            Level_Verbose = 4,
        }

        public static bool enable = true;
        public static int logLevel = 7;
        public static bool log2Unity = true;
        public static bool log2File = false;
       
        static int writeCount = 0;
        static long lastTime;
        static string logDir;
        static FileStream fileStream;
        static StringBuilder strBuilder = new StringBuilder();

        public static void Init(string dir)
        {
            Application.logMessageReceived += OnLogPrinted;
            logDir = dir + "log/";
            if (!Directory.Exists(logDir))
            {
                Directory.CreateDirectory(logDir);
            }

            if(log2File)
            {
                OpenFile();
            }
        }

        public static void Uninit()
        {
            CloseFile();
        }
        static void OpenFile()
        {
            string fileName = logDir + "log_" + System.DateTime.Now.ToString("yyyyMMddHHmmss") + ".log";
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }
            fileStream = File.Create(fileName);
        }
        static void CloseFile()
        {
            if (fileStream == null) return;
            WriteLog();
            fileStream.Close();
        }
        public static void Flush()
        {
            if (fileStream == null) return;
            try
            {
                fileStream.Flush();
            }
            catch
            {

            }
        }
        public static void Update()
        {
            long now = DateTime.Now.Ticks;
            if (now - lastTime > 100000000)
            {
                WriteLog();
            }
        }
        public static void LogWithStack(string message)
        {
            if (!enable) return;

            {
                string stackInfo = new System.Diagnostics.StackTrace().ToString();
                string msg = message + stackInfo;
                if (log2Unity)
                    Debug.Log(msg);

                if (log2File)
                    LogFile(msg);

            }
        }
        public static void Log(string message)
        {
            if (!enable) return;

            if( (logLevel & (int)LogLevel.Level_Verbose) != 0)
            {
                if (log2Unity)
                    Debug.Log(message);

                if(log2File)
                    LogFile(message);

            }
        }
        public static void LogWarning(string message)
        {
            if (!enable) return;

            if ((logLevel & (int)LogLevel.Level_Warning) != 0)
            {
                if (log2Unity)
                    Debug.LogWarning(message);

                if (log2File)
                    LogFile(message);

            }
        }
        public static void LogError(string message)
        {
            if (!enable) return;

            if ((logLevel & (int)LogLevel.Level_Error) != 0)
            {
                if (log2Unity)
                    Debug.LogError(message);

                if (log2File)
                    LogFile(message);

            }
        }

        #region private
        static void OnLogPrinted(string strLog, string strStack, LogType type)
        {
            if (type == LogType.Exception || type == LogType.Error)
            {
                if (type == LogType.Exception)
                {
                    LogUtils.LogError("Exception:", strLog, "\n", strStack, "\n--");
                }
                LogUtils.Flush();
            }
        }
        static void WriteLog()
        {
            if (fileStream == null) return;
            try
            {
                if (writeCount > 0)
                {
                    fileStream.Flush();
                    writeCount = 0;
                    lastTime = DateTime.Now.Ticks;
                }
            }
            catch
            {

            }
        }
        static void outInt(StringBuilder outstr, int val, int len)
        {
            int n = 1;
            for (int i = 1; i < len; i++)
            {
                n *= 10;
            }
            for (int i = 0; i < len; i++)
            {
                if (val == 0)
                {
                    outstr.Append('0');
                }
                else if (val >= n)
                {
                    int a = (val / n) % 10;
                    outstr.Append((char)(a + 0x30));
                }
                else
                {
                    outstr.Append('0');
                }
                val %= n;
                n /= 10;
            }
        }

        static void AddText(string value)
        {
            if (fileStream != null && !string.IsNullOrEmpty(value))
            {
                try
                {
                    byte[] info = System.Text.Encoding.UTF8.GetBytes(value);
                    fileStream.Write(info, 0, info.Length);
                    writeCount += info.Length;

                    if (writeCount > 4096)
                    {
                        fileStream.Flush();
                        writeCount = 0;
                        lastTime = DateTime.Now.Ticks;
                    }
                }
                catch
                {

                }
            }
        }

        static void LogFile(string strmsg)
        {
            if (strmsg == null ||
                fileStream == null)
            {
                return;
            }

            DateTime now = DateTime.Now;
            strBuilder.Length = 0;
            outInt(strBuilder, now.Hour, 2);
            strBuilder.Append(':');
            outInt(strBuilder, now.Minute, 2);
            strBuilder.Append(':');
            outInt(strBuilder, now.Second, 2);
            strBuilder.Append(':');
            outInt(strBuilder, now.Millisecond, 3);
            strBuilder.Append(':');
            strBuilder.Append(strmsg);
            strBuilder.Append('\n');
            AddText(strBuilder.ToString());
        }

        public static void Log(string str1, string str2)
        {
            Log(StringCombine.StrCat(str1, str2));
        }

        public static void Log(string str1, string str2, string str3)
        {
            Log(StringCombine.StrCat(str1, str2, str3));
        }

        public static void Log(string str1, string str2, string str3, string str4)
        {
            Log(StringCombine.StrCat(str1, str2, str3, str4));
        }

        public static void Log(string str1, string str2, string str3, string str4, string str5)
        {
            Log(StringCombine.StrCat(str1, str2, str3, str4, str5));
        }

        public static void Log(string str1, string str2, string str3, string str4, string str5, string str6)
        {
            Log(StringCombine.StrCat(str1, str2, str3, str4, str5, str6));
        }
        public static void Log(string str1, string str2, string str3, string str4, string str5, string str6, string str7)
        {
            Log(StringCombine.StrCat(str1, str2, str3, str4, str5, str6, str7));
        }
        public static void Log(string str1, string str2, string str3, string str4, string str5, string str6, string str7, string str8)
        {
            Log(StringCombine.StrCat(str1, str2, str3, str4, str5, str6, str7, str8));
        }
        public static void LogWarningWarning(string str1, string str2)
        {
            LogWarning(StringCombine.StrCat(str1, str2));
        }
        public static void LogWarningWarning(string str1, string str2, string str3)
        {
            LogWarning(StringCombine.StrCat(str1, str2, str3));
        }

        public static void LogWarningWarning(string str1, string str2, string str3, string str4)
        {
            LogWarning(StringCombine.StrCat(str1, str2, str3, str4));
        }

        public static void LogWarningWarning(string str1, string str2, string str3, string str4, string str5)
        {
            LogWarning(StringCombine.StrCat(str1, str2, str3, str4, str5));
        }
        public static void LogWarningWarning(string str1, string str2, string str3, string str4, string str5, string str6)
        {
            LogWarning(StringCombine.StrCat(str1, str2, str3, str4, str5, str6));
        }

        public static void LogWarningWarning(string str1, string str2, string str3, string str4, string str5, string str6, string str7)
        {
            LogWarning(StringCombine.StrCat(str1, str2, str3, str4, str5, str6, str7));
        }

        public static void LogWarningWarning(string str1, string str2, string str3, string str4, string str5, string str6, string str7, string str8)
        {
            LogWarning(StringCombine.StrCat(str1, str2, str3, str4, str5, str6, str7, str8));
        }

        public static void LogError(string str1, string str2)
        {
            LogError(StringCombine.StrCat(str1, str2));
        }
        public static void LogError(string str1, string str2, string str3)
        {
            LogError(StringCombine.StrCat(str1, str2, str3));
        }

        public static void LogError(string str1, string str2, string str3, string str4)
        {
            LogError(StringCombine.StrCat(str1, str2, str3, str4));
        }

        public static void LogError(string str1, string str2, string str3, string str4, string str5)
        {
            LogError(StringCombine.StrCat(str1, str2, str3, str4, str5));
        }

        public static void LogError(string str1, string str2, string str3, string str4, string str5, string str6)
        {
            LogError(StringCombine.StrCat(str1, str2, str3, str4, str5, str6));
        }

        public static void LogError(string str1, string str2, string str3, string str4, string str5, string str6, string str7)
        {
            LogError(StringCombine.StrCat(str1, str2, str3, str4, str5, str6, str7));
        }
   
        public static void LogError(string str1, string str2, string str3, string str4, string str5, string str6, string str7, string str8)
        {
            LogError(StringCombine.StrCat(str1, str2, str3, str4, str5, str6, str7, str8));
        }

        #endregion

    }

}