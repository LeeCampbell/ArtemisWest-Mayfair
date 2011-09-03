using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using ArtemisWest.PropertyInvestment.Calculator.UI.Input.RentalProperty;
using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.Unity;

namespace ArtemisWest.PropertyInvestment.Calculator
{
    //public sealed class PropertyInvestmentContainerModule : IModule
    //{
    //    private readonly IUnityContainer _container;

    //    public PropertyInvestmentContainerModule(IUnityContainer container)
    //    {
    //        _container = container.CreateChildContainer();
    //    }

    //    #region Implementation of IModule

    //    public void Initialize()
    //    {
    //        _container.RegisterType<IModule, PropertyInvestmentModule>();
    //        _container.RegisterType<IRentalPropertyInputPresenter, RentalPropertyInputPresenter>();
    //    }

    //    #endregion
    //}

    //public sealed class PropertyInvestmentContainerModule : Autofac.Module
    //{
    //    protected override void Load(ContainerBuilder builder)
    //    {
    //        base.Load(builder);

    //        builder.RegisterType<PropertyInvestmentModule>().As<Microsoft.Practices.Prism.Modularity.IModule>();
    //        builder.RegisterType<RentalPropertyInputPresenter>().As <IRentalPropertyInputPresenter>();


    //        /* From http://code.google.com/p/autofac/wiki/PrismIntegration
    //         * 
    //        builder.SetDefaultOwnership(Autofac.InstanceOwnership.External);

    //        builder.Register<EntLibLoggerAdapter>().As<ILoggerFacade>();

    //        builder.Register(c => new StaticModuleEnumerator()
    //                .AddModule(typeof(NewsModule))
    //                .AddModule(typeof(MarketModule))
    //                .AddModule(typeof(WatchModule), "MarketModule")
    //                .AddModule(typeof(PositionModule), "MarketModule", "NewsModule"))
    //            .As<IModuleEnumerator>();

    //        builder.Register(c => c.Resolve<Shell>()).As<IShellView>();

    //        builder.Register<ShellPresenter>();

    //        builder.RegisterTypesMatching(
    //            t => t.Name.Contains("Module")
    //                || t.Name.Contains("View")
    //                || t.Name.Contains("Proxy"))
    //            .ExternallyOwned();*/
    //    }
    //}
}
