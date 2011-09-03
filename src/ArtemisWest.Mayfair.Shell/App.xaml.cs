using System.Windows;

namespace ArtemisWest.Mayfair.Shell
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            // The boostrapper will create the Shell instance, so the App.xaml does not have a StartupUri.
            var bootstrapper = new Bootstrapper();
            bootstrapper.Run();
        }


        //IContainer _container;

        //protected override void OnStartup(StartupEventArgs e)
        //{
        //    base.OnStartup(e);

        //    var builder = new ContainerBuilder();
        //    RegisterStaticTypes(builder);
        //    RegisterAutoFacModules(builder);
        //    ConfigureServiceLocator(builder);
        //    _container = builder.Build();

        //    SetServiceLocator();
        //    ConfigureRegionAdapterMappings();

        //    var view = _container.Resolve<Shell>();

        //    Current.MainWindow = view;
        //    Current.MainWindow.Show();

        //    var modules = _container.Resolve<IEnumerable<IModule>>();
        //    foreach (var module in modules)
        //    {
        //        module.Initialize();
        //    }

        //    this.ShutdownMode = ShutdownMode.OnMainWindowClose;
        //}

        //private static void RegisterStaticTypes(ContainerBuilder builder)
        //{
        //    builder.RegisterAssemblyTypes(
        //        typeof(Microsoft.Practices.Prism.Bootstrapper).Assembly //Prism types.
        //        )
        //        .AsImplementedInterfaces()
        //        .AsSelf()
        //        .SingleInstance();
        //}

        //private static void RegisterAutoFacModules(ContainerBuilder builder)
        //{
        //    var propertyInvestmentModule = new PropertyInvestmentContainerModule();
        //    builder.RegisterModule(new ShellModule());
        //    builder.RegisterModule(propertyInvestmentModule);
        //}

        //private RegionAdapterMappings ConfigureRegionAdapterMappings()
        //{
        //    var instance = _container.Resolve<RegionAdapterMappings>();
        //    instance.RegisterMapping(typeof(Selector), _container.Resolve<SelectorRegionAdapter>());
        //    instance.RegisterMapping(typeof(ItemsControl), _container.Resolve<ItemsControlRegionAdapter>());
        //    instance.RegisterMapping(typeof(ContentControl), _container.Resolve<ContentControlRegionAdapter>());

        //    return instance;
        //}

        //private static void ConfigureServiceLocator(ContainerBuilder builder)
        //{
        //    builder.Register(c => new AutofacServiceLocator(c.Resolve<IComponentContext>()))
        //        .As<IServiceLocator>()
        //        .SingleInstance();
        //}
        //private void SetServiceLocator()
        //{
        //    ServiceLocator.SetLocatorProvider(() => _container.Resolve<IServiceLocator>());
        //}

        //protected override void OnExit(ExitEventArgs e)
        //{
        //    _container.Dispose();
        //    _container = null;
        //}

    }
}
