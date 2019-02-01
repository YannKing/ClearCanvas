using ClearCanvas.Common;
using NewLife.Log;
using System;
using System.Data;
using System.IO;
using LogLevel = ClearCanvas.Common.LogLevel;


namespace ClearCanvas.ImageViewer.StudyManagement.Core.Storage
{
    public static class MySqlDatabaseHelper<TContext>
    {
        public static void CreateDatabase(string fileName)
        {
            //不需要,防止出错保留。
        }

        public static IDbConnection CreateConnection()
        {
            try
            {

                var connection = CreateConnectionInternal();
                Platform.Log(LogLevel.Info, "加载MySql成功。");
                //XTrace.WriteLine($"CreateConnection=>加载MySql成功。");//Yann 日志输出太频繁 注掉了
                return connection;
            }
            catch (Exception e)
            {
                Platform.Log(LogLevel.Error, "加载MySql错误：" + e.Message + "\n\r" + e.StackTrace);
                XTrace.WriteLine($"加载MySql错误!!!!!!!!!!!!!!!!!!");
                XTrace.WriteException(e);
            }
            return null;

        }

        public static string GetDatabaseFilePath(string fileName)
        {
            //不需要,防止出错保留。
            return Path.Combine(GetDatabaseDirectory(), fileName);
        }

        public static string GetDatabaseDirectory()
        {
            //不需要,防止出错保留。
            return Platform.ApplicationDataDirectory;
        }

        private static IDbConnection CreateConnectionInternal()
        {
            读写配置文件.SWConfig CFW = new 读写配置文件.SWConfig(AppDomain.CurrentDomain.BaseDirectory + "\\sysset.xml");
            //以后需要将字符串放到配置文件中，暂时写在程序里测试。
            var connectString = "User Id=" + CFW.GetConfigValue("数据库用户名").ToString() + ";Password=" + CFW.GetConfigValue("数据库密码").ToString() + ";Host=" + CFW.GetConfigValue("数据连接地址").ToString() + ";Database=" + CFW.GetConfigValue("数据库名称").ToString() + ";Persist Security Info=True";
            //var connectString = "User Id=root;Password=admin1,;Host=127.0.0.1;Database=dicom_store;Persist Security Info=True";
            //替换为MySql
            var connection = new Devart.Data.MySql.MySqlConnection(connectString);
            connection.Open();
            //XTrace.WriteLine($"Connenction: {connection.ToJson()}");
            return connection;
        }
    }
}
