using System;
using System.Data.SQLite;
using System.IO;
using System.Reflection;

namespace FileManager
{
    internal static class DbUtils
    {
        #region Variables

        private static SQLiteConnection audio_dbConnection = null;

        private static readonly object myLock = new object();
        //to store in config file
        private static readonly string dbString = "audio_db.sqlite";

        private static readonly string dbConnection = "Data Source={0};Version=3;";

        //pool/array of command

        #endregion

        #region Manage Connection

        public static SQLiteConnection GetConnection()
        {
            lock (myLock)
            {
                if (audio_dbConnection == null)
                {
                    Check_Create_DB(Directory.GetCurrentDirectory(), dbString);

                    try
                    {
                        audio_dbConnection = new SQLiteConnection(String.Format(dbConnection, dbString));
                    }
                    catch (Exception e)
                    { Utils.SaveLogFile(MethodBase.GetCurrentMethod(), e); }
                }

                openConnection();

                return audio_dbConnection;
            }
        }

        private static void Check_Create_DB(string DbPath, string DbName)
        {
            string FullPath = DbPath + "\\" + DbName;

            if (!File.Exists(FullPath))
            {
                try
                {
                    SQLiteConnection.CreateFile(DbName);
                }
                catch (Exception e)
                { Utils.SaveLogFile(MethodBase.GetCurrentMethod(), e); }
            }
        }

        private static void openConnection()
        {
            if (audio_dbConnection != null && audio_dbConnection.State != System.Data.ConnectionState.Open)
            {
                audio_dbConnection.Open();
            }
        }

        public static void CloseConnection()
        {
            if (audio_dbConnection != null && audio_dbConnection.State != System.Data.ConnectionState.Closed)
            {
                audio_dbConnection.Close();
            }
        }

        #endregion

        #region Execute query

        private static void PrepareStatement(ref SQLiteCommand Command, params object[] args)
        {
            foreach (var arg in args)
            {
                SQLiteParameter Field = Command.CreateParameter();

                Command.Parameters.Add(Field);

                Field.Value = arg;
            }
        }

        public static void ExecuteNonQuery(string sql)
        {
            SQLiteCommand command = new SQLiteCommand(sql, GetConnection());

            try
            {
                command.ExecuteNonQuery();

                //CloseConnection();
            }
            catch (Exception e)
            { Utils.SaveLogFile(MethodBase.GetCurrentMethod(), e); }
        }

        public static void ExecuteNonQuery(string sql, params object[] args)
        {
            SQLiteCommand command = new SQLiteCommand(sql, GetConnection());

            PrepareStatement(ref command, args);

            try
            {
                command.ExecuteNonQuery();

                //CloseConnection();
            }
            catch (Exception e)
            { Utils.SaveLogFile(MethodBase.GetCurrentMethod(), e); }
        }

        #endregion
    }
}