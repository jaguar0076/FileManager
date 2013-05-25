using System;

namespace FileManager
{
    static class Utils
    {
        public static bool HasMethod(this object o, string methodName)
        {
            return o.GetType().GetProperty(methodName) != null;
        }

        public static void SetMethodValue(this object o, string methodName, string val)
        {
            try
            {
                o.GetType().GetProperty(methodName).SetValue(o, val, null);
            }
            catch (Exception ex)
            { /*throw exception here*/ }
        }

        public static string GetMethodValue(this object o, string methodName)
        {
            string val = "";

            try
            {
                val = (string)o.GetType().GetProperty(methodName).GetValue(o, null);
            }
            catch (Exception ex)
            { /*throw exception here*/ }

            return val;
        }

        public static void CheckSetMethodValue(this object o, string methodName, string val)
        {
            if (HasMethod(o, methodName))
            {
                SetMethodValue(o, methodName, val);
            }
            else
            { /*throw exception here*/ }
        }

        public static string CheckGetMethodValue(this object o, string methodName)
        {
            string val = "";

            if (HasMethod(o, methodName))
            {
                val = GetMethodValue(o, methodName);
            }
            else
            { /*throw exception here*/ }

            return val;
        }
    }
}