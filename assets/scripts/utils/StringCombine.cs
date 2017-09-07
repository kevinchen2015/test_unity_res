using System.Text;

namespace tpf
{
    public class StringCombine
    {
        static StringBuilder sb = null;
        static int strSeq = 0;
        static public int Begin()
        {
            if (sb == null)
                sb = new StringBuilder();
            else
                sb.Length = 0;
            strSeq++;
            return strSeq;
        }
        static public void Append(string str)
        {
            if (sb != null)
                sb.Append(str);
        }
        static public StringBuilder Builder()
        {
            return sb;
        }
        static public string Get(int seq)
        {
            if (seq == strSeq)
                return sb.ToString();
            else
                return "string error!";
        }
        static public string StrFormat(string format, params object[] items)
        {
            Begin();
            sb.AppendFormat(format, items);
            return sb.ToString();
        }
        static public void StrFormatInMem(string format, params object[] items)
        {
            sb.AppendFormat(format, items);
        }
        static public string StrCat(string str1, string str2, string str3, string str4, string str5, string str6, string str7, string str8, string str9, string str10, string str11, string str12)
        {
            Begin();
            StrCatInMem(str1, str2, str3, str4, str5, str6, str7, str8, str9, str10, str11, str12);
            return sb.ToString();
        }
        static public string StrCat(string str1, string str2, string str3, string str4, string str5, string str6, string str7, string str8, string str9, string str10, string str11, string str12, string str13)
        {
            Begin();
            StrCatInMem(str1, str2, str3, str4, str5, str6, str7, str8, str9, str10, str11, str12, str13);
            return sb.ToString();
        }
        static public string StrCat(string str1, string str2, string str3, string str4, string str5, string str6, string str7, string str8, string str9, string str10, string str11)
        {
            Begin();
            StrCatInMem(str1, str2, str3, str4, str5, str6, str7, str8, str9, str10, str11);
            return sb.ToString();
        }
        static public string StrCat(string str1, string str2, string str3, string str4, string str5, string str6, string str7, string str8, string str9, string str10)
        {
            Begin();
            StrCatInMem(str1, str2, str3, str4, str5, str6, str7, str8, str9, str10);
            return sb.ToString();
        }
        static public string StrCat(string str1, string str2, string str3, string str4, string str5, string str6, string str7, string str8, string str9)
        {
            Begin();
            StrCatInMem(str1, str2, str3, str4, str5, str6, str7, str8, str9);
            return sb.ToString();
        }
        static public string StrCat(string str1, string str2, string str3, string str4, string str5, string str6, string str7, string str8)
        {
            Begin();
            StrCatInMem(str1, str2, str3, str4, str5, str6, str7, str8);
            return sb.ToString();
        }
        static public string StrCat(string str1, string str2, string str3, string str4, string str5, string str6, string str7)
        {
            Begin();
            StrCatInMem(str1, str2, str3, str4, str5, str6, str7);
            return sb.ToString();
        }
        static public string StrCat(string str1, string str2, string str3, string str4, string str5, string str6)
        {
            Begin();
            StrCatInMem(str1, str2, str3, str4, str5, str6);
            return sb.ToString();
        }
        static public string StrCat(string str1, string str2, string str3, string str4, string str5)
        {
            Begin();
            StrCatInMem(str1, str2, str3, str4, str5);
            return sb.ToString();
        }
        static public string StrCat(string str1, string str2, string str3, string str4)
        {
            Begin();
            StrCatInMem(str1, str2, str3, str4);
            return sb.ToString();
        }
        static public string StrCat(string str1, string str2, string str3)
        {
            Begin();
            StrCatInMem(str1, str2, str3);
            return sb.ToString();
        }
        static public string StrCat(string str1, string str2)
        {
            Begin();
            StrCatInMem(str1, str2);
            return sb.ToString();
        }
        static public void StrCatInMem(string str1, string str2)
        {
            sb.Append(str1);
            sb.Append(str2);
        }

        static public void StrCatInMem(string str1, string str2, string str3)
        {
            sb.Append(str1);
            sb.Append(str2);
            sb.Append(str3);
        }

        static public void StrCatInMem(string str1, string str2, string str3, string str4)
        {
            sb.Append(str1);
            sb.Append(str2);
            sb.Append(str3);
            sb.Append(str4);
        }

        static public void StrCatInMem(string str1, string str2, string str3, string str4, string str5)
        {
            sb.Append(str1);
            sb.Append(str2);
            sb.Append(str3);
            sb.Append(str4);
            sb.Append(str5);
        }
        static public void StrCatInMem(string str1, string str2, string str3, string str4, string str5, string str6)
        {
            sb.Append(str1);
            sb.Append(str2);
            sb.Append(str3);
            sb.Append(str4);
            sb.Append(str5);
            sb.Append(str6);
        }
        static public void StrCatInMem(string str1, string str2, string str3, string str4, string str5, string str6, string str7)
        {
            sb.Append(str1);
            sb.Append(str2);
            sb.Append(str3);
            sb.Append(str4);
            sb.Append(str5);
            sb.Append(str6);
            sb.Append(str7);
        }
        static public void StrCatInMem(string str1, string str2, string str3, string str4, string str5, string str6, string str7, string str8)
        {
            sb.Append(str1);
            sb.Append(str2);
            sb.Append(str3);
            sb.Append(str4);
            sb.Append(str5);
            sb.Append(str6);
            sb.Append(str7);
            sb.Append(str8);

        }
        static public void StrCatInMem(string str1, string str2, string str3, string str4, string str5, string str6, string str7, string str8, string str9)
        {
            sb.Append(str1);
            sb.Append(str2);
            sb.Append(str3);
            sb.Append(str4);
            sb.Append(str5);
            sb.Append(str6);
            sb.Append(str7);
            sb.Append(str8);
            sb.Append(str9);
        }
        static public void StrCatInMem(string str1, string str2, string str3, string str4, string str5, string str6, string str7, string str8, string str9, string str10)
        {
            sb.Append(str1);
            sb.Append(str2);
            sb.Append(str3);
            sb.Append(str4);
            sb.Append(str5);
            sb.Append(str6);
            sb.Append(str7);
            sb.Append(str8);
            sb.Append(str9);
            sb.Append(str10);
        }
        static public void StrCatInMem(string str1, string str2, string str3, string str4, string str5, string str6, string str7, string str8, string str9, string str10, string str11)
        {
            sb.Append(str1);
            sb.Append(str2);
            sb.Append(str3);
            sb.Append(str4);
            sb.Append(str5);
            sb.Append(str6);
            sb.Append(str7);
            sb.Append(str8);
            sb.Append(str9);
            sb.Append(str10);
            sb.Append(str11);
        }
        static public void StrCatInMem(string str1, string str2, string str3, string str4, string str5, string str6, string str7, string str8, string str9, string str10, string str11, string str12)
        {
            sb.Append(str1);
            sb.Append(str2);
            sb.Append(str3);
            sb.Append(str4);
            sb.Append(str5);
            sb.Append(str6);
            sb.Append(str7);
            sb.Append(str8);
            sb.Append(str9);
            sb.Append(str10);
            sb.Append(str11);
            sb.Append(str12);
        }
        static public void StrCatInMem(string str1, string str2, string str3, string str4, string str5, string str6, string str7, string str8, string str9, string str10, string str11, string str12, string str13)
        {
            sb.Append(str1);
            sb.Append(str2);
            sb.Append(str3);
            sb.Append(str4);
            sb.Append(str5);
            sb.Append(str6);
            sb.Append(str7);
            sb.Append(str8);
            sb.Append(str9);
            sb.Append(str10);
            sb.Append(str11);
            sb.Append(str12);
            sb.Append(str13);
        }
    }
}