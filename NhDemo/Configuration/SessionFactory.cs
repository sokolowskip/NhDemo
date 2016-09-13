using System;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using NhDemo.Entities;
using NHibernate;
using NHibernate.Tool.hbm2ddl;

namespace NhDemo.Configuration
{
    public class SessionFactory
    {
        private static ISessionFactory _factory;

        public static ISession OpenSession()
        {
            var session =  Factory.OpenSession();
            session.FlushMode = FlushMode.Never;
            return session;
        }

        private  static ISessionFactory Factory
        {
            get { return _factory ?? (_factory = InitializeSessionFactory()); }
        }

        private static ISessionFactory InitializeSessionFactory()
        {
            var sessionFactory = Fluently.Configure()
                .Database(
                    MsSqlConfiguration
                        .MsSql2008
                        .ConnectionString($@"Server={ServerName};Initial Catalog=NhDemo;Trusted_Connection=True;")
                        .ShowSql())
                .Mappings(m => m.FluentMappings.AddFromAssemblyOf<Document>())
                .ExposeConfiguration(cfg => new SchemaExport(cfg).Create(true, true))
                .BuildSessionFactory();

            return sessionFactory;
        }

        public static void RestoreDatabase(string databaseName, string backUpFile)
        {
            if (DoesDatabaseExist(databaseName))
            {
                DeleteDatabase(databaseName);
            }

            var connection = new ServerConnection(ServerName);
            Server sqlServer = new Server(connection);
            Restore rstDatabase = new Restore();
            rstDatabase.Action = RestoreActionType.Database;
            rstDatabase.Database = databaseName;
            BackupDeviceItem bkpDevice = new BackupDeviceItem(backUpFile, DeviceType.File);
            rstDatabase.Devices.Add(bkpDevice);
            rstDatabase.ReplaceDatabase = true;
            rstDatabase.RelocateFiles.Add(new RelocateFile(databaseName,Path.Combine(DataPath, databaseName + ".mdf")));
            rstDatabase.RelocateFiles.Add(new RelocateFile(databaseName + "_log", Path.Combine(DataPath, databaseName + "_log.ldf")));
            rstDatabase.SqlRestore(sqlServer);

            _factory = Fluently.Configure()
                .Database(
                    MsSqlConfiguration
                        .MsSql2008
                        .ConnectionString($@"Server={ServerName};Initial Catalog={databaseName};Trusted_Connection=True;")
                        .ShowSql())
                .Mappings(m => m.FluentMappings.AddFromAssemblyOf<Document>())
                .ExposeConfiguration(cfg => new SchemaExport(cfg).Create(true, false))
                .BuildSessionFactory();
        }

        public static bool DoesDatabaseExist(string databaseName)
        {
            using (var connection = new SqlConnection($"server={ServerName};Trusted_Connection=yes"))
            {
                using (var command = new SqlCommand($"SELECT db_id('{databaseName}')", connection))
                {
                    connection.Open();
                    return (command.ExecuteScalar() != DBNull.Value);
                }
            }
        }

        public static void DeleteDatabase(string databaseName)
        {
            var connection = new ServerConnection(ServerName);
            Server sqlServer = new Server(connection);
            sqlServer.KillDatabase(databaseName);
        }

        private static string ServerName => ConfigurationManager.AppSettings["serverName"];
        private static string DataPath => ConfigurationManager.AppSettings["dataPath"];
    }
}