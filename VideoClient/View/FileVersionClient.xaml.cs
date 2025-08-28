using System.Windows;
using VideoBackend;
using VideoBackend.Interface;
using VideoBackend.Model;

namespace VideoClient.View
{
    /// <summary>
    /// Interaction logic for FileVersion.xaml
    /// </summary>
    public partial class FileVersionClient : Window
    {
        public FileVersionClient(IFileOperation fileOperation, CloudFileMetadata fileMetadata, string accessToken)
        {
            InitializeComponent();
            DataContext = new Viewmodel.FileVersionViewModel(fileOperation, fileMetadata, accessToken);
        }
    }
}
