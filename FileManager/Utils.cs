using System;
using System.Reflection;

namespace FileManager
{
    static class Utils
    {
        public static bool HasProperty(this object o, string methodName)
        {
            return o.GetType().GetProperty(methodName) != null;
        }

        public static void SetPropertyValue(this object o, string methodName, string val)
        {
            try
            {
                o.GetType().GetProperty(methodName).SetValue(o, val, null);
            }
            catch (Exception ex)
            { /*throw exception here*/ }
        }

        public static void SetPropertyValue(this object o, string methodName, bool val)
        {
            try
            {
                o.GetType().GetProperty(methodName).SetValue(o, val, null);
            }
            catch (Exception ex)
            { /*throw exception here*/ }
        }

        public static string GetPropertyValue(this object o, string methodName)
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

        public static void CheckSetPropertyValue(this object o, string methodName, string val)
        {
            if (HasProperty(o, methodName))
            {
                SetPropertyValue(o, methodName, val);
            }
            else
            { /*throw exception here*/ }
        }

        public static void CheckSetPropertyValue(this object o, string methodName, bool val)
        {
            if (HasProperty(o, methodName))
            {
                SetPropertyValue(o, methodName, val);
            }
            else
            { /*throw exception here*/ }
        }

        public static string CheckGetPropertyValue(this object o, string methodName)
        {
            string val = "";

            if (HasProperty(o, methodName))
            {
                val = GetPropertyValue(o, methodName);
            }
            else
            { /*throw exception here*/ }

            return val;
        }

        public static bool HasMethod(this object o, string methodName)
        {
            return o.GetType().GetMethod(methodName) != null;
        }

        public static void SetMethodValue(this object o, string methodName, string val)
        {
            try
            {
                o.GetType().GetMethod(methodName).Invoke(o, new object[] { val });
            }
            catch (Exception ex)
            { /*throw exception here*/ }
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
    }
}