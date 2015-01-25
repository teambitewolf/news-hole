using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate;
using System.Configuration;
using System.Reflection;

namespace NewsHole.Data.Infrastructure
{
    public class SessionProvider
    {
        private static ISessionFactory _sessionFactory;
        private static NHibernate.Cfg.Configuration _config;

        public static ISessionFactory SessionFactory
        {
            get
            {
                var assemblyName = Assembly.GetAssembly(typeof(Entities.User)).FullName;

                if (_sessionFactory == null)
                {
                    _sessionFactory = Fluently.Configure()
                        .Database(MySQLConfiguration.Standard.ConnectionString(ConfigurationManager.ConnectionStrings["mysql"].ConnectionString))
                        .Mappings(m =>
                        {
                            m.FluentMappings.AddFromAssembly(Assembly.GetAssembly(typeof(Entities.User)));
                        })
                        .BuildSessionFactory();
                }
                return _sessionFactory;
            }
        }
    }
}
