using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace FileManager
{
    internal static class Utils
    {
        #region Variables
        //Should be stored in a config file
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
            { Utils.SaveLogFile(MethodBase.GetCurrentMethod(), new Exception("Error in If statements")); }
        }

        internal static void CheckSetPropertyValue(this object o, string propertyName, string val)
        {
            if (HasProperty(o, propertyName))
            {
                SetPropertyValue(o, propertyName, val);
            }
            else
            { Utils.SaveLogFile(MethodBase.GetCurrentMethod(), new Exception("Error in If statements")); }
        }

        internal static void CheckSetPropertyValue(this object o, string propertyName, uint val)
        {
            if (HasProperty(o, propertyName))
            {
                SetPropertyValue(o, propertyName, val);
            }
            else
            { Utils.SaveLogFile(MethodBase.GetCurrentMethod(), new Exception("Error in If statements")); }
        }

        internal static string CheckGetPropertyValue(this object o, string propertyName)
        {
            string val = "";

            if (HasProperty(o, propertyName))
            {
                val = GetPropertyValue(o, propertyName);
            }
            else
            { Utils.SaveLogFile(MethodBase.GetCurrentMethod(), new Exception("Error in If statements")); }

            return val;
        }

        internal static void CheckSetMethodValue(this object o, string methodName, string val)
        {
            if (HasMethod(o, methodName))
            {
                SetMethodValue(o, methodName, val);
            }
            else
            { Utils.SaveLogFile(MethodBase.GetCurrentMethod(), new Exception("Error in If statements")); }
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

        #region Logging

        internal static void SaveLogFile(object method, Exception exception)
        {
            string location = Directory.GetCurrentDirectory() + "\\" + "error_log.txt";

            try
            {
                //Opens a new file stream which allows asynchronous reading and writing
                using (StreamWriter sw = new StreamWriter(new FileStream(location, FileMode.Append, FileAccess.Write, FileShare.ReadWrite)))
                {
                    //Writes the method name with the exception and writes the exception underneath
                    sw.WriteLine(String.Format("{0} ({1}) - Method: {2}", DateTime.Now.ToShortDateString(), DateTime.Now.ToShortTimeString(), method.ToString()));
                    sw.WriteLine(exception.ToString());
                    sw.WriteLine("");
                }
            }
            catch (IOException)
            {
                if (!File.Exists(location))
                {
                    File.Create(location);

                    SaveLogFile(method, exception);
                }
            }
            //Utils.SaveLogFile(MethodBase.GetCurrentMethod(), new Exception("MusicBrainzTrackId: " + filetag.Tag.AmazonId));
        }

        #endregion

        #region MD5Hash

        internal static string ComputeMD5Hash(string FilePath)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(FilePath))
                {
                    return BitConverter.ToString(md5.ComputeHash(stream));
                }
            }
        }

        #endregion
    }
}