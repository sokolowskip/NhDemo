using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
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
                        .ConnectionString(@"Server=.\SQLEXPRESS;Initial Catalog=NhDemo;Trusted_Connection=True;")
                        .ShowSql())
                .Mappings(m => m.FluentMappings.AddFromAssemblyOf<Document>())
                .ExposeConfiguration(cfg => new SchemaExport(cfg).Create(true, true))
                .BuildSessionFactory();

            return sessionFactory;
        }
    }
}