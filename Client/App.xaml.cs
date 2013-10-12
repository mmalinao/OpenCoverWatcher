using System.Windows;
using Microsoft.Practices.Unity;

namespace Client
{
    
    public partial class App
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            IUnityContainer container = new UnityContainer();

            container.RegisterType<IWatcherModel, WatcherModel>();
            container.RegisterType<IFolderBrowserDialogWrap, FolderBrowserDialogWrap>();
            container.RegisterType<IWatcherViewModel, WatcherViewModel>();

            var window = container.Resolve<MainWindow>();
            Current.MainWindow = window;
            window.Show();
        }

    }
}
