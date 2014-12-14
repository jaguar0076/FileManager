using System;
using System.Data.SQLite;
using System.IO;
using System.Reflection;

namespace FileManager
{
    internal static class DbUtils
    {
        #region Variables

        private static SQLiteConnection AudioDbConnection = null;

        private static readonly object MyLock = new object();
        //to store in config file
        private static readonly string DbString = "audio_db.sqlite";

        private static readonly string DbConnection = "Data Source={0};Version=3;";

        //pool/array of command

        #endregion

        #region Manage Connection

        public static SQLiteConnection GetConnection()
        {
            lock (MyLock)
            {
                if (AudioDbConnection == null)
                {
                    Check_Create_DB(Directory.GetCurrentDirectory(), DbString);

                    try
                    {
                        AudioDbConnection = new SQLiteConnection(String.Format(DbConnection, DbString));
                    }
                    catch (Exception e)
                    { Utils.SaveLogFile(MethodBase.GetCurrentMethod(), e); }
                }

                OpenConnection();

                return AudioDbConnection;
            }
        }

        private static void Check_Create_DB(string dbPath, string dbName)
        {
            string FullPath = dbPath + "\\" + dbName;

            if (!File.Exists(FullPath))
            {
                try
                {
                    SQLiteConnection.CreateFile(dbName);
                }
                catch (Exception e)
                { Utils.SaveLogFile(MethodBase.GetCurrentMethod(), e); }
            }
        }

        private static void OpenConnection()
        {
            if (AudioDbConnection != null && AudioDbConnection.State != System.Data.ConnectionState.Open)
            {
                AudioDbConnection.Open();
            }
        }

        public static void CloseConnection()
        {
            if (AudioDbConnection != null && AudioDbConnection.State != System.Data.ConnectionState.Closed)
            {
                AudioDbConnection.Close();
            }
        }

        #endregion

        #region Execute query

        private static void PrepareStatement(ref SQLiteCommand command, params object[] args)
        {
            foreach (var arg in args)
            {
                SQLiteParameter Field = command.CreateParameter();

                command.Parameters.Add(Field);

                Field.Value = arg;
            }
        }

        internal static void ExecuteNonQuery(string sql)
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

        internal static void ExecuteNonQuery(string sql, params object[] args)
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