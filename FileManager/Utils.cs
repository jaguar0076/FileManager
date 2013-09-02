namespace FileManager
{
    static class Utils
    {
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
    }
}