using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using VideoBackend.Interface;
using VideoBackend.Model;
using VideoModels;

namespace VideoClient.Viewmodel
{
    public class FileVersionViewModel : ViewModelBase
    {
        private IFileOperation _fileOperation;
        private CloudFileMetadata _fileMetadata;
        private string _accessToken;

        private bool _showBusyIndictor;

        public bool ShowBusyIndicator
        {
            get { return _showBusyIndictor; }
            set
            {
                _showBusyIndictor = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<FileRevision> _fileVersions;

        public ObservableCollection<FileRevision> FileVersions
        {
            get { return _fileVersions; }
            set { _fileVersions = value; OnPropertyChanged(); }
        }

        private FileRevision _selectedFileVersion;

        public FileRevision SelectedFileVersion
        {
            get { return _selectedFileVersion; }
            set { _selectedFileVersion = value; OnPropertyChanged(); }
        }

        public FileVersionViewModel(IFileOperation fileOperation, CloudFileMetadata fileMetadata, string accessToken)
        {
            this._fileOperation = fileOperation;
            this._fileMetadata = fileMetadata;
            this._accessToken = accessToken;
            _ = LoadFileVersions();
        }

        public async Task LoadFileVersions()
        {
            ShowBusyIndicator = true;
            try
            {
                var revisions = await _fileOperation.GetFileRevisions(_accessToken, _fileMetadata.PathDisplay);
                FileVersions = new ObservableCollection<FileRevision>(revisions);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Fetch Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                ShowBusyIndicator = false;
            }
        }
    }
}
