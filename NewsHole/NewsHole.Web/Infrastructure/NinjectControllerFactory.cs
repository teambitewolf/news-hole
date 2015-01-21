using System;
using System.Web.Mvc;
using System.Web.Routing;

using Ninject;
using NewsHole.Dependencies;

namespace NewsHole.Web.Infrastructure
{
    public class NinjectControllerFactory : DefaultControllerFactory
    {
        private IKernel _ninjectKernel;

        public NinjectControllerFactory()
        {
            _ninjectKernel = new StandardKernel();
            LoadModules();

            AddBindings();
        }

        protected override IController GetControllerInstance(RequestContext requestContext, Type controllerType)
        {
            return controllerType == null ? null : (IController)_ninjectKernel.Get(controllerType);
        }

        private void LoadModules()
        {
            _ninjectKernel.Load(new AccountModule());
            _ninjectKernel.Load(new DataModule());
            _ninjectKernel.Load(new EmailModule());
        }

        private void AddBindings()
        {
            _ninjectKernel.Bind<IAuthenticationHelper>().To<FormsAuthenticationWrapper>();
        }
    }
}