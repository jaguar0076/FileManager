using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace FileManager
{
    static class Utils
    {
        #region Variables
        //Will be stored in a config file
        private static string[] ExcludedString = { "\\", "/", "?", ":", "*", "\"", ">", "<", "|" };

        #endregion

        #region Check property & method name

        private static bool HasProperty(this object o, string propertyName)
        {
            return o.GetType().GetProperty(propertyName) != null;
        }

        private static bool HasMethod(this object o, string methodName)
        {
            return o.GetType().GetMethod(methodName) != null;
        }

        #endregion

        #region Set property & method value

        private static void SetPropertyValue(this object o, string propertyName, bool val)
        {
            o.GetType().GetProperty(propertyName).SetValue(o, val, null);
        }

        private static void SetPropertyValue(this object o, string propertyName, string val)
        {
            o.GetType().GetProperty(propertyName).SetValue(o, val, null);
        }

        private static void SetPropertyValue(this object o, string propertyName, uint val)
        {
            o.GetType().GetProperty(propertyName).SetValue(o, val, null);
        }

        private static void SetMethodValue(this object o, string methodName, string val)
        {
            o.GetType().GetMethod(methodName).Invoke(o, new object[] { val });
        }

        #endregion

        #region Get property value

        private static string GetPropertyValue(this object o, string propertyName)
        {
            string val = "";

            val = (string)o.GetType().GetProperty(propertyName).GetValue(o, null);

            return val;
        }

        #endregion

        #region Check/Set property & method value

        internal static void CheckSetPropertyValue(this object o, string propertyName, bool val)
        {
            if (HasProperty(o, propertyName))
            {
                SetPropertyValue(o, propertyName, val);
            }
            else
            { /*throw exception here*/ }
        }

        internal static void CheckSetPropertyValue(this object o, string propertyName, string val)
        {
            if (HasProperty(o, propertyName))
            {
                SetPropertyValue(o, propertyName, val);
            }
            else
            { /*throw exception here*/ }
        }

        internal static void CheckSetPropertyValue(this object o, string propertyName, uint val)
        {
            if (HasProperty(o, propertyName))
            {
                SetPropertyValue(o, propertyName, val);
            }
            else
            { /*throw exception here*/ }
        }

        internal static string CheckGetPropertyValue(this object o, string propertyName)
        {
            string val = "";

            if (HasProperty(o, propertyName))
            {
                val = GetPropertyValue(o, propertyName);
            }
            else
            { /*throw exception here*/ }

            return val;
        }

        internal static void CheckSetMethodValue(this object o, string methodName, string val)
        {
            if (HasMethod(o, methodName))
            {
                SetMethodValue(o, methodName, val);
            }
            else
            { /*throw exception here*/ }
        }

        #endregion

        #region String Formatting utilities

        internal static string Clean_String(string txt)
        {
            StringBuilder sb = new StringBuilder(txt);

            return sb.Replace("\0", string.Empty).ToString();
        }

        internal static string Name_Cleanup(string FileName)
        {//It's frickin faster than Regex, tested with LINQPad 10000000 times, Regex about 11 secondes, between 0.4 and 0.8 secondes with this

            StringBuilder sb = new StringBuilder(FileName);

            foreach (var ExString in ExcludedString)
            {
                sb.Replace(ExString, string.Empty);
            }

            //return Regex.Replace(FileName, @"[\/?:*""><|]+", "-", RegexOptions.Compiled);

            return sb.ToString();
        }

        internal static string FormatStringBasedOnRegex(string FileName, string StringMatch, char StringFormat)
        {
            return Regex.Match(FileName, StringMatch).Value.PadLeft(2, StringFormat);
        }

        #endregion

        #region MD5Hash

        private static byte[] ComputeMD5Hash(string FilePath)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(FilePath))
                {
                    return md5.ComputeHash(stream);
                }
            }
        }

        #endregion
    }
}