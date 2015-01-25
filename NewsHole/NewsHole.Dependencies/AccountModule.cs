using NewsHole.Account.Infrastructure;
using NewsHole.Account.Services;
using Ninject.Modules;

namespace NewsHole.Dependencies
{
    public class AccountModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IAccountService>().To<AccountService>();
            Bind<ICrypt>().To<BCryptWrapper>();
        }
    }
}
