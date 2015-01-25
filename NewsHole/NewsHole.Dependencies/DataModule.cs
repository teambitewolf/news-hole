using NewsHole.Data.Infrastructure;
using NewsHole.Data.Repositories;
using NHibernate;
using Ninject.Modules;

namespace NewsHole.Dependencies
{
    public class DataModule : NinjectModule
    {
        public override void Load()
        {
            Bind<ISession>().ToMethod(ctx => SessionProvider.SessionFactory.OpenSession());
            Bind<IUserRepository>().To<UserRepository>();
            Bind(typeof(IRepository<,>)).To(typeof(Repository<,>));
        }
    }
}
