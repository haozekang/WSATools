using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace WSATools.ExtendMethod
{
    public static class StringExtendMethod
    {
        [Pure]
        public static string StringTrim(this string str)
        {
            if (IsBlank(str))
            {
                return default;
            }
            return str.Trim();
        }

        [Pure]
        public static Guid ToGuid(this string str)
        {
            if (IsBlank(str))
            {
                return default;
            }
            Guid guid;
            if (Guid.TryParse(str, out guid))
            {
                return guid;
            }
            return default;
        }

        [Pure]
        public static int ToInteger(this string str)
        {
            int _int = 0;
            if (int.TryParse(str, out _int))
            {
                return _int;
            }
            return default;
        }

        [Pure]
        public static float ToFloat(this string str)
        {
            float _float = 0;
            if (float.TryParse(str, out _float))
            {
                return _float;
            }
            return default;
        }

        [Pure]
        public static double ToDouble(this string str)
        {
            double _double = 0;
            if (double.TryParse(str, out _double))
            {
                return _double;
            }
            return default;
        }

        [Pure]
        public static bool IsBlank(this string str)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return true;
            }
            if (string.IsNullOrEmpty(str))
            {
                return true;
            }
            return false;
        }

        [Pure]
        public static void IsBlank(this string str, Action action)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return;
            }
            if (string.IsNullOrEmpty(str))
            {
                return;
            }
            action?.Invoke();
        }

        [Pure]
        public static void IsBlank(this string str, Action<char> action)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return;
            }
            if (string.IsNullOrEmpty(str))
            {
                return;
            }
            foreach (char o in str)
            {
                action?.Invoke(o);
            }
        }

        [Pure]
        public static bool IsNotBlank(this string str)
        {
            return !IsBlank(str);
        }

        public static void IsNotBlank(this string str, Action action)
        {
            if (IsBlank(str))
            {
                return;
            }
            action?.Invoke();
        }

        public static void IsNotBlank(this string str, Action<char> action)
        {
            if (IsBlank(str))
            {
                return;
            }
            foreach (char o in str)
            {
                action?.Invoke(o);
            }
        }

        /// <summary>
        /// 返回指定路径地址的文件名（包含扩展名）
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        [Pure]
        public static string GetFilename(this string str)
        {
            return Path.GetFileName(str);
        }

        /// <summary>
        /// 返回指定路径地址的文件名（不含扩展名）
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        [Pure]
        public static string GetFilenameWithoutExtension(this string str)
        {
            return Path.GetFileNameWithoutExtension(str);
        }

        /// <summary>
        /// 返回指定路径地址的扩展名
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        [Pure]
        public static string GetExtension(this string str)
        {
            return Path.GetExtension(str);
        }

        /// <summary>
        /// 返回指定路径地址的目录信息
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        [Pure]
        public static string GetDirectoryName(this string str)
        {
            return Path.GetDirectoryName(str);
        }

        /// <summary>
        /// 正则表达式替换字符串
        /// </summary>
        /// <param name="str">字符串内容</param>
        /// <param name="pattern">替换字符</param>
        /// <param name="replaceStr">替换值</param>
        /// <returns></returns>
        [Pure]
        public static string RegexReplace(this string str, string pattern, string replaceStr)
        {
            return Regex.Replace(str, pattern, replaceStr);
        }

        /// <summary>
        /// 自定义正则匹配验证规则
        /// </summary>
        /// <param name="str"></param>
        /// <param name="pattern"></param>
        /// <returns></returns>
        [Pure]
        public static bool RegexIs(this string str, string pattern)
        {
            return Regex.IsMatch(str, @pattern);
        }

        /// <summary>
        /// 获取所有的，根据左右字符串获取中间字符串
        /// </summary>
        /// <param name="strString"></param>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        [Pure]
        public static String[] getAllSubStringByLeftStringAndRightStrng(this string strString, String left, String right)
        {
            List<string> result = new List<string>();
            int leftIndex = -1;
            int rightIndex = -1;
            int i = 0, leftCount = 0, rightCount = 0, maxCount = 0;
            leftCount = Regex.Matches(strString, left).Count;
            rightCount = Regex.Matches(strString, right).Count;
            maxCount = leftCount > rightCount ? leftCount : rightCount;
            for (i = 0; i < maxCount; i++)
            {
                if (strString.Length < 2)
                {
                    break;
                }
                leftIndex = strString.IndexOf(left);
                // 如果左字符串为最后一个
                if (leftIndex + left.Length == strString.Length)
                {
                    break;
                }
                else
                {
                    // 去除左字符串，然后查找右字符串
                    rightIndex = strString.Substring(leftIndex + left.Length).IndexOf(right);
                }
                if (leftIndex < 0 || rightIndex < 0)
                {
                    break;
                }
                rightIndex = rightIndex + leftIndex + left.Length;
                result.Add(strString.Substring(leftIndex + left.Length, rightIndex - (leftIndex + left.Length)));
                if (strString.Length > rightIndex + right.Length)
                {
                    strString = strString.Substring(rightIndex + right.Length);
                }
                else
                {
                    break;
                }
            }
            return result.ToArray();
        }

        /// <summary>
        /// 替换字符串中所有出现过的子字符串
        /// </summary>
        /// <param name="oldString"></param>
        /// <param name="newString"></param>
        /// <returns></returns>
        [Pure]
        public static string ReplaceAll(this string str, List<String> oldString, List<String> newString)
        {
            if (oldString.Count > 0 && newString.Count > 0 && oldString.Count == newString.Count)
            {
                for (int i = 0, count = oldString.Count; i < count; i++)
                {
                    str = str.Replace(oldString[i], newString[i]);
                }
            }
            return str;
        }

        /// <summary>
        /// 替换字符串中所有出现过的子字符串
        /// </summary>
        /// <param name="oldString"></param>
        /// <param name="newString"></param>
        /// <returns></returns>
        [Pure]
        public static string ReplaceAll(this string str, String[] oldString, String[] newString)
        {
            if (oldString.Length > 0 && newString.Length > 0 && oldString.Length == newString.Length)
            {
                for (int i = 0, count = oldString.Length; i < count; i++)
                {
                    str = str.Replace(oldString[i], newString[i]);
                }
            }
            return str;
        }

        /// <summary>
        /// 计算MD5
        /// </summary>
        /// <returns></returns>
        [Pure]
        public static string MD5Encrypt32(this string str)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(str);
            bytes = md5.ComputeHash(bytes);
            md5.Clear();

            string ret = "";
            for (int i = 0; i < bytes.Length; i++)
            {
                ret += Convert.ToString(bytes[i], 16).PadLeft(2, '0');
            }
            return ret.PadLeft(32, '0');
        }

        /// <summary>
        /// 返回所有子串下标
        /// </summary>
        /// <returns></returns>
        [Pure]
        public static int[] IndexOfAll(this string str, string sub_str)
        {
            if (IsBlank(str) || IsBlank(sub_str))
            {
                return null;
            }
            List<int> index_list = new List<int>();
            string s = str;
            int index = 0;
            int dis_len = 0;
            while ((index = s.IndexOf(sub_str)) != -1)
            {
                index_list.Add(index + dis_len);
                if (index + sub_str.Length == s.Length)
                {
                    break;
                }
                s = s.Substring(index + sub_str.Length);
                dis_len = str.Length - s.Length;
            }
            return index_list.ToArray();
        }

        /// <summary>
        /// 判断文件是否存在
        /// </summary>
        /// <returns></returns>
        [Pure]
        public static bool AsFilePathAndExists(this string str)
        {
            if (str.IsBlank())
            {
                return false;
            }
            return File.Exists(str);
        }

        /// <summary>
        /// 判断文件夹是否存在
        /// </summary>
        /// <returns></returns>
        [Pure]
        public static bool AsDirectoryPathAndExists(this string str)
        {
            if (str.IsBlank())
            {
                return false;
            }
            return Directory.Exists(str);
        }
    }
}
