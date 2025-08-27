using Dropbox.Api;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;
using System.IO;
using System.Net.Http;
using System.Windows;
using System.Windows.Input;
using VideoBackend;
using VideoBackend.Model;

namespace VideoClient.Viewmodel
{
    public class MainViewModel : ViewModelBase
    {
        #region Constants 

        private const string APP_KEY = "eehgx19oildrdpq";
        private const string APP_NAME = "VideoUpload262";
        private const string WEB_API_HOST = "https://localhost:7268";
        private const string DownloadFolder = "C:\\Users\\abhij\\Downloads\\";

        #endregion

        private string strAccessToken = string.Empty;
        private string strAuthenticationURL = string.Empty;
        private DropBoxBase _dropBoxBase;
        private VideoUpload _videoUpload;

        public bool UseHttpClient { get; set; } = false;

        private string _welcomText;

        public string WelcomeText
        {
            get { return _welcomText; }
            set { _welcomText = value; OnPropertyChanged(); }
        }


        private bool _showBusyIndicator;

        public bool ShowBusyIndicator
        {
            get { return _showBusyIndicator; }
            set
            {
                _showBusyIndicator = value;
                OnPropertyChanged();
            }
        }

        private string _busyText;

        public string BusyText
        {
            get { return _busyText; }
            set { _busyText = value; OnPropertyChanged(); }
        }


        private bool isAuthenticated = false;

        public bool IsAuthenticated
        {
            get { return isAuthenticated; }
            set { isAuthenticated = value; OnPropertyChanged(); }
        }


        private ObservableCollection<CloudFile> _cloudFilesCollection;

        public ObservableCollection<CloudFile> CloudFilesCollection
        {
            get { return _cloudFilesCollection; }
            set
            {
                _cloudFilesCollection = value;
                OnPropertyChanged();
            }
        }

        private CloudFile? _selectedItem = null;

        public CloudFile? SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                _selectedItem = value;
                OnPropertyChanged();
            }
        }

        #region Commands

        public ICommand AuthenticateCommand { get; private set; }

        public ICommand UploadCommand { get; private set; }

        public ICommand DownloadCommand { get; private set; }

        public ICommand DeleteCommand { get; private set; }

        #endregion


        public MainViewModel(bool useHttpClient, VideoUpload videoUpload)
        {
            UseHttpClient = useHttpClient;
            _videoUpload = videoUpload;
            _dropBoxBase = new DropBoxBase(APP_KEY, APP_NAME);

            CloudFilesCollection = new ObservableCollection<CloudFile>();
            AuthenticateCommand = new RelayCommand(DoAuthenticate);
            UploadCommand = new RelayCommand(DoUpload);
            DownloadCommand = new RelayCommand(DoDownload);
            DeleteCommand = new RelayCommand(DoDelete);

            BusyText = "Please Wait...";
        }

        private async void DoDelete()
        {
            ShowBusyIndicator = true;
            BusyText = "Deleting...";
            try
            {
                if (SelectedItem == null || string.IsNullOrWhiteSpace(SelectedItem.PathDisplay))
                {
                    MessageBox.Show("Please select a file to delete.", "No File Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (UseHttpClient)
                {
                    using (var client = new HttpClient())
                    {
                        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", strAccessToken);
                        var response = await client.GetAsync($"{WEB_API_HOST}/api/v1/files/delete?filePath={SelectedItem.PathDisplay}");

                        if (response.IsSuccessStatusCode)
                        {
                            MessageBox.Show("File deleted successfully.", "File Deleted", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show("File deletion failed.", "File Deletion Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
                else
                {
                    await _videoUpload.Delete(strAccessToken, SelectedItem.PathDisplay);
                    MessageBox.Show("File deleted successfully.", "File Deleted", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                var files = await GetCloudFiles(strAccessToken);
                CloudFilesCollection = new ObservableCollection<CloudFile>(files);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "File delete Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                ShowBusyIndicator = false;
                BusyText = "Please Wait...";
            }

        }

        private async void DoDownload()
        {
            ShowBusyIndicator = true;
            BusyText = "Downloading...";
            try
            {
                if (SelectedItem == null || string.IsNullOrWhiteSpace(SelectedItem.PathDisplay))
                {
                    MessageBox.Show("Please select a file to delete.", "No File Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                await _videoUpload.Download(strAccessToken, SelectedItem.PathDisplay, DownloadFolder + SelectedItem.Name);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Download Failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                ShowBusyIndicator = false;
                BusyText = "Please Wait...";
            }
        }

        public async void DoUpload()
        {
            try
            {
                var openFileDialog = new Microsoft.Win32.OpenFileDialog();
                openFileDialog.Filter = "MP4 Files (*.mp4)|*.mp4|All Files (*.*)|*.*";
                openFileDialog.FilterIndex = 1; // Set the default filter to MP4
                openFileDialog.RestoreDirectory = true;
                var result = openFileDialog.ShowDialog();

                if (result == false || openFileDialog == null || string.IsNullOrWhiteSpace(openFileDialog.FileName))
                {
                    return;
                }

                ShowBusyIndicator = true;
                BusyText = "Uploading...";

                var fileName = Path.GetFileName(openFileDialog.FileName);

                var useHttpClient = false;

                if (useHttpClient)
                {
                    using (var client = new HttpClient())
                    {
                        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", strAccessToken);
                        var response = await client.GetAsync($"{WEB_API_HOST}/api/v1/files/get-upload-url?fileName={fileName}");

                        if (response.IsSuccessStatusCode)
                        {
                            string jsonResponse = await response.Content.ReadAsStringAsync();
                            if (!string.IsNullOrWhiteSpace(jsonResponse))
                            {
                                JObject jsonObject = JObject.Parse(jsonResponse);

                                string presignedUrl = (string)jsonObject["url"];

                                if (string.IsNullOrWhiteSpace(presignedUrl))
                                {
                                    MessageBox.Show("Upload failed. Presigned url is null", "Upload Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                                }
                            }
                        }
                    }
                }
                else
                {
                    await _videoUpload.UploadLargeFile(strAccessToken, openFileDialog.FileName, "/" + fileName);
                    var files = await GetCloudFiles(strAccessToken);
                    CloudFilesCollection = new ObservableCollection<CloudFile>(files);

                    MessageBox.Show("File uploaded successfully.", "Upload Successful", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Upload Failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                ShowBusyIndicator = false;
                BusyText = "Please Wait...";
            }
        }

        public async void DoAuthenticate()
        {
            try
            {
                ShowBusyIndicator = true;
                BusyText = "Authenticating...";

                if (string.IsNullOrEmpty(APP_KEY))
                {
                    MessageBox.Show("Please enter valid App Key !");
                    return;
                }

                _dropBoxBase = new DropBoxBase(APP_KEY, APP_NAME);

                strAuthenticationURL = _dropBoxBase.GeneratedAuthenticationURL(); // This method must be executed before generating Access Token.    
                strAccessToken = _dropBoxBase.GenerateAccessToken();

                if (string.IsNullOrWhiteSpace(strAccessToken))
                {
                    MessageBox.Show("Authentication failed. Please try again !");
                    return;
                }

                var user = await _videoUpload.GetCurrentAccountAsync(strAccessToken);

                var files = await GetCloudFiles(strAccessToken);
                CloudFilesCollection = new ObservableCollection<CloudFile>(files);
                WelcomeText = $"Welcome {user.DisplayName} ({user.Email})";
                MessageBox.Show("Authentication complete");

                IsAuthenticated = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Authentication Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                ShowBusyIndicator = false;
                BusyText = "Please Wait...";
            }
        }

        private async Task<IList<CloudFile>> GetCloudFiles(string accessToken)
        {
            IList<CloudFile> files = new List<CloudFile>();
            try
            {
                if (UseHttpClient)
                {
                    using (var client = new HttpClient())
                    {
                        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
                        var response = await client.GetAsync($"{WEB_API_HOST}/api/v1/files");

                        if (response.IsSuccessStatusCode)
                        {
                            string jsonResponse = await response.Content.ReadAsStringAsync();
                            if (!string.IsNullOrWhiteSpace(jsonResponse))
                            {
                                files = JsonConvert.DeserializeObject<List<CloudFile>>(jsonResponse);
                            }
                        }
                    }
                }
                else
                {
                    files = await _videoUpload.GetAllFiles(accessToken);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "File Fetch Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return files ?? new List<CloudFile>();
        }
    }
}
