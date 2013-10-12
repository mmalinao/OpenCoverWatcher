using Microsoft.Practices.Unity;

namespace Client
{    
    public partial class MainWindow
    {
        
        public MainWindow(IWatcherViewModel vm)
        {
            DataContext = vm;
            InitializeComponent();
        }



    }
}
