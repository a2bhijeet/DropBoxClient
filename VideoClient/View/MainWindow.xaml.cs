using System.Windows;
using VideoBackend;
using VideoClient.Viewmodel;

namespace VideoClient.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Constructor    
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel(useHttpClient: true, new VideoUpload()); ;
        }
        #endregion
    }
}
